using EmployeeSupportAgent.API.Data;
using EmployeeSupportAgent.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSupportAgent.API.Repositories;

public class EmployeeRepository : EFRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext db) : base(db) { }

    public Task<Employee?> GetByEmployeeCodeAsync(string code) =>
        _db.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == code);

    public Task<Employee?> GetByUsernameAsync(string username) =>
        _db.Employees.FirstOrDefaultAsync(e => e.Email == username);
}
