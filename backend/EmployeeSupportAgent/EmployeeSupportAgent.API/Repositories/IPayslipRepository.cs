using EmployeeSupportAgent.API.Models;

namespace EmployeeSupportAgent.API.Repositories;

public interface IPayslipRepository : IRepository<Payslip>
{
    Task<IReadOnlyList<Payslip>> GetForEmployeeAsync(int employeeId);
}