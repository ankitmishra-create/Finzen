using FinanceManagement.Application.DTO;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;

namespace FinanceManagement.Application.Interfaces
{
    public interface IBudgetService
    {
        Task<BudgetVM> CreateVM();
        Task<Budget> CreateBudget(BudgetVM budgetVM);
        Task<List<BudgetDto>> GetUserBudgets();
        Task<BudgetVM> EditView(Guid BudgetId);
        Task<Budget> EditBudget(BudgetVM budgetVM);
        Task DeleteBudget(Guid budgetId);
    }
}
