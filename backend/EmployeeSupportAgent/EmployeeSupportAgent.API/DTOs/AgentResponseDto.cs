namespace EmployeeSupportAgent.API.Dtos;

public class AgentResponseDto
{
    public string Reply { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public Dictionary<string, object>? Meta { get; set; }
}
