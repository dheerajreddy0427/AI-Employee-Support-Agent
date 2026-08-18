namespace EmployeeSupportAgent.API.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
