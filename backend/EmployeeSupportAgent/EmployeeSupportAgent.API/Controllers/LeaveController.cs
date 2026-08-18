using System.Security.Claims;
using EmployeeSupportAgent.API.Data;
using EmployeeSupportAgent.API.Dtos;
using EmployeeSupportAgent.API.Infrastructure;
using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Repositories;
using EmployeeSupportAgent.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSupportAgent.API.Controllers;

[Authorize]
[ApiController]
[Route("api/leave")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveRepository _leaves;
    private readonly IEmployeeRepository _employees;
    private readonly LeaveService _leaveService;
    private readonly AppDbContext _db;

    public LeaveController(
        ILeaveRepository leaves,
        IEmployeeRepository employees,
        LeaveService leaveService,
        AppDbContext db)
    {
        _leaves = leaves;
        _employees = employees;
        _leaveService = leaveService;
        _db = db;
    }

    private int CurrentEmployeeId() => int.Parse(
        User.FindFirstValue("EmployeeId")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? "0");

    private bool IsApprover() => User.IsInRole("Manager") || User.IsInRole("HR") || User.IsInRole("Admin");

    [HttpPost("apply")]
    public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeaveDto dto)
    {
        if (dto.EndDate < dto.StartDate)
            return BadRequest(new { error = "End date must be on or after start date." });

        var emp = await _employees.GetByIdAsync(dto.EmployeeId);
        if (emp == null) return NotFound(new { error = "Employee not found." });

        var days = (int)Math.Ceiling((dto.EndDate - dto.StartDate).TotalDays) + 1;
        if (days > emp.LeaveBalance)
            return BadRequest(new { error = $"Insufficient leave balance. You have {emp.LeaveBalance} day(s), need {days}." });

        var leave = await _leaveService.ApplyAsync(dto.EmployeeId, dto.StartDate, dto.EndDate, dto.Reason);
        return Ok(leave);
    }

    [HttpGet("history")]
    public async Task<IActionResult> MyHistory()
    {
        var id = CurrentEmployeeId();
        var leaves = await _leaves.GetHistoryForEmployeeAsync(id);
        return Ok(leaves);
    }

    [HttpGet("history/{employeeId}")]
    public async Task<IActionResult> History(int employeeId)
    {
        if (employeeId != CurrentEmployeeId() && !IsApprover())
            return Forbid();
        var leaves = await _leaves.GetHistoryForEmployeeAsync(employeeId);
        return Ok(leaves);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> Pending()
    {
        if (!IsApprover()) return Forbid();
        var leaves = await _leaves.GetPendingAsync();
        return Ok(leaves);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var leave = await _leaves.GetByIdAsync(id);
        if (leave == null) return NotFound();
        if (leave.EmployeeId != CurrentEmployeeId() && !IsApprover()) return Forbid();
        return Ok(leave);
    }

    [HttpPut("approve")]
    public async Task<IActionResult> Approve([FromBody] LeaveApprovalDto dto)
    {
        if (!IsApprover()) return Forbid();
        var leave = await _leaves.GetByIdAsync(dto.LeaveId);
        if (leave == null) return NotFound();
        if (leave.Status != "Pending")
            return BadRequest(new { error = $"Leave is already {leave.Status}." });

        var emp = await _employees.GetByIdAsync(leave.EmployeeId);
        if (emp == null) return NotFound(new { error = "Employee not found." });

        var days = (int)Math.Ceiling((leave.EndDate - leave.StartDate).TotalDays) + 1;
        if (days > emp.LeaveBalance)
            return BadRequest(new { error = $"Insufficient balance to approve ({emp.LeaveBalance} available, {days} required)." });

        var now = DateTime.UtcNow;
        leave.Status = "Approved";
        leave.ApprovedBy = dto.ManagerId;
        leave.ApprovedDate = now;
        leave.Remarks = dto.Remarks;
        leave.UpdatedAt = now;
        emp.LeaveBalance -= days;
        emp.UpdatedAt = now;

        // Atomic: both rows commit together or roll back.
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            await _leaves.UpdateAsync(leave);
            await _employees.UpdateAsync(emp);
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
        return Ok(leave);
    }

    [HttpPut("reject")]
    public async Task<IActionResult> Reject([FromBody] LeaveApprovalDto dto)
    {
        if (!IsApprover()) return Forbid();
        var leave = await _leaves.GetByIdAsync(dto.LeaveId);
        if (leave == null) return NotFound();
        leave.Status = "Rejected";
        leave.ApprovedBy = dto.ManagerId;
        leave.ApprovedDate = DateTime.UtcNow;
        leave.Remarks = dto.Remarks;
        leave.UpdatedAt = DateTime.UtcNow;
        await _leaves.UpdateAsync(leave);
        return Ok(leave);
    }
}
