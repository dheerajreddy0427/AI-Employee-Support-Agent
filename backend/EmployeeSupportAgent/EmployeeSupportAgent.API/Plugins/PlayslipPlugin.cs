using EmployeeSupportAgent.API.Services;
using Microsoft.SemanticKernel;

namespace EmployeeSupportAgent.API.Plugins;

public class PayslipPlugin
{
    private readonly PayslipService _payslipService;

    public PayslipPlugin(PayslipService payslipService)
    {
        _payslipService = payslipService;
    }

    [KernelFunction]
    public string GetLatestPayslip(int employeeId)
    {
        var payslips = _payslipService.GetForEmployeeAsync(employeeId).GetAwaiter().GetResult();

        var latest = payslips.OrderByDescending(x => x.UploadedDate).FirstOrDefault();
        if (latest == null) return "No payslips found";
        return latest.FileUrl;
    }
}
