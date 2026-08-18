using System.Globalization;
using System.Text.RegularExpressions;

namespace EmployeeSupportAgent.API.Services;

public enum AgentIntent
{
    Greeting,
    Help,
    Profile,
    Department,
    LeaveBalance,
    MyLeaves,
    ApplyLeave,
    Payslip,
    RaiseTicket,
    MyTickets,
    Reimbursement,
    MyReimbursements,
    Fallback
}

public class IntentResult
{
    public AgentIntent Intent { get; set; }
    public Dictionary<string, object> Slots { get; set; } = new();
    public string Reply { get; set; } = string.Empty;
}

/// <summary>
/// Deterministic rule-based NLU router. Keeps the agent working out of the box
/// without an external LLM; the same plugin services are still invoked.
/// </summary>
public class IntentRouter
{
    private static readonly Regex IsoDate = new(@"\b(\d{4})-(\d{1,2})-(\d{1,2})\b", RegexOptions.Compiled);
    private static readonly Regex SlashDate = new(@"\b(\d{1,2})/(\d{1,2})/(\d{2,4})\b", RegexOptions.Compiled);
    private static readonly Regex MonthName = new(
        @"\b(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Sept|Oct|Nov|Dec)[a-z]*\s+(\d{1,2})(?:,?\s+(\d{2,4}))?\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex Amount = new(@"\b(\d+(?:\.\d{1,2})?)\b", RegexOptions.Compiled);
    private static readonly string[] DateKeywords = { "today", "tomorrow", "yesterday" };

    public IntentResult Match(string rawMessage)
    {
        var msg = (rawMessage ?? string.Empty).Trim().ToLowerInvariant();
        var result = new IntentResult();

        if (string.IsNullOrWhiteSpace(msg))
        {
            result.Intent = AgentIntent.Fallback;
            result.Reply = "Please type a question. Try \"help\" to see what I can do.";
            return result;
        }

        // 1) Greeting
        if (Regex.IsMatch(msg, @"\b(hi|hello|hey|good\s+(morning|afternoon|evening))\b"))
        {
            result.Intent = AgentIntent.Greeting;
            result.Reply = "Hi there! I'm your HR assistant. I can help with leaves, payslips, IT tickets, and reimbursements. Try \"help\" to see everything.";
            return result;
        }

        // 2) Help
        if (Regex.IsMatch(msg, @"\b(help|what can you do|commands|options)\b"))
        {
            result.Intent = AgentIntent.Help;
            result.Reply = "Here's what I can do:\n"
                + "• \"apply leave from 2026-08-10 to 2026-08-12\"\n"
                + "• \"leave balance\" or \"how many leaves do I have\"\n"
                + "• \"my leaves\"\n"
                + "• \"show my payslip\" or \"last salary\"\n"
                + "• \"raise ticket for <issue>\"\n"
                + "• \"my tickets\"\n"
                + "• \"reimburse 250 for travel\"\n"
                + "• \"my reimbursements\"\n"
                + "• \"my profile\" or \"my department\"";
            return result;
        }

        // 3) Profile / department
        if (Regex.IsMatch(msg, @"\b(my profile|who am i|about me|my details)\b"))
        {
            result.Intent = AgentIntent.Profile;
            return result;
        }
        if (Regex.IsMatch(msg, @"\b(my department|which department|my team|my role)\b"))
        {
            result.Intent = AgentIntent.Department;
            return result;
        }

        // 4) My reimbursements list (check before submission to win priority)
        if (Regex.IsMatch(msg, @"\b(my reimbursement|my reimbursements|reimbursement status|reimbursements status|my claims|show reimbursements|list reimbursements)\b"))
        {
            result.Intent = AgentIntent.MyReimbursements;
            return result;
        }

        // 5) Reimbursement submission
        if (Regex.IsMatch(msg, @"\b(submit|file|raise|create|new|claim|reimburse)\b.*\b(reimburs(e|ement))\b")
            || Regex.IsMatch(msg, @"\breimburs(e|ement)\b\s+\d")
            || Regex.IsMatch(msg, @"^\s*reimburs(e|ement)\b"))
        {
            // Try to extract amount
            var amtMatch = Regex.Match(msg, @"\b(?:reimburs(?:e|ement)\s+)?(\d+(?:\.\d{1,2})?)\b");
            if (amtMatch.Success && decimal.TryParse(amtMatch.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amt))
            {
                result.Slots["amount"] = amt;
                var reason = ExtractReason(rawMessage);
                if (!string.IsNullOrWhiteSpace(reason))
                    result.Slots["reason"] = reason;
            }
            result.Intent = AgentIntent.Reimbursement;
            return result;
        }

        // 6) Apply leave — most specific
        if (Regex.IsMatch(msg, @"\b(apply|book|request|take)\s+leave\b")
            || Regex.IsMatch(msg, @"\bleave\s+from\b")
            || Regex.IsMatch(msg, @"\b(\d+)\s*days?\s+(off|leave)\b"))
        {
            var (start, end) = ExtractLeaveRange(rawMessage);
            if (start.HasValue) result.Slots["startDate"] = start.Value;
            if (end.HasValue) result.Slots["endDate"] = end.Value;
            result.Intent = AgentIntent.ApplyLeave;
            return result;
        }

        // 7) My leaves
        if (Regex.IsMatch(msg, @"\b(my leaves|leave history|leave status|leaves status|my leave requests)\b"))
        {
            result.Intent = AgentIntent.MyLeaves;
            return result;
        }

        // 8) Leave balance
        if (Regex.IsMatch(msg, @"\b(leave\s+balance|how\s+many\s+leaves?|leaves?\s+remaining|remaining\s+leaves?|available\s+leaves?)\b"))
        {
            result.Intent = AgentIntent.LeaveBalance;
            return result;
        }

        // 9) Raise IT ticket
        if (Regex.IsMatch(msg, @"\b(raise|create|file|log|open|new)\s+(it\s+)?(ticket|issue|request)\b")
            || Regex.IsMatch(msg, @"\bit\s+(support|help|ticket)\b")
            || Regex.IsMatch(msg, @"\breport\s+(a|an|the)?\s*(issue|problem|bug)\b"))
        {
            var issue = ExtractAfterTrigger(rawMessage, new[] { "raise ticket", "raise it ticket", "create ticket", "file ticket", "log ticket", "open ticket", "new ticket", "report", "it support", "it help" });
            if (!string.IsNullOrWhiteSpace(issue))
                result.Slots["issue"] = issue;
            result.Intent = AgentIntent.RaiseTicket;
            return result;
        }

        // 10) My tickets
        if (Regex.IsMatch(msg, @"\b(my tickets?|ticket status|tickets? status)\b"))
        {
            result.Intent = AgentIntent.MyTickets;
            return result;
        }

        // 11) Payslip
        if (Regex.IsMatch(msg, @"\b(payslip|salary slip|salary|latest pay|last pay|compensation)\b"))
        {
            result.Intent = AgentIntent.Payslip;
            return result;
        }

        // 12) Fallback
        result.Intent = AgentIntent.Fallback;
        result.Reply = "I didn't catch that. Type \"help\" to see what I can do, or ask about leave, payslip, tickets, or reimbursements.";
        return result;
    }

    public static (DateTime? start, DateTime? end) ExtractLeaveRange(string text)
    {
        // "apply leave from 2026-08-10 to 2026-08-12"
        var fromTo = Regex.Match(text, @"\bfrom\s+(\S+)\s+to\s+(\S+)", RegexOptions.IgnoreCase);
        if (fromTo.Success)
        {
            var s = ParseDate(fromTo.Groups[1].Value);
            var e = ParseDate(fromTo.Groups[2].Value);
            if (s.HasValue && e.HasValue) return (s, e);
        }

        // "2026-08-10 to 2026-08-12"
        var twoIso = Regex.Matches(text, IsoDate.ToString());
        if (twoIso.Count >= 2)
        {
            var s = ParseDate(twoIso[0].Value);
            var e = ParseDate(twoIso[1].Value);
            if (s.HasValue && e.HasValue) return (s, e);
        }

        // "from Aug 10 to Aug 12"
        var twoNames = Regex.Matches(text, MonthName.ToString(), RegexOptions.IgnoreCase);
        if (twoNames.Count >= 2)
        {
            var s = ParseDate(twoNames[0].Value);
            var e = ParseDate(twoNames[1].Value);
            if (s.HasValue && e.HasValue) return (s, e);
        }

        // "take 3 days off" / "3 days leave"
        var daysMatch = Regex.Match(text, @"\b(\d+)\s*days?\s+(off|leave)\b", RegexOptions.IgnoreCase);
        if (daysMatch.Success && int.TryParse(daysMatch.Groups[1].Value, out var n) && n > 0)
        {
            var start = DateTime.Today;
            return (start, start.AddDays(n - 1));
        }

        // "tomorrow"
        if (Regex.IsMatch(text, @"\btomorrow\b", RegexOptions.IgnoreCase))
        {
            var d = DateTime.Today.AddDays(1);
            return (d, d);
        }

        return (null, null);
    }

    public static DateTime? ParseDate(string token)
    {
        token = token.Trim().Trim('.', ',', ';');
        string[] formats = { "yyyy-MM-dd", "yyyy/M/d", "M/d/yyyy", "M-d-yyyy", "d MMM", "d MMM yyyy", "MMM d", "MMM d yyyy" };
        if (DateTime.TryParseExact(token, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
            return dt.Date;
        if (DateTime.TryParse(token, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
            return dt.Date;
        return null;
    }

    private static string ExtractReason(string text)
    {
        // "for <reason>" or "because <reason>"
        var m = Regex.Match(text, @"\bfor\s+([a-zA-Z][\w\s\-]{2,80})$", RegexOptions.IgnoreCase);
        if (m.Success) return m.Groups[1].Value.Trim();
        return string.Empty;
    }

    private static string ExtractAfterTrigger(string text, string[] triggers)
    {
        foreach (var trig in triggers.OrderByDescending(t => t.Length))
        {
            var idx = text.ToLowerInvariant().IndexOf(trig, StringComparison.Ordinal);
            if (idx >= 0)
            {
                var after = text.Substring(idx + trig.Length).Trim(' ', ':', '-', '.', ',');
                if (!string.IsNullOrWhiteSpace(after)) return after;
            }
        }
        return string.Empty;
    }
}
