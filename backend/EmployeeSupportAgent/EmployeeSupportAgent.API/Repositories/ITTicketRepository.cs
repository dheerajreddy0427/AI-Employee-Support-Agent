using EmployeeSupportAgent.API.Data;
using EmployeeSupportAgent.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSupportAgent.API.Repositories;

public class ITTicketRepository : EFRepository<ITTicket>, IITTicketRepository
{
    public ITTicketRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<ITTicket>> GetAllOrderedAsync()
    {
        return await _db.ITTickets
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ITTicket>> GetByStatusAsync(string status)
    {
        return await _db.ITTickets
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();
    }
}