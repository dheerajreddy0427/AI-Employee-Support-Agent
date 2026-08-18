namespace EmployeeSupportAgent.API.Dtos;

public class UpdateTicketStatusDto
{
    /// <summary>One of: Open | InProgress | Resolved | Closed.</summary>
    public string Status { get; set; } = string.Empty;
    public int? AssignedToId { get; set; }
}

public static class TicketStatuses
{
    public const string Open = "Open";
    public const string InProgress = "InProgress";
    public const string Resolved = "Resolved";
    public const string Closed = "Closed";

    public static readonly string[] All = { Open, InProgress, Resolved, Closed };

    public static bool IsValid(string status) => Array.IndexOf(All, status) >= 0;
}
