namespace EmployeeSupportAgent.API.Models;

/// <summary>
/// Allowed Status values: "Open" | "InProgress" | "Resolved" | "Closed".
/// Kept as string so it round-trips through the SQLite/JSON pipeline unchanged.
/// </summary>
public class ITTicket
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string IssueTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? AssignedToId { get; set; }
    public DateTime? ResolvedDate { get; set; }
}