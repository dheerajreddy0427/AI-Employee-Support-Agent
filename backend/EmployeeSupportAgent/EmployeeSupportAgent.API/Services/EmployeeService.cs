using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Repositories;

namespace EmployeeSupportAgent.API.Services;

public class EmployeeService
{
    private readonly IEmployeeRepository _employees;

    public EmployeeService(IEmployeeRepository employees)
    {
        _employees = employees;
    }

    public Task<IReadOnlyList<Employee>> GetAllEmployeesAsync() => _employees.ListAsync();

    public Task<Employee?> GetEmployeeByIdAsync(int id) => _employees.GetByIdAsync(id);

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        var now = DateTime.UtcNow;
        employee.CreatedAt = now;
        employee.UpdatedAt = now;
        await _employees.AddAsync(employee);
        return employee;
    }

    public async Task<Employee?> UpdateEmployeeAsync(int id, Employee updated)
    {
        var employee = await _employees.GetByIdAsync(id);
        if (employee == null) return null;

        employee.FullName = updated.FullName;
        employee.Email = updated.Email;
        employee.Department = updated.Department;
        employee.LeaveBalance = updated.LeaveBalance;
        employee.UpdatedAt = DateTime.UtcNow;

        await _employees.UpdateAsync(employee);
        return employee;
    }

    public async Task<int> GetLeaveBalanceAsync(int id)
    {
        var employee = await _employees.GetByIdAsync(id);
        return employee?.LeaveBalance ?? 0;
    }
}