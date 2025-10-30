using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    }
}
