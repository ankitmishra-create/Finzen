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
        public RecurringTransactionService(ICurrencyConversionService currencyConversionService,ILoggedInUser loggedInUser, IUnitOfWork unitOfWork)
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
            return allRecuringTransaction.ToList();
        }

        public async Task DeleteRecurringTransaction(Guid recurringTransactionId)
        {
            var recurredTransaction = await _unitOfWork.RecurringTransaction.GetAsync(t => t.RecurringTransactionId == recurringTransactionId);
            if (recurredTransaction != null)
            {
                recurredTransaction.IsActive = false;
                recurredTransaction.NextTransactionDate = null;
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
            
            if(recurringTransactionVM.TransactionTerrority == TransactionTerrority.Domestic)
            {
                recurringTransaction.Amount = (decimal)recurringTransaction.OriginalAmount;
            }
            else
            {
                var userId = _loggedInUser.CurrentLoggedInUser();
                var user = await _unitOfWork.User.GetPopulatedAsync(u => u.UserId==userId,include: q=> q.Include(c => c.Currency));
                if (user == null)
                {
                    return null;
                }
                var convertedRates = await _currencyConversionService.GetConvertedRates(recurringTransaction.OriginalCurrency, user.Currency.CurrencyCode);
                recurringTransaction.Amount = recurringTransactionVM.OriginalAmount * convertedRates.conversion_rate;
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
                RecurringTransactionId=recurringTransaction.RecurringTransactionId,
                OriginalAmount= (decimal)recurringTransaction.OriginalAmount,
                RecurrenceFrequency = recurringTransaction.Frequency,
                TransactionTerrority = recurringTransaction.TransactionTerrority,
                OriginalCurrency= recurringTransaction.OriginalCurrency,
                Description=recurringTransaction.Description
            };
            return recurringTransactionVM;
        }
    }
}
