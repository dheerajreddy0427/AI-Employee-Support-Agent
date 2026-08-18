using System.Security.Claims;
using EmployeeSupportAgent.API.Dtos;
using EmployeeSupportAgent.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeSupportAgent.API.Controllers;

[Authorize]
[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly AgentService _agent;

    public AgentController(AgentService agent)
    {
        _agent = agent;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<AgentResponseDto>> Chat([FromBody] ChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new AgentResponseDto { Reply = "Message cannot be empty.", Intent = "Fallback" });

        var employeeId = int.Parse(User.FindFirstValue("EmployeeId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "0");

        if (employeeId == 0)
            return Unauthorized();

        var response = await _agent.AskAsync(employeeId, request.Message, ct);
        return Ok(response);
    }
}
