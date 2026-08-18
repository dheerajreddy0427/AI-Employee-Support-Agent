using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Repositories;

namespace EmployeeSupportAgent.API.Services;

public class TicketService
{
    private readonly IITTicketRepository _tickets;

    public TicketService(IITTicketRepository tickets)
    {
        _tickets = tickets;
    }

    public async Task<ITTicket> CreateAsync(int employeeId, string title, string description)
    {
        var now = DateTime.UtcNow;
        var ticket = new ITTicket
        {
            EmployeeId = employeeId,
            IssueTitle = title,
            Description = description,
            Status = "Open",
            CreatedDate = now,
            UpdatedAt = now
        };

        await _tickets.AddAsync(ticket);
        return ticket;
    }

    public Task<IReadOnlyList<ITTicket>> GetAllAsync() => _tickets.GetAllOrderedAsync();
    public Task<ITTicket?> GetByIdAsync(int id) => _tickets.GetByIdAsync(id);
}