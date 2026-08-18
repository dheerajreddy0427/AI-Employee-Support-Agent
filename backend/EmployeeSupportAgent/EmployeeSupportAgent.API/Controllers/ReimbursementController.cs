using System.Security.Claims;
using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Repositories;
using EmployeeSupportAgent.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeSupportAgent.API.Controllers;

[Authorize]
[ApiController]
[Route("api/reimbursements")]
public class ReimbursementController : ControllerBase
{
    private readonly ReimbursementService _reimbursementService;
    private readonly IReimbursementRepository _reimbursements;

    public ReimbursementController(ReimbursementService reimbursementService, IReimbursementRepository reimbursements)
    {
        _reimbursementService = reimbursementService;
        _reimbursements = reimbursements;
    }

    private int CurrentEmployeeId() => int.Parse(
        User.FindFirstValue("EmployeeId")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? "0");

    private bool IsHrOrAdmin() => User.IsInRole("HR") || User.IsInRole("Admin");

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var all = await _reimbursementService.GetAllAsync();
        if (IsHrOrAdmin())
            return Ok(all.OrderByDescending(r => r.SubmittedDate));
        return Ok(all
            .Where(r => r.EmployeeId == CurrentEmployeeId())
            .OrderByDescending(r => r.SubmittedDate));
    }

    [HttpGet("pending")]
    public async Task<IActionResult> Pending()
    {
        if (!IsHrOrAdmin()) return Forbid();
        var items = await _reimbursements.GetPendingAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var r = await _reimbursementService.GetByIdAsync(id);
        if (r == null) return NotFound();
        if (r.EmployeeId != CurrentEmployeeId() && !IsHrOrAdmin()) return Forbid();
        return Ok(r);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Reimbursement request)
    {
        request.EmployeeId = CurrentEmployeeId();
        var created = await _reimbursementService.CreateAsync(request);
        return Ok(created);
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        if (!IsHrOrAdmin()) return Forbid();
        var r = await _reimbursementService.GetByIdAsync(id);
        if (r == null) return NotFound();
        r.Status = "Approved";
        r.ApprovedBy = CurrentEmployeeId();
        r.ApprovedDate = DateTime.UtcNow;
        r.UpdatedAt = DateTime.UtcNow;
        await _reimbursements.UpdateAsync(r);
        return Ok(r);
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        if (!IsHrOrAdmin()) return Forbid();
        var r = await _reimbursementService.GetByIdAsync(id);
        if (r == null) return NotFound();
        r.Status = "Rejected";
        r.ApprovedBy = CurrentEmployeeId();
        r.ApprovedDate = DateTime.UtcNow;
        r.UpdatedAt = DateTime.UtcNow;
        await _reimbursements.UpdateAsync(r);
        return Ok(r);
    }
}