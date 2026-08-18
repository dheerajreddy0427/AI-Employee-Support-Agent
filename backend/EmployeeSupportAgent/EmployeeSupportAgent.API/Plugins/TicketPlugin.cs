using EmployeeSupportAgent.API.Services;
using Microsoft.SemanticKernel;

namespace EmployeeSupportAgent.API.Plugins;

public class TicketPlugin
{
    private readonly TicketService _ticketService;

    public TicketPlugin(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [KernelFunction]
    public string RaiseTicket(int employeeId, string title, string description)
    {
        var ticket = _ticketService.CreateAsync(employeeId, title, description).GetAwaiter().GetResult();
        return $"Ticket #{ticket.Id} created successfully";
    }
}
