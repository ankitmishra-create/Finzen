using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Core.Enums;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ILoggedInUser _loggedInUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrencyConversionService _currencyConversion;

        public DashboardService(ILoggedInUser loggedInUser, IUnitOfWork unitOfWork, ICurrencyConversionService currencyConversion)
        {
            _loggedInUser = loggedInUser;
            _unitOfWork = unitOfWork;
            _currencyConversion = currencyConversion;
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var user = await _unitOfWork.User.GetPopulatedAsync(u => u.UserId == userId, include: q => q.Include(u => u.Currency));

            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }
            var totalIncome = await GetTotalAmountByTypeAsync(userId, CategoryType.Income);
            var totalExpense = await GetTotalAmountByTypeAsync(userId, CategoryType.Expense);

            var recentTransaction = await _unitOfWork.Transaction.LastFiveTransaction(userId);
            var dashboardDto = new DashboardDto()
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                TotalBalance = totalIncome - totalExpense,
                RecentTransaction = recentTransaction,
                BaseCurrencyCode = user.Currency.CurrencyCode
            };
            return dashboardDto;
        }

        private async Task<decimal> GetTotalAmountByTypeAsync(Guid userId, CategoryType categoryType)
        {
            return await _unitOfWork.Transaction.SumAsync(
                t => t.UserId == userId && t.Category.CategoryType == categoryType
            );
        }

        public async Task<decimal> CurrencyConversion(string currencyToConvert)
        {
            var loggedInUser = _loggedInUser.CurrentLoggedInUser();
            var user = await _unitOfWork.User.GetPopulatedAsync(u => u.UserId == loggedInUser, include: q => q.Include(t => t.Currency));

            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }
            if (user.Currency == null)
            {
                throw new InvalidCurrencyException("Invalid Currency");
            }
            var userBaseCurrencyCode = user.Currency.CurrencyCode;
            if (userBaseCurrencyCode.Equals(currencyToConvert, StringComparison.OrdinalIgnoreCase))
            {
                return 1.0m;
            }
            var convertedCurrencyRate = await _currencyConversion.GetConvertedRates(userBaseCurrencyCode, currencyToConvert);
            return convertedCurrencyRate.conversion_rate;
        }
    }
}
