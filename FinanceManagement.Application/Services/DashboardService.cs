using FinanceManagement.Application.Interfaces;
using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Tsp;

namespace FinanceManagement.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ILoggedInUser _loggedInUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrencyConversion _currencyConversion;

        public DashboardService(ILoggedInUser loggedInUser, IUnitOfWork unitOfWork,ICurrencyConversion currencyConversion)
        {
            _loggedInUser = loggedInUser;
            _unitOfWork = unitOfWork;
            _currencyConversion = currencyConversion;
        }
        public async Task<IEnumerable<Transaction>> Last5Transaction()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }
            var last5Transaction = await _unitOfWork.Transaction.LastFiveTransaction(userId);
            await _unitOfWork.Transaction.GetAllPopulatedAsync(include: q => q.Include(t => t.Category));
            return last5Transaction;
        }

        public async Task<decimal?> TotalIncome()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var incomeTransactions = await _unitOfWork.Transaction.GetAllAsync(t => t.UserId == userId && t.Category.CategoryType.ToString() == "Income");
            var income = incomeTransactions.Sum(t => t.Amount);
            return income;
        }
        public async Task<decimal?> TotalExpense()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var expenseTransactions = await _unitOfWork.Transaction.GetAllAsync(t => t.UserId == userId && t.Category.CategoryType.ToString() == "Expense");
            var expense = expenseTransactions.Sum(e => e.Amount);
            return expense;
        }
        public async Task<User> GetUser()
        {
            var loggedInUser = _loggedInUser.CurrentLoggedInUser();
            User user = await _unitOfWork.User.GetAsync(u => u.UserId == loggedInUser);
            await _unitOfWork.User.GetAllPopulatedAsync(include: q => q.Include(q => q.Currency));
            return user;
        }

        public async Task<decimal> CurrencyConversion(string currencyToConvert)
        {
            var loggedInUser = _loggedInUser.CurrentLoggedInUser();
            var user = await _unitOfWork.User.GetAllPopulatedAsync(u => u.UserId==loggedInUser,include: q => q.Include(t => t.Currency));
            var userUpdated = user.First();

            var userBaseCurrencyCode = userUpdated.Currency.CurrencyCode;
            var currencyToUpdate = currencyToConvert;

            var convertedCurrencyRate = await _currencyConversion.GetConvertedRates(userBaseCurrencyCode, currencyToUpdate);
            return convertedCurrencyRate.conversion_rate;
        }
        
        public async Task<IEnumerable<Transaction>> After5()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }
            var allTransactions = await _unitOfWork.Transaction.GetAllAsync(t => t.UserId==userId);
            var updatedALlTransactions = allTransactions.OrderByDescending(u => u.TransactionDate).ThenByDescending(u => u.Amount).Skip(5);

            await _unitOfWork.Transaction.GetAllPopulatedAsync(include: q => q.Include(t => t.Category));
            return updatedALlTransactions;
        }

    }
}
