namespace EmployeeSupportAgent.API.Dtos;

public class ChatMessageDto
{
    public int EmployeeId { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
}
