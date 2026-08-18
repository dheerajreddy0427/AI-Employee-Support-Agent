namespace EmployeeSupportAgent.API.Models;

public class Payslip
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string MonthYear { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}