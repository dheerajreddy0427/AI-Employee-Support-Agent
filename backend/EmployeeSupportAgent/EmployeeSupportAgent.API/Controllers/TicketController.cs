using System.Security.Claims;
using EmployeeSupportAgent.API.Dtos;
using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Repositories;
using EmployeeSupportAgent.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeSupportAgent.API.Controllers;

[Authorize]
[ApiController]
[Route("api/tickets")]
public class TicketController : ControllerBase
{
    private readonly TicketService _ticketService;
    private readonly IITTicketRepository _tickets;

    public TicketController(TicketService ticketService, IITTicketRepository tickets)
    {
        _ticketService = ticketService;
        _tickets = tickets;
    }

    private int CurrentEmployeeId() => int.Parse(
        User.FindFirstValue("EmployeeId")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? "0");

    private bool IsHrOrAdmin() => User.IsInRole("HR") || User.IsInRole("Admin");

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var all = await _ticketService.GetAllAsync();
        if (IsHrOrAdmin()) return Ok(all);
        return Ok(all.Where(t => t.EmployeeId == CurrentEmployeeId()));
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllForAdmin()
    {
        if (!IsHrOrAdmin()) return Forbid();
        return Ok(await _ticketService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var ticket = await _tickets.GetByIdAsync(id);
        if (ticket == null) return NotFound();
        if (ticket.EmployeeId != CurrentEmployeeId() && !IsHrOrAdmin()) return Forbid();
        return Ok(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ITTicket request)
    {
        var ticket = await _ticketService.CreateAsync(
            CurrentEmployeeId(),
            request.IssueTitle,
            request.Description);
        return Ok(ticket);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusDto dto)
    {
        if (!IsHrOrAdmin()) return Forbid();
        if (!TicketStatuses.IsValid(dto.Status))
            return BadRequest(new { error = $"Invalid status '{dto.Status}'. Allowed: {string.Join(", ", TicketStatuses.All)}." });

        var ticket = await _tickets.GetByIdAsync(id);
        if (ticket == null) return NotFound();

        ticket.Status = dto.Status;
        ticket.AssignedToId = dto.AssignedToId ?? ticket.AssignedToId;
        ticket.UpdatedAt = DateTime.UtcNow;
        if (dto.Status == TicketStatuses.Resolved) ticket.ResolvedDate = DateTime.UtcNow;
        await _tickets.UpdateAsync(ticket);
        return Ok(ticket);
    }

    [HttpPut("{id}/close")]
    public async Task<IActionResult> Close(int id)
    {
        var ticket = await _tickets.GetByIdAsync(id);
        if (ticket == null) return NotFound();
        if (ticket.EmployeeId != CurrentEmployeeId() && !IsHrOrAdmin()) return Forbid();
        ticket.Status = TicketStatuses.Closed;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _tickets.UpdateAsync(ticket);
        return Ok(ticket);
    }
}