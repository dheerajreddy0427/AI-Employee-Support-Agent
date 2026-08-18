using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Repositories;

namespace EmployeeSupportAgent.API.Services;

public class PayslipService
{
    private readonly IPayslipRepository _payslips;

    public PayslipService(IPayslipRepository payslips)
    {
        _payslips = payslips;
    }

    public Task<IReadOnlyList<Payslip>> GetForEmployeeAsync(int employeeId) =>
        _payslips.GetForEmployeeAsync(employeeId);

    public Task<Payslip?> GetByIdAsync(int id) => _payslips.GetByIdAsync(id);

    public async Task<Payslip> CreateAsync(Payslip payslip)
    {
        var now = DateTime.UtcNow;
        payslip.UploadedDate = now;
        payslip.CreatedAt = now;
        payslip.UpdatedAt = now;
        await _payslips.AddAsync(payslip);
        return payslip;
    }
}