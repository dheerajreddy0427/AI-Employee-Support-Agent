namespace EmployeeSupportAgent.API.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;

    /// <summary>BCrypt hash of the user's password. Never store plain text.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public int EmployeeId { get; set; }

    /// <summary>Soft-delete flag. Disabled users cannot log in.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Forced password reset on the next login. Cleared by AuthService.ChangePasswordAsync.</summary>
    public bool MustChangePassword { get; set; } = false;

    /// <summary>Server-side timestamp of the last successful login. Informational only.</summary>
    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}