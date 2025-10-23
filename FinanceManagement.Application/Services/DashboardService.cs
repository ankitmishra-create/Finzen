using FinanceManagement.Application.Interfaces;
using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace FinanceManagement.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ILoggedInUser _loggedInUser;
        private readonly IUnitOfWork _unitOfWork;
        public DashboardService(ILoggedInUser loggedInUser, IUnitOfWork unitOfWork)
        {
            _loggedInUser = loggedInUser;
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<Transaction>> Last5Transaction()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }
            var last5Transaction =await _unitOfWork.Transaction.LastFiveTransaction(userId);
            await _unitOfWork.Transaction.GetAllPopulatedAsync(include: q => q.Include(t => t.Category));
            return last5Transaction;
        }

        public async Task<decimal> TotalIncome()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var incomeTransactions= await _unitOfWork.Transaction.GetAllAsync(t => t.UserId == userId && t.Category.CategoryType.ToString()=="Income");
            var income = incomeTransactions.Sum(t => t.Amount);
            return income;
        }
        public async Task<decimal> TotalExpense()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var expenseTransactions = await _unitOfWork.Transaction.GetAllAsync(t => t.UserId == userId && t.Category.CategoryType.ToString() == "Expense");
            var expense = expenseTransactions.Sum(e => e.Amount);
            return expense;
        }
    }
}
