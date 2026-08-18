using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Repositories;

namespace EmployeeSupportAgent.API.Services;

public class LeaveService
{
    private readonly ILeaveRepository _leaves;

    public LeaveService(ILeaveRepository leaves)
    {
        _leaves = leaves;
    }

    public async Task<LeaveRequest> ApplyAsync(int employeeId, DateTime startDate, DateTime endDate, string reason)
    {
        var now = DateTime.UtcNow;
        var leave = new LeaveRequest
        {
            EmployeeId = employeeId,
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason,
            Status = "Pending",
            CreatedAt = now,
            UpdatedAt = now
        };

        await _leaves.AddAsync(leave);
        return leave;
    }

    public Task<LeaveRequest?> GetByIdAsync(int id) => _leaves.GetByIdAsync(id);
}