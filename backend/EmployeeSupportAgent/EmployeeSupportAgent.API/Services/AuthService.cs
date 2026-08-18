using EmployeeSupportAgent.API.Infrastructure;
using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Repositories;

namespace EmployeeSupportAgent.API.Services;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly IEmployeeRepository _employees;

    public AuthService(IUserRepository users, IEmployeeRepository employees)
    {
        _users = users;
        _employees = employees;
    }

    public Task<Employee?> GetEmployeeAsync(int employeeId) =>
        _employees.GetByIdAsync(employeeId);

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var user = await _users.GetByUsernameAsync(username);
        if (user == null) return null;

        // Disabled users cannot log in.
        if (!user.IsActive) return null;

        // BCrypt.Verify is safe against timing attacks.
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        if (newPassword.Length < 8)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["newPassword"] = new[] { "New password must be at least 8 characters." }
            });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 11);
        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
        return true;
    }

    public async Task<bool> AdminResetPasswordAsync(int userId, string newPassword)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (newPassword.Length < 8)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["newPassword"] = new[] { "New password must be at least 8 characters." }
            });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 11);
        user.UpdatedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
        return true;
    }

    public async Task MarkLoggedInAsync(int userId)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user == null) return;
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
    }

    public async Task SetActiveAsync(int userId, bool isActive)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
    }

    public async Task<User> CreateUserAsync(int employeeId, string username, string password, bool mustChangePassword)
    {
        var existing = await _users.GetByUsernameAsync(username);
        if (existing != null)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["username"] = new[] { "Username already exists." }
            });

        var usersForEmployee = await _users.ListAsync(u => u.EmployeeId == employeeId);
        if (usersForEmployee.Count > 0)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["employeeId"] = new[] { "This employee already has a login." }
            });

        var now = DateTime.UtcNow;
        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11),
            EmployeeId = employeeId,
            IsActive = true,
            MustChangePassword = mustChangePassword,
            CreatedAt = now,
            UpdatedAt = now
        };
        await _users.AddAsync(user);
        return user;
    }
}