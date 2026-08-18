using EmployeeSupportAgent.API.Services;
using Microsoft.SemanticKernel;

namespace EmployeeSupportAgent.API.Plugins;

public class LeavePlugin
{
    private readonly LeaveService _leaveService;

    public LeavePlugin(LeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    [KernelFunction]
    public string ApplyLeave(int employeeId, DateTime startDate, DateTime endDate)
    {
        // Reason is filled by the model context; pass a sensible default if blank.
        var leave = _leaveService
            .ApplyAsync(employeeId, startDate, endDate, "Requested via HR assistant")
            .GetAwaiter().GetResult();

        return $"Leave request #{leave.Id} submitted successfully.";
    }
}
