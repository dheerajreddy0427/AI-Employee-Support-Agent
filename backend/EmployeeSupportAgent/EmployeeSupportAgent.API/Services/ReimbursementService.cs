using EmployeeSupportAgent.API.Models;
using EmployeeSupportAgent.API.Repositories;

namespace EmployeeSupportAgent.API.Services;

public class ReimbursementService
{
    private readonly IReimbursementRepository _reimbursements;

    public ReimbursementService(IReimbursementRepository reimbursements)
    {
        _reimbursements = reimbursements;
    }

    public async Task<Reimbursement> CreateAsync(Reimbursement request)
    {
        var now = DateTime.UtcNow;
        request.Status = "Pending";
        request.SubmittedDate = now;
        request.CreatedAt = now;
        request.UpdatedAt = now;
        await _reimbursements.AddAsync(request);
        return request;
    }

    public Task<IReadOnlyList<Reimbursement>> GetAllAsync() => _reimbursements.ListAsync();
    public Task<Reimbursement?> GetByIdAsync(int id) => _reimbursements.GetByIdAsync(id);
}
