using System.Security.Claims;
using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Repositories;
using EmployeeSupportAgent.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeSupportAgent.API.Controllers;

[Authorize]
[ApiController]
[Route("api/payslips")]
public class PayslipController : ControllerBase
{
    private readonly PayslipService _payslipService;

    public PayslipController(PayslipService payslipService)
    {
        _payslipService = payslipService;
    }

    private int CurrentEmployeeId() => int.Parse(
        User.FindFirstValue("EmployeeId")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? "0");

    private bool IsHrOrAdmin() => User.IsInRole("HR") || User.IsInRole("Admin");

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        if (employeeId != CurrentEmployeeId() && !IsHrOrAdmin()) return Forbid();
        var payslips = await _payslipService.GetForEmployeeAsync(employeeId);
        return Ok(payslips);
    }

    [HttpGet("me")]
    public Task<IActionResult> Mine() => GetByEmployee(CurrentEmployeeId());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var payslip = await _payslipService.GetByIdAsync(id);
        if (payslip == null) return NotFound();
        if (payslip.EmployeeId != CurrentEmployeeId() && !IsHrOrAdmin()) return Forbid();
        return Ok(payslip);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Payslip payslip)
    {
        if (!IsHrOrAdmin()) return Forbid();
        var created = await _payslipService.CreateAsync(payslip);
        return Ok(created);
    }
}