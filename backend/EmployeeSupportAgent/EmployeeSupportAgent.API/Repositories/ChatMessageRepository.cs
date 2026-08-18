using EmployeeSupportAgent.API.Data;
using EmployeeSupportAgent.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSupportAgent.API.Repositories;

public class ChatMessageRepository : EFRepository<ChatMessage>, IChatMessageRepository
{
    public ChatMessageRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<ChatMessage>> GetForEmployeeAsync(int employeeId)
    {
        return await _db.ChatMessages
            .Where(m => m.EmployeeId == employeeId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> ClearForEmployeeAsync(int employeeId)
    {
        var rows = _db.ChatMessages.Where(m => m.EmployeeId == employeeId);
        _db.ChatMessages.RemoveRange(rows);
        return await _db.SaveChangesAsync();
    }
}