using EmployeeSupportAgent.API.Models;

namespace EmployeeSupportAgent.API.Repositories;

public interface ILeaveRepository : IRepository<LeaveRequest>
{
    Task<IReadOnlyList<LeaveRequest>> GetPendingAsync();
    Task<IReadOnlyList<LeaveRequest>> GetHistoryForEmployeeAsync(int employeeId);
}
