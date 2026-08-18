using System.Security.Claims;
using EmployeeSupportAgent.API.Controllers;
using EmployeeSupportAgent.API.Dtos;
using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeSupportAgent.API.Controllers;

[Authorize]
[ApiController]
[Route("api/employees")]
public class EmployeeController : ControllerBase
{
    private readonly EmployeeService _employees;

    public EmployeeController(EmployeeService employees)
    {
        _employees = employees;
    }

    private int CurrentEmployeeId() => int.Parse(
        User.FindFirstValue("EmployeeId")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? "0");

    private bool IsHrOrAdmin() => User.IsInRole("HR") || User.IsInRole("Admin");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        if (!IsHrOrAdmin()) return Forbid();
        var all = await _employees.GetAllEmployeesAsync();
        var users = HttpContext.RequestServices
            .GetService(typeof(Repositories.IUserRepository)) as Repositories.IUserRepository;
        var userByEmp = (await users!.ListAsync())
            .ToDictionary(u => u.EmployeeId, u => u.Username);
        return Ok(all.Select(e => AuthController.ToDto(
            e,
            new User { EmployeeId = e.Id, Username = userByEmp.TryGetValue(e.Id, out var u) ? u : "" },
            includeSensitive: true)));
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var emp = await _employees.GetEmployeeByIdAsync(CurrentEmployeeId());
        if (emp == null) return NotFound();
        var user = await GetCurrentUserAsync();
        return Ok(AuthController.ToDto(emp, user, includeSensitive: true));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> Get(int id)
    {
        if (id != CurrentEmployeeId() && !IsHrOrAdmin()) return Forbid();
        var emp = await _employees.GetEmployeeByIdAsync(id);
        if (emp == null) return NotFound();
        var user = await GetUserForEmployeeAsync(id);
        return Ok(AuthController.ToDto(emp, user, includeSensitive: id == CurrentEmployeeId() || IsHrOrAdmin()));
    }

    [HttpGet("{id}/leave-balance")]
    public async Task<IActionResult> GetLeaveBalance(int id)
    {
        if (id != CurrentEmployeeId() && !IsHrOrAdmin()) return Forbid();
        var emp = await _employees.GetEmployeeByIdAsync(id);
        if (emp == null) return NotFound();
        return Ok(new { employeeId = emp.Id, fullName = emp.FullName, leaveBalance = emp.LeaveBalance });
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] Employee employee)
    {
        if (!IsHrOrAdmin()) return Forbid();
        if (string.IsNullOrWhiteSpace(employee.FullName)
            || string.IsNullOrWhiteSpace(employee.Email)
            || string.IsNullOrWhiteSpace(employee.EmployeeCode)
            || string.IsNullOrWhiteSpace(employee.Role))
        {
            return BadRequest(new { error = "FullName, Email, EmployeeCode and Role are required." });
        }
        var created = await _employees.CreateEmployeeAsync(employee);
        var user = await GetUserForEmployeeAsync(created.Id);
        return CreatedAtAction(nameof(Get), new { id = created.Id },
            AuthController.ToDto(created, user, includeSensitive: true));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var emp = await _employees.GetEmployeeByIdAsync(id);
        if (emp == null) return NotFound();

        var isSelf = id == CurrentEmployeeId();
        var isAdmin = IsHrOrAdmin();
        if (!isSelf && !isAdmin) return Forbid();

        // Self-editable fields
        if (dto.FullName != null) emp.FullName = dto.FullName;
        if (dto.Email != null) emp.Email = dto.Email;
        if (dto.Department != null) emp.Department = dto.Department;

        // Admin-only fields
        if (isAdmin)
        {
            if (dto.LeaveBalance.HasValue) emp.LeaveBalance = dto.LeaveBalance.Value;
            if (dto.Role != null) emp.Role = dto.Role;
            if (dto.EmployeeCode != null) emp.EmployeeCode = dto.EmployeeCode;
        }

        emp.UpdatedAt = DateTime.UtcNow;
        var updated = await _employees.UpdateEmployeeAsync(id, emp);
        var user = await GetUserForEmployeeAsync(id);
        return Ok(AuthController.ToDto(updated!, user, includeSensitive: true));
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        return await GetUserForEmployeeAsync(CurrentEmployeeId());
    }

    private async Task<User?> GetUserForEmployeeAsync(int employeeId)
    {
        var users = HttpContext.RequestServices
            .GetService(typeof(Repositories.IUserRepository)) as Repositories.IUserRepository;
        var list = await users!.ListAsync(u => u.EmployeeId == employeeId);
        return list.FirstOrDefault();
    }
}
