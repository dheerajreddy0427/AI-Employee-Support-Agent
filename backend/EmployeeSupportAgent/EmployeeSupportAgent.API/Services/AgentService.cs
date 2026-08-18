using System.Text.RegularExpressions;
using EmployeeSupportAgent.API.Data;
using EmployeeSupportAgent.API.Dtos;
using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Plugins;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EmployeeSupportAgent.API.Services;

public class AgentService
{
    private readonly Kernel _kernel;
    private readonly IntentRouter _router;
    private readonly AppDbContext _db;
    private readonly EmployeePlugin _employee;
    private readonly LeavePlugin _leave;
    private readonly TicketPlugin _ticket;
    private readonly PayslipPlugin _payslip;
    private readonly ReimbursementPlugin _reimbursement;
    private readonly ILogger<AgentService> _log;

    public AgentService(
        Kernel kernel,
        IntentRouter router,
        AppDbContext db,
        EmployeePlugin employee,
        LeavePlugin leave,
        TicketPlugin ticket,
        PayslipPlugin payslip,
        ReimbursementPlugin reimbursement,
        ILogger<AgentService> log)
    {
        _kernel = kernel;
        _router = router;
        _db = db;
        _employee = employee;
        _leave = leave;
        _ticket = ticket;
        _payslip = payslip;
        _reimbursement = reimbursement;
        _log = log;
    }

    public async Task<AgentResponseDto> AskAsync(int employeeId, string message, CancellationToken ct = default)
    {
        var match = _router.Match(message);

        // If a real LLM is wired up, prefer it for natural chat. We still bias
        // the router for known intents so the demo is reliable.
        if (_kernel.Services.GetService<IChatCompletionService>() != null
            && match.Intent is AgentIntent.Greeting or AgentIntent.Help or AgentIntent.Fallback)
        {
            try
            {
                var llmReply = await RunLlmAsync(message);
                Persist(employeeId, "User", message);
                Persist(employeeId, "Agent", llmReply);
                return new AgentResponseDto { Reply = llmReply, Intent = match.Intent.ToString() };
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "LLM path failed, falling back to router");
            }
        }

        var (reply, meta) = Dispatch(employeeId, match);
        Persist(employeeId, "User", message);
        Persist(employeeId, "Agent", reply);

        return new AgentResponseDto
        {
            Reply = reply,
            Intent = match.Intent.ToString(),
            Meta = meta.Count == 0 ? null : meta
        };
    }

    private async Task<string> RunLlmAsync(string message)
    {
        var chat = _kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory("You are an HR support assistant. Be concise, kind, and helpful.");
        history.AddUserMessage(message);
        var result = await chat.GetChatMessageContentAsync(history);
        return result.Content ?? string.Empty;
    }

    private (string reply, Dictionary<string, object> meta) Dispatch(int employeeId, IntentResult match)
    {
        var meta = new Dictionary<string, object>();

        switch (match.Intent)
        {
            case AgentIntent.Greeting:
            case AgentIntent.Help:
                return (match.Reply, meta);

            case AgentIntent.Profile:
                {
                    var emp = _db.Employees.Find(employeeId);
                    if (emp == null) return ("I couldn't find your profile.", meta);
                    return ($"You're {emp.FullName}, working in the {emp.Department} department as {emp.Role}. You have {emp.LeaveBalance} leave days remaining.", meta);
                }

            case AgentIntent.Department:
                {
                    var dept = _employee.GetDepartment(employeeId);
                    return ($"You belong to the {dept} department.", meta);
                }

            case AgentIntent.LeaveBalance:
                {
                    var bal = _employee.GetLeaveBalance(employeeId);
                    return ($"You have {bal} leave days remaining.", meta);
                }

            case AgentIntent.MyLeaves:
                {
                    var leaves = _db.LeaveRequests
                        .Where(l => l.EmployeeId == employeeId)
                        .OrderByDescending(l => l.CreatedAt)
                        .Take(5)
                        .ToList();

                    if (leaves.Count == 0)
                        return ("You have no leave requests on file.", meta);

                    var lines = leaves.Select(l =>
                        $"#{l.Id} {l.StartDate:yyyy-MM-dd} → {l.EndDate:yyyy-MM-dd} · {l.Status}");
                    meta["leaves"] = leaves.Select(l => new
                    {
                        id = l.Id,
                        startDate = l.StartDate,
                        endDate = l.EndDate,
                        status = l.Status,
                        reason = l.Reason
                    }).ToArray();
                    return ("Here are your recent leave requests:\n" + string.Join("\n", lines), meta);
                }

            case AgentIntent.ApplyLeave:
                {
                    if (!match.Slots.TryGetValue("startDate", out var sObj)
                        || !match.Slots.TryGetValue("endDate", out var eObj))
                    {
                        return ("To apply for leave, tell me the dates. For example: \"apply leave from 2026-08-10 to 2026-08-12\".", meta);
                    }
                    var start = (DateTime)sObj;
                    var end = (DateTime)eObj;
                    if (end < start)
                        return ("The end date is before the start date. Please try again.", meta);
                    var emp = _db.Employees.Find(employeeId);
                    if (emp == null) return ("I couldn't find your profile.", meta);
                    var days = (int)Math.Ceiling((end - start).TotalDays) + 1;
                    if (days > emp.LeaveBalance)
                        return ($"You only have {emp.LeaveBalance} leave days, but you asked for {days}.", meta);

                    var reply = _leave.ApplyLeave(employeeId, start, end);
                    meta["leave"] = new { startDate = start, endDate = end, days };
                    return (reply + $" ({days} day(s), pending approval).", meta);
                }

            case AgentIntent.Payslip:
                {
                    var url = _payslip.GetLatestPayslip(employeeId);
                    if (string.IsNullOrEmpty(url) || url == "No payslips found")
                        return ("No payslips have been uploaded for you yet.", meta);
                    meta["payslipUrl"] = url;
                    return ($"Here is your latest payslip: {url}", meta);
                }

            case AgentIntent.RaiseTicket:
                {
                    if (!match.Slots.TryGetValue("issue", out var issueObj) || string.IsNullOrWhiteSpace(issueObj?.ToString()))
                        return ("What issue should I raise a ticket for? Try: \"raise ticket for laptop not working\".", meta);
                    var issue = issueObj.ToString()!;
                    // Strip a leading "for" left over from the trigger
                    issue = Regex.Replace(issue, @"^\s*for\s+", "", RegexOptions.IgnoreCase);
                    if (string.IsNullOrWhiteSpace(issue))
                        return ("What issue should I raise a ticket for? Try: \"raise ticket for laptop not working\".", meta);
                    var title = issue.Length > 40 ? issue.Substring(0, 40) + "…" : issue;
                    var reply = _ticket.RaiseTicket(employeeId, title, issue);
                    meta["issue"] = issue;
                    return (reply + " — IT will look at it shortly.", meta);
                }

            case AgentIntent.MyTickets:
                {
                    var tickets = _db.ITTickets
                        .Where(t => t.EmployeeId == employeeId)
                        .OrderByDescending(t => t.CreatedDate)
                        .Take(5)
                        .ToList();
                    if (tickets.Count == 0) return ("You have no open tickets.", meta);
                    var lines = tickets.Select(t => $"#{t.Id} {t.IssueTitle} · {t.Status}");
                    meta["tickets"] = tickets.Select(t => new
                    {
                        id = t.Id,
                        title = t.IssueTitle,
                        status = t.Status,
                        createdDate = t.CreatedDate
                    }).ToArray();
                    return ("Your recent IT tickets:\n" + string.Join("\n", lines), meta);
                }

            case AgentIntent.Reimbursement:
                {
                    if (!match.Slots.TryGetValue("amount", out var amtObj) || !decimal.TryParse(amtObj?.ToString(), out var amount))
                        return ("How much should I reimburse? Try: \"reimburse 250 for travel\".", meta);
                    var reason = match.Slots.TryGetValue("reason", out var rObj) ? rObj?.ToString() ?? "General expense" : "General expense";
                    var reply = _reimbursement.SubmitReimbursement(employeeId, amount, reason);
                    meta["amount"] = amount;
                    meta["reason"] = reason;
                    return (reply + $" for {reason}.", meta);
                }

            case AgentIntent.MyReimbursements:
                {
                    var items = _db.Reimbursements
                        .Where(r => r.EmployeeId == employeeId)
                        .OrderByDescending(r => r.SubmittedDate)
                        .Take(5)
                        .ToList();
                    if (items.Count == 0) return ("You have no reimbursement requests.", meta);
                    var lines = items.Select(r => $"#{r.Id} {r.Amount:C} · {r.Description} · {r.Status}");
                    meta["reimbursements"] = items.Select(r => new
                    {
                        id = r.Id,
                        amount = r.Amount,
                        description = r.Description,
                        status = r.Status
                    }).ToArray();
                    return ("Your recent reimbursements:\n" + string.Join("\n", lines), meta);
                }

            default:
                return (match.Reply ?? "Sorry, I couldn't understand your request.", meta);
        }
    }

    private void Persist(int employeeId, string sender, string text)
    {
        try
        {
            _db.ChatMessages.Add(new ChatMessage
            {
                EmployeeId = employeeId,
                Sender = sender,
                MessageText = text,
                CreatedAt = DateTime.UtcNow
            });
            _db.SaveChanges();
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Failed to persist chat message");
        }
    }
}
