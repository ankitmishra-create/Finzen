using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FinanceManagement.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ILoggedInUser _loggedInUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrencyConversionService _currencyConversion;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(ILogger<DashboardService> logger, ILoggedInUser loggedInUser, IUnitOfWork unitOfWork, ICurrencyConversionService currencyConversion)
        {
            _loggedInUser = loggedInUser;
            _unitOfWork = unitOfWork;
            _currencyConversion = currencyConversion;
            _logger = logger;
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();

            try
            {
                var user = await _unitOfWork.User.GetPopulatedAsync(u => u.UserId == userId, include: q => q.Include(u => u.Currency));

                if (user == null)
                {
                    throw new UserNotFoundException("User not found");
                }
                if (user.Currency == null)
                {
                    _logger.LogError("User {UserId} has no base currency assigned", userId);
                    throw new InvalidCurrencyException("User's base currency is not configured.");
                }

                var userTransaction = await _unitOfWork.Transaction.GetAllPopulatedAsync(t => t.UserId == userId, include: q => q.Include(c => c.Category));
                var incomeTransactions = userTransaction.Where(t => t.Category.CategoryType==CategoryType.Income).GroupBy(t => t.Category.CategoryName).Select(x => new CategorySummaryDto
                {
                    CategoryName=x.Key,
                    Amount=x.Sum(a => a.Amount)
                }).ToList();

                var expenseTransactions = userTransaction.Where(t => t.Category.CategoryType == CategoryType.Expense).GroupBy(t => t.Category.CategoryName).Select(x => new CategorySummaryDto
                {
                    CategoryName=x.Key,
                    Amount = x.Sum(t => t.Amount)
                }).ToList();

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
                dashboardDto.IncomeCategorySummary = incomeTransactions;
                dashboardDto.ExpenseCategorySummary = expenseTransactions;
                return dashboardDto;
            }
            catch (UserNotFoundException)
            {
                _logger.LogError("Requested User Not Found");
                throw;
            }
            catch (InvalidCurrencyException)
            {
                _logger.LogError("Currency Trying to be accessed is invalid");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error while retrieving dashboard data for user {UserId}", userId);
                throw new DataRetrievalException("A database error occurred while loading dashboard data.", ex);
            }
        }

        private async Task<decimal> GetTotalAmountByTypeAsync(Guid userId, CategoryType categoryType)
        {
            return await _unitOfWork.Transaction.SumAsync(
                t => t.UserId == userId && t.Category.CategoryType == categoryType
            );
        }

        public async Task<decimal> CurrencyConversion(string currencyToConvert)
        {
            var loggedInUserId = _loggedInUser.CurrentLoggedInUser();

            User user;
            try
            {
                user = await _unitOfWork.User.GetPopulatedAsync(u => u.UserId == loggedInUserId, include: q => q.Include(t => t.Currency));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CurrencyConversion: Database error finding user {UserId}", loggedInUserId);
                throw new DatabaseException("Could not retrieve user for currency conversion.", ex);
            }
            if (user == null)
            {
                _logger.LogWarning("CurrencyConversion: User {UserId} not found", loggedInUserId);
                throw new UserNotFoundException("User not found");
            }
            if (user.Currency == null)
            {
                _logger.LogWarning("CurrencyConversion: User {UserId} has no base currency set", loggedInUserId);
                throw new InvalidCurrencyException("Invalid Currency");
            }
            var userBaseCurrencyCode = user.Currency.CurrencyCode;
            if (userBaseCurrencyCode.Equals(currencyToConvert, StringComparison.OrdinalIgnoreCase))
            {
                return 1.0m;
            }
            try
            {
                var convertedCurrencyRate = await _currencyConversion.GetConvertedRates(userBaseCurrencyCode, currencyToConvert);
                if (convertedCurrencyRate == null)
                {
                    _logger.LogWarning("Currency conversion API returned null for {Base} to {Target}", userBaseCurrencyCode, currencyToConvert);
                    throw new CurrencyConversionException("Currency conversion service returned no data.");
                }
                return convertedCurrencyRate.conversion_rate;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Currency conversion API request failed: {ErrorMessage}", ex.Message);
                throw new CurrencyConversionException("Failed to connect to currency conversion service.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Currency conversion failed. {Base} to {Target}", userBaseCurrencyCode, currencyToConvert);
                throw new CurrencyConversionException($"An unexpected error occurred during currency conversion: {ex.Message}", ex);
            }
        }

        public async Task<YearlyTransactionSummary> DashboardGraphData()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var userTransaction = await _unitOfWork.Transaction.GetAllPopulatedAsync(t => t.UserId == userId, include: q => q.Include(c => c.Category));
            var orderedTransaction = userTransaction.Where(t => t.TransactionDate.HasValue)
               .GroupBy(t => t.TransactionDate.Value.Year).OrderByDescending(t => t.Key).Take(1).Select(y => new YearlyTransactionSummary
               {
                   Year = y.Key,
                   Months = y.GroupBy(t => t.TransactionDate.Value.Month).OrderBy(m => m.Key)
                   .Select(m => new MonthSummary
                   {
                       MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m.Key),
                       Categories = m.GroupBy(t => t.Category.CategoryType).Select(c => new CategorySummary
                       {
                           CategoryType = (CategoryType)c.Key,
                           Amount = (decimal)c.Sum(t => t.Amount)
                       }).ToList()
                   }).ToList()
               }).ToList();
            if (orderedTransaction.Count > 0)
            {
                return orderedTransaction[0];    
            }
            return null;

        }
    }
}
