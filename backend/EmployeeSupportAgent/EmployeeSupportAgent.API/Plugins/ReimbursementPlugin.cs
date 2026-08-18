using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Services;
using Microsoft.SemanticKernel;

namespace EmployeeSupportAgent.API.Plugins;

public class ReimbursementPlugin
{
    private readonly ReimbursementService _service;

    public ReimbursementPlugin(ReimbursementService service)
    {
        _service = service;
    }

    [KernelFunction]
    public string SubmitReimbursement(int employeeId, decimal amount, string description)
    {
        var reimbursement = _service
            .CreateAsync(new Reimbursement
            {
                EmployeeId = employeeId,
                Amount = amount,
                Description = description
            })
            .GetAwaiter().GetResult();

        return $"Reimbursement request #{reimbursement.Id} submitted";
    }
}
