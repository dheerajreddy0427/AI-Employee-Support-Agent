using EmployeeSupportAgent.API.Data;
using EmployeeSupportAgent.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSupportAgent.API.Repositories;

public class ReimbursementRepository : EFRepository<Reimbursement>, IReimbursementRepository
{
    public ReimbursementRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Reimbursement>> GetPendingAsync()
    {
        return await _db.Reimbursements
            .Where(r => r.Status == "Pending")
            .OrderByDescending(r => r.SubmittedDate)
            .ToListAsync();
    }
}