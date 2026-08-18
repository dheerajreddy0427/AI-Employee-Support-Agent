using EmployeeSupportAgent.API.Services;
using Microsoft.SemanticKernel;

namespace EmployeeSupportAgent.API.Plugins;

public class EmployeePlugin
{
    private readonly EmployeeService _employeeService;

    public EmployeePlugin(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [KernelFunction]
    public string GetEmployeeName(int employeeId)
    {
        var employee = _employeeService.GetEmployeeByIdAsync(employeeId).GetAwaiter().GetResult();
        return employee?.FullName ?? "Employee not found";
    }

    [KernelFunction]
    public int GetLeaveBalance(int employeeId)
    {
        var employee = _employeeService.GetEmployeeByIdAsync(employeeId).GetAwaiter().GetResult();
        return employee?.LeaveBalance ?? 0;
    }

    [KernelFunction]
    public string GetDepartment(int employeeId)
    {
        var employee = _employeeService.GetEmployeeByIdAsync(employeeId).GetAwaiter().GetResult();
        return employee?.Department ?? "Unknown";
    }
}
