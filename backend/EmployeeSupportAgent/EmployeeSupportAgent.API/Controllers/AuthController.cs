using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeSupportAgent.API.Dtos;
using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeSupportAgent.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthController(IConfiguration configuration, AuthService authService)
    {
        _configuration = configuration;
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Username and password are required." });

        var user = await _authService.ValidateUserAsync(request.Username, request.Password);
        if (user == null) return Unauthorized(new { error = "Invalid username or password." });

        var employee = await _authService.GetEmployeeAsync(user.EmployeeId);
        if (employee == null) return Unauthorized(new { error = "Account is not linked to an employee." });

        var token = IssueToken(user, employee);

        return Ok(new LoginResponseDto
        {
            Token = token,
            User = ToDto(employee, user, includeSensitive: true)
        });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (request.NewPassword != request.ConfirmNewPassword)
            return BadRequest(new { error = "New password and confirmation do not match." });

        var userId = int.Parse(User.FindFirstValue("EmployeeId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "0");

        // Find the User row for this EmployeeId (User.Id != EmployeeId in this schema)
        var empId = userId;
        var users = HttpContext.RequestServices.GetService(typeof(EmployeeSupportAgent.API.Repositories.IUserRepository))
            as EmployeeSupportAgent.API.Repositories.IUserRepository;
        var userRow = (await users!.ListAsync(u => u.EmployeeId == empId)).FirstOrDefault();
        if (userRow == null) return Unauthorized();

        try
        {
            await _authService.ChangePasswordAsync(userRow.Id, request.CurrentPassword, request.NewPassword);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }

        return NoContent();
    }

    private string IssueToken(User user, Employee employee)
    {
        var claims = new[]
        {
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new System.Security.Claims.Claim("name", user.Username),
            new System.Security.Claims.Claim("EmployeeId", user.EmployeeId.ToString()),
            new System.Security.Claims.Claim("role", employee.Role),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, employee.Role)
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public static UserDto ToDto(Employee employee, User user, bool includeSensitive)
    {
        return new UserDto
        {
            EmployeeId = employee.Id,
            Username = user?.Username ?? string.Empty,
            FullName = employee.FullName,
            Email = employee.Email,
            Department = employee.Department,
            Role = employee.Role,
            LeaveBalance = employee.LeaveBalance,
            EmployeeCode = includeSensitive ? employee.EmployeeCode : null
        };
    }
}
