using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Application.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly ILoggedInUser _loggedInUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrencyConversionService _currencyConversionService;
        public RecurringTransactionService(ICurrencyConversionService currencyConversionService, ILoggedInUser loggedInUser, IUnitOfWork unitOfWork)
        {
            _loggedInUser = loggedInUser;
            _unitOfWork = unitOfWork;
            _currencyConversionService = currencyConversionService;
        }

        public async Task<List<RecurringTransactions>> GetRecuringTransactionsAsync()
        {
            var user = _loggedInUser.CurrentLoggedInUser();
            var allRecuringTransaction = await _unitOfWork.RecurringTransaction.GetAllPopulatedAsync(rt => rt.UserId == user, include: q => q.Include(q => q.Category));
            await _unitOfWork.RecurringTransaction.GetAllPopulatedAsync(include: q => q.Include(q => q.User));
            var sortedResult = allRecuringTransaction.OrderByDescending(t => t.NextTransactionDate).ThenByDescending(t => t.Amount);
            return sortedResult.ToList();
        }

        public async Task DeleteRecurringTransaction(Guid recurringTransactionId)
        {
            var recurredTransaction = await _unitOfWork.RecurringTransaction.GetAsync(t => t.RecurringTransactionId == recurringTransactionId);
            if (recurredTransaction != null)
            {
                recurredTransaction.IsActive = false;
                recurredTransaction.NextTransactionDate = null;
                recurredTransaction.NextStepUpDate = null;
                recurredTransaction.IsStepUpTransaction = null;
            }
            _unitOfWork.RecurringTransaction.Update(recurredTransaction);
            await _unitOfWork.SaveAsync();
        }

        public async Task<RecurringTransactions> CreateRecurringTransaction(AddTransactionVM addTransactionVM, decimal originalAmount)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var user = await _unitOfWork.User.GetPopulatedAsync(u => u.UserId == userId, include: q => q.Include(t => t.Currency));

            RecurringTransactions recurringTransaction = new RecurringTransactions()
            {
                UserId = addTransactionVM.UserId,
                CategoryId = (Guid)addTransactionVM.CategoryId,
                Frequency = (Core.Enums.RecurrenceFrequency)addTransactionVM.RecurrenceFrequency,
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
                switch ((RecurrenceFrequency)nextStepUpDate)
                {
                    case RecurrenceFrequency.Daily:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddDays(1);
                        break;
                    case RecurrenceFrequency.Weekly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddDays(7);
                        break;
                    case RecurrenceFrequency.Monthly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddMonths(1);
                        break;
                    case RecurrenceFrequency.Quarterly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddMonths(3);
                        break;
                    case RecurrenceFrequency.Yearly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddYears(1);
                        break;
                }
            }


            var nextTransactionDate = addTransactionVM.RecurrenceFrequency;
            switch ((RecurrenceFrequency)nextTransactionDate)
            {
                case RecurrenceFrequency.Daily:
                    recurringTransaction.NextTransactionDate = recurringTransaction.LastExecutedDate.Value.AddDays(1);
                    break;
                case RecurrenceFrequency.Weekly:
                    recurringTransaction.NextTransactionDate = recurringTransaction.LastExecutedDate.Value.AddDays(7);
                    break;
                case RecurrenceFrequency.Monthly:
                    recurringTransaction.NextTransactionDate = recurringTransaction.LastExecutedDate.Value.AddMonths(1);
                    break;
                case RecurrenceFrequency.Quarterly:
                    recurringTransaction.NextTransactionDate = recurringTransaction.LastExecutedDate.Value.AddMonths(3);
                    break;
                case RecurrenceFrequency.Yearly:
                    recurringTransaction.NextTransactionDate = recurringTransaction.LastExecutedDate.Value.AddYears(1);
                    break;
            }


            return recurringTransaction;
        }

        public async Task<RecurringTransactions> EditRecurringTransaction(RecurringTransactionVM recurringTransactionVM)
        {
            var recurringTransaction = await _unitOfWork.RecurringTransaction.GetAsync(r => r.RecurringTransactionId == recurringTransactionVM.RecurringTransactionId);
            recurringTransaction.OriginalAmount = recurringTransactionVM.OriginalAmount;
            recurringTransaction.Frequency = recurringTransactionVM.RecurrenceFrequency;
            recurringTransaction.Description = recurringTransactionVM.Description;

            if (recurringTransactionVM.TransactionTerrority == TransactionTerrority.Domestic)
            {
                recurringTransaction.Amount = (decimal)recurringTransaction.OriginalAmount;
            }
            else
            {
                var userId = _loggedInUser.CurrentLoggedInUser();
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
                switch ((RecurrenceFrequency)nextStepUpDate)
                {
                    case RecurrenceFrequency.Daily:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddDays(1);
                        break;
                    case RecurrenceFrequency.Weekly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddDays(7);
                        break;
                    case RecurrenceFrequency.Monthly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddMonths(1);
                        break;
                    case RecurrenceFrequency.Quarterly:
                        recurringTransaction.NextStepUpDate = recurringTransaction.LastStepUpDate.Value.AddMonths(3);
                        break;
                    case RecurrenceFrequency.Yearly:
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
            switch ((RecurrenceFrequency)nextTransactionDate)
            {
                case RecurrenceFrequency.Daily:
                    recurringTransaction.NextTransactionDate = todaysDate.AddDays(1);
                    break;
                case RecurrenceFrequency.Weekly:
                    recurringTransaction.NextTransactionDate = todaysDate.AddDays(7);
                    break;
                case RecurrenceFrequency.Monthly:
                    recurringTransaction.NextTransactionDate = todaysDate.AddMonths(1);
                    break;
                case RecurrenceFrequency.Quarterly:
                    recurringTransaction.NextTransactionDate = todaysDate.AddMonths(3);
                    break;
                case RecurrenceFrequency.Yearly:
                    recurringTransaction.NextTransactionDate = todaysDate.AddYears(1);
                    break;
            }
            _unitOfWork.RecurringTransaction.Update(recurringTransaction);
            await _unitOfWork.SaveAsync();
            return recurringTransaction;
        }

        public async Task<RecurringTransactionVM> EditView(Guid recurringTransactionId)
        {
            var recurringTransaction = await _unitOfWork.RecurringTransaction.GetAsync(r => r.RecurringTransactionId == recurringTransactionId);
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
