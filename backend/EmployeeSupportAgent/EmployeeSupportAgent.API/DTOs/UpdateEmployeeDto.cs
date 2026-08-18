namespace EmployeeSupportAgent.API.Dtos;

/// <summary>
/// Fields a user can edit about themselves (FullName/Email/Department).
/// LeaveBalance/Role/EmployeeCode are admin-only and may be set on the same
/// payload but will be ignored unless the caller is HR/Admin.
/// </summary>
public class UpdateEmployeeDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Department { get; set; }
    public int? LeaveBalance { get; set; }
    public string? Role { get; set; }
    public string? EmployeeCode { get; set; }
}
