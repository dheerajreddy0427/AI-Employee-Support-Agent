namespace EmployeeSupportAgent.API.Dtos;

/// <summary>Projection of an Employee for safe client consumption.</summary>
public class UserDto
{
    public int EmployeeId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int LeaveBalance { get; set; }

    /// <summary>Visible only to HR/Admin.</summary>
    public string? EmployeeCode { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}
