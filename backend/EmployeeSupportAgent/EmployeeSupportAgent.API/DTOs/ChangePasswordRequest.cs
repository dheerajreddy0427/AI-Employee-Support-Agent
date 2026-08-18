using System.ComponentModel.DataAnnotations;

namespace EmployeeSupportAgent.API.Dtos;

public class ChangePasswordRequest
{
    [Required, MinLength(1)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
