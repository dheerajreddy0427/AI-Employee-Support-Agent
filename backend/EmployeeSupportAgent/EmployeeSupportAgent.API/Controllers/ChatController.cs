using System.Security.Claims;
using EmployeeSupportAgent.API.Data;
using EmployeeSupportAgent.API.Dtos;
using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeSupportAgent.API.Controllers;

[Authorize]
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatMessageRepository _messages;

    public ChatController(IChatMessageRepository messages)
    {
        _messages = messages;
    }

    private int CurrentEmployeeId() => int.Parse(
        User.FindFirstValue("EmployeeId")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? "0");

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var id = CurrentEmployeeId();
        var chats = await _messages.GetForEmployeeAsync(id);
        return Ok(chats);
    }

    [HttpPost]
    public async Task<IActionResult> SaveMessage([FromBody] ChatMessageDto dto)
    {
        var id = CurrentEmployeeId();
        if (dto.EmployeeId != 0 && dto.EmployeeId != id)
            return Forbid();

        var chat = new ChatMessage
        {
            EmployeeId = id,
            Sender = dto.Sender,
            MessageText = dto.MessageText,
            CreatedAt = DateTime.UtcNow
        };
        await _messages.AddAsync(chat);
        return Ok(chat);
    }

    [HttpDelete("history")]
    public async Task<IActionResult> ClearHistory()
    {
        var id = CurrentEmployeeId();
        var deleted = await _messages.ClearForEmployeeAsync(id);
        return Ok(new { deleted });
    }
}