using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Application.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly ILoggedInUser _loggedInUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrencyConversionService _currencyConversionService;
        private readonly ILogger<RecurringTransactionService> _logger;
        public RecurringTransactionService(ILogger<RecurringTransactionService> logger,ICurrencyConversionService currencyConversionService, ILoggedInUser loggedInUser, IUnitOfWork unitOfWork)
        {
            _loggedInUser = loggedInUser;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _currencyConversionService = currencyConversionService;
        }

        public async Task<List<RecurringTransactions>> GetRecuringTransactionsAsync()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            try
            {
                var allRecuringTransaction = await _unitOfWork.RecurringTransaction.GetAllPopulatedAsync(rt => rt.UserId == userId, include: q => q.Include(q => q.Category));
                var sortedResult = allRecuringTransaction.OrderBy(t => t.NextTransactionDate).ThenByDescending(t => t.Amount);
                return sortedResult.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error getting recurring transactions for user {UserId}", userId);
                throw new DataRetrievalException("Could not retrieve recurring transactions.", ex);
            }
        }

        public async Task DeleteRecurringTransaction(Guid recurringTransactionId)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var recurredTransaction = await _unitOfWork.RecurringTransaction.GetAsync(t => t.RecurringTransactionId == recurringTransactionId && t.UserId==userId);
            if (recurredTransaction == null)
            {
                _logger.LogError("Delete Recurring Transaction: Recurring Tranaction not found for {recurringTransactionId}", recurringTransactionId);
                throw new RecurringTransactionNotFoundException("Recurring Transaction not Found"); 
            }
            if (recurredTransaction != null)
            {
                recurredTransaction.IsActive = false;
                recurredTransaction.NextTransactionDate = null;
                recurredTransaction.NextStepUpDate = null;
                recurredTransaction.IsStepUpTransaction = null;
            }
            _unitOfWork.RecurringTransaction.Update(recurredTransaction);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Database Error Exception Occured");
                throw new DatabaseException("Database Exception", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected Exception");
            }
        }

        public async Task<RecurringTransactions> CreateRecurringTransaction(AddTransactionVM addTransactionVM, decimal originalAmount)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var user = await _unitOfWork.User.GetPopulatedAsync(u => u.UserId == userId, include: q => q.Include(t => t.Currency));

            RecurringTransactions recurringTransaction = new RecurringTransactions()
            {
                UserId = addTransactionVM.UserId,
                CategoryId = (Guid)addTransactionVM.CategoryId,
                Frequency = (Core.Enums.Frequency)addTransactionVM.RecurrenceFrequency,
                StartDate = addTransactionVM.RecurringStartDate.Value,
                EndDate = addTransactionVM.RecurringEndDate,
                Amount = (decimal)addTransactionVM.Amount,
                Description = addTransactionVM.Description,
                TransactionTerrority = addTransactionVM.TransactionTerrority,
                LastExecutedDate = DateTime.UtcNow,
                IsActive = true,
                TransactionCurrency = user.Currency.CurrencyCode,
                OriginalAmount = originalAmount,
                OriginalCurrency = addTransactionVM.SelectedCurrency
            };

            if (addTransactionVM.IsStepUpTransaction)
            {
                recurringTransaction.IsStepUpTransaction = true;
                if (addTransactionVM.StepUpAmount.HasValue)
                {
                    recurringTransaction.StepUpAmount = addTransactionVM.StepUpAmount;
                }
                else
                {
                    recurringTransaction.StepUpPercentage = addTransactionVM.StepUpPercentage;
                }
                recurringTransaction.StepUpFrequeny = addTransactionVM.StepUpFrequeny;
                recurringTransaction.LastStepUpDate = DateTime.UtcNow;
                
                var nextStepUpDate = addTransactionVM.StepUpFrequeny;
                switch ((Frequency)nextStepUpDate)
                {
                    case Frequency.Daily:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddDays(1);
                        break;
                    case Frequency.Weekly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddDays(7);
                        break;
                    case Frequency.Monthly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddMonths(1);
                        break;
                    case Frequency.Quarterly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddMonths(3);
                        break;
                    case Frequency.Yearly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddYears(1);
                        break;
                }
            }


            var nextTransactionDate = addTransactionVM.RecurrenceFrequency;
            switch ((Frequency)nextTransactionDate)
            {
                case Frequency.Daily:
                    recurringTransaction.NextTransactionDate = recurringTransaction.LastExecutedDate.Value.AddDays(1);
                    break;
                case Frequency.Weekly:
                    recurringTransaction.NextTransactionDate = recurringTransaction.LastExecutedDate.Value.AddDays(7);
                    break;
                case Frequency.Monthly:
                    recurringTransaction.NextTransactionDate = recurringTransaction.LastExecutedDate.Value.AddMonths(1);
                    break;
                case Frequency.Quarterly:
                    recurringTransaction.NextTransactionDate = recurringTransaction.LastExecutedDate.Value.AddMonths(3);
                    break;
                case Frequency.Yearly:
                    recurringTransaction.NextTransactionDate = recurringTransaction.LastExecutedDate.Value.AddYears(1);
                    break;
            }


            return recurringTransaction;
        }

        public async Task<RecurringTransactions> EditRecurringTransaction(RecurringTransactionVM recurringTransactionVM)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var recurringTransaction = await _unitOfWork.RecurringTransaction.GetAsync(r => r.RecurringTransactionId == recurringTransactionVM.RecurringTransactionId && r.UserId==userId);
            if (recurringTransaction == null)
            {
                _logger.LogError("Edit Recurring Transaction: Edit Failed for {recurrintransactionid}", recurringTransactionVM.RecurringTransactionId);
                throw new RecurringTransactionNotFoundException("Recurring Transaction Not Found");
            }
            recurringTransaction.OriginalAmount = recurringTransactionVM.OriginalAmount;
            recurringTransaction.Frequency = recurringTransactionVM.RecurrenceFrequency;
            recurringTransaction.Description = recurringTransactionVM.Description;

            if (recurringTransactionVM.TransactionTerrority == TransactionTerrority.Domestic)
            {
                recurringTransaction.Amount = (decimal)recurringTransaction.OriginalAmount;
            }
            else
            {
                var user = await _unitOfWork.User.GetPopulatedAsync(u => u.UserId == userId, include: q => q.Include(c => c.Currency));
                if (user == null)
                {
                    return null;
                }
                var convertedRates = await _currencyConversionService.GetConvertedRates(recurringTransaction.OriginalCurrency, user.Currency.CurrencyCode);
                recurringTransaction.Amount = recurringTransactionVM.OriginalAmount * convertedRates.conversion_rate;
            }

            if (recurringTransactionVM.IsStepUpTransaction == true)
            {
                recurringTransaction.StepUpAmount = recurringTransactionVM.StepUpAmount;
                recurringTransaction.StepUpPercentage = recurringTransactionVM.StepUpPercentage;
                recurringTransaction.StepUpFrequeny = recurringTransactionVM.StepUpFrequeny;

                var nextStepUpDate = recurringTransactionVM.StepUpFrequeny;
                switch ((Frequency)nextStepUpDate)
                {
                    case Frequency.Daily:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddDays(1);
                        break;
                    case Frequency.Weekly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddDays(7);
                        break;
                    case Frequency.Monthly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddMonths(1);
                        break;
                    case Frequency.Quarterly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddMonths(3);
                        break;
                    case Frequency.Yearly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddYears(1);
                        break;
                }
            }
            else
            {
                recurringTransaction.IsStepUpTransaction = false;
                recurringTransaction.StepUpAmount = null;
                recurringTransaction.StepUpPercentage = null;
                recurringTransaction.StepUpFrequeny = null;
                recurringTransaction.NextStepUpDate = null;
            }

            var todaysDate = DateTime.UtcNow;
            var nextTransactionDate = recurringTransactionVM.RecurrenceFrequency;
            switch ((Frequency)nextTransactionDate)
            {
                case Frequency.Daily:
                    recurringTransaction.NextTransactionDate = todaysDate.AddDays(1);
                    break;
                case Frequency.Weekly:
                    recurringTransaction.NextTransactionDate = todaysDate.AddDays(7);
                    break;
                case Frequency.Monthly:
                    recurringTransaction.NextTransactionDate = todaysDate.AddMonths(1);
                    break;
                case Frequency.Quarterly:
                    recurringTransaction.NextTransactionDate = todaysDate.AddMonths(3);
                    break;
                case Frequency.Yearly:
                    recurringTransaction.NextTransactionDate = todaysDate.AddYears(1);
                    break;
            }
            _unitOfWork.RecurringTransaction.Update(recurringTransaction);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch(DatabaseException ex)
            {
                _logger.LogError(ex,"Database Error Exception Occured");
                throw new DatabaseException("Database Exception",ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected Exception");
            }
            return recurringTransaction;
        }

        public async Task<RecurringTransactionVM> EditView(Guid recurringTransactionId)
        {
            var recurringTransaction = await _unitOfWork.RecurringTransaction.GetAsync(r => r.RecurringTransactionId == recurringTransactionId);
            if (recurringTransaction == null)
            {
                _logger.LogError("Edit Recurring Transaction: Edit Failed for {recurrintransactionid}", recurringTransactionId);
                throw new RecurringTransactionNotFoundException("Recurring Transaction Not Found");
            }
            RecurringTransactionVM recurringTransactionVM = new RecurringTransactionVM
            {
                RecurringTransactionId = recurringTransaction.RecurringTransactionId,
                OriginalAmount = (decimal)recurringTransaction.OriginalAmount,
                RecurrenceFrequency = recurringTransaction.Frequency,
                TransactionTerrority = recurringTransaction.TransactionTerrority,
                OriginalCurrency = recurringTransaction.OriginalCurrency,
                Description = recurringTransaction.Description,
            };

            if (recurringTransaction.IsStepUpTransaction.HasValue && (bool)recurringTransaction.IsStepUpTransaction == true)
            {
                recurringTransactionVM.IsStepUpTransaction = true;
                if (recurringTransaction.StepUpAmount.HasValue)
                {
                    recurringTransactionVM.StepUpAmount = recurringTransaction.StepUpAmount.Value;
                }
                else
                {
                    recurringTransactionVM.StepUpPercentage = recurringTransaction?.StepUpPercentage;
                }
                recurringTransactionVM.StepUpFrequeny = recurringTransaction?.StepUpFrequeny;
            }
            else
            {
                recurringTransactionVM.IsStepUpTransaction = false;
            }
            return recurringTransactionVM;
        }
    }
}
