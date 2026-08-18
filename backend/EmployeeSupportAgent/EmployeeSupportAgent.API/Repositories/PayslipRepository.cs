using EmployeeSupportAgent.API.Data;
using EmployeeSupportAgent.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSupportAgent.API.Repositories;

public class PayslipRepository : EFRepository<Payslip>, IPayslipRepository
{
    public PayslipRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Payslip>> GetForEmployeeAsync(int employeeId)
    {
        return await _db.Payslips
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.UploadedDate)
            .ToListAsync();
    }
}