using EmployeeSupportAgent.API.Models;

namespace EmployeeSupportAgent.API.Repositories;

public interface IChatMessageRepository : IRepository<ChatMessage>
{
    Task<IReadOnlyList<ChatMessage>> GetForEmployeeAsync(int employeeId);
    Task<int> ClearForEmployeeAsync(int employeeId);
}