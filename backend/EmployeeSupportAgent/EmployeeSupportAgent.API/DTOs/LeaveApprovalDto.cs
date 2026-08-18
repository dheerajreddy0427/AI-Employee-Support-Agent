namespace EmployeeSupportAgent.API.Dtos;

public class LeaveApprovalDto
{
    public int LeaveId { get; set; }
    public int ManagerId { get; set; }
    public string Remarks { get; set; } = string.Empty;
}
