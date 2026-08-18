using EmployeeSupportAgent.API.Data;
using EmployeeSupportAgent.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSupportAgent.API.Repositories;

public class LeaveRepository : EFRepository<LeaveRequest>, ILeaveRepository
{
    public LeaveRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<LeaveRequest>> GetPendingAsync()
    {
        return await _db.LeaveRequests
            .Where(l => l.Status == "Pending")
            .OrderBy(l => l.StartDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<LeaveRequest>> GetHistoryForEmployeeAsync(int employeeId)
    {
        return await _db.LeaveRequests
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }
}
