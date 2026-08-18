using EmployeeSupportAgent.API.Models;

namespace EmployeeSupportAgent.API.Repositories;

public interface IReimbursementRepository : IRepository<Reimbursement>
{
    Task<IReadOnlyList<Reimbursement>> GetPendingAsync();
}