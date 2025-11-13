using FinanceManagement.Application.ViewModels;

namespace FinanceManagement.Application.Interfaces;

public interface IInvestmentService
{
    Task<InvestmentTransactionVM> AddInvestment(InvestmentTransactionVM  investmentTransactionVm);
}