using EmployeeSupportAgent.API.Data;
using EmployeeSupportAgent.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSupportAgent.API.Repositories;

public class UserRepository : EFRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public Task<User?> GetByUsernameAsync(string username) =>
        _db.Users.FirstOrDefaultAsync(u => u.Username == username);
}