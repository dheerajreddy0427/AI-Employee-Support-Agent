using EmployeeSupportAgent.API.Models;

namespace EmployeeSupportAgent.API.Repositories;

public interface IITTicketRepository : IRepository<ITTicket>
{
    Task<IReadOnlyList<ITTicket>> GetAllOrderedAsync();
    Task<IReadOnlyList<ITTicket>> GetByStatusAsync(string status);
}
