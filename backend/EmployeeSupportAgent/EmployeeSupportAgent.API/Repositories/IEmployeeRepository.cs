using EmployeeSupportAgent.API.Models;

namespace EmployeeSupportAgent.API.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByEmployeeCodeAsync(string code);
    Task<Employee?> GetByUsernameAsync(string username);
}
