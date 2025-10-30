using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.Utility;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.External;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggedInUser _loggedInUser;
        private readonly ICurrencyConversionService _currencyConversion;
        private readonly IRecurringTransactionService _recurringTransaction;
        public TransactionService(IRecurringTransactionService recurringTransactions, IUnitOfWork unitOfWork, ILoggedInUser loggedInUser, ICurrencyConversionService currencyConversion)
        {
            _unitOfWork = unitOfWork;
            _loggedInUser = loggedInUser;
            _currencyConversion = currencyConversion;
            _recurringTransaction = recurringTransactions;
        }

        public async Task<AddTransactionVM> CreateView()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var categories = await _unitOfWork.Category.GetAllAsync(u => u.UserId == userId);
            var user = await _unitOfWork.User.GetAsync(u => u.UserId == userId);
            var userBaseCurrencySymbol = await _unitOfWork.Currency.GetAsync(c => c.CurrencyId == user.CurrencyId);

            AddTransactionVM addTransactionVM = new AddTransactionVM()
            {
                UserId = userId,
                Categories = (List<Category>)categories,
                TransactionTerrority = TransactionTerrority.Domestic,
                SelectedCurrency = userBaseCurrencySymbol.CurrencyCode,
                TransactionTimeLine = TransactionTimeLine.OneTime,
                IsStepUpTransaction = false
            };
            return addTransactionVM;
        }

        public async Task AddTransactionAsync(AddTransactionVM addTransactionVM)
        {
            var user = await _unitOfWork.User.GetByIdAsync(addTransactionVM.UserId);
            if (user == null)
            {
                return;
            }
            var category = await _unitOfWork.Category.GetAsync(u => u.CategoryId == addTransactionVM.CategoryId);
            if (category == null)
            {
                return;
            }

            var userBasecurrency = await _unitOfWork.Currency.GetAsync(c => c.CurrencyId == user.CurrencyId);
            if (addTransactionVM.TransactionTerrority.ToString() == "Domestic")
            {
                addTransactionVM.SelectedCurrency = userBasecurrency.CurrencyCode;
            }

            decimal originalAmount = (decimal)addTransactionVM.Amount;

            string currencyUsed;
            Currency findUsedCurrency;
            Currency findUserBaseCurrency = await _unitOfWork.Currency.GetAsync(c => c.CurrencyId == user.CurrencyId);
            ConvertedRates convertedAmount;

            if (addTransactionVM.TransactionTerrority.ToString() == "International")
            {
                currencyUsed = addTransactionVM.SelectedCurrency.ToUpper();
                findUsedCurrency = await _unitOfWork.Currency.GetAsync(c => c.CurrencyCode == currencyUsed);

                if (findUsedCurrency != findUserBaseCurrency)
                {
                    convertedAmount = await _currencyConversion.GetConvertedRates(findUsedCurrency.CurrencyCode, findUserBaseCurrency.CurrencyCode);
                    addTransactionVM.Amount = convertedAmount.conversion_rate * addTransactionVM.Amount;
                    addTransactionVM.SelectedCurrency = addTransactionVM.SelectedCurrency;
                }

            }

            Transaction transaction = new Transaction()
            {
                UserId = user.UserId,
                Amount = addTransactionVM.Amount,
                Description = addTransactionVM.Description,
                TransactionDate = addTransactionVM.TransactionDate,
                CategoryId = addTransactionVM.CategoryId,
                SelectedCurrency = addTransactionVM.SelectedCurrency,
                TransactionTerrority = addTransactionVM.TransactionTerrority,
                TransactionTimeLine = addTransactionVM.TransactionTimeLine,
                RecurrenceFrequency = addTransactionVM.RecurrenceFrequency,
                OriginalAmount = originalAmount,
                OriginalCurrency = findUserBaseCurrency.CurrencyCode,
            };

            if (addTransactionVM.TransactionTimeLine.ToString() == "Recurring" && addTransactionVM.RecurringStartDate.HasValue)
            {
                var recurringTransaction = await _recurringTransaction.CreateRecurringTransaction(addTransactionVM, originalAmount);
                transaction.GeneratedFromRecurringId = recurringTransaction.RecurringTransactionId;
                await _unitOfWork.RecurringTransaction.AddAsync(recurringTransaction);
            }
            await _unitOfWork.Transaction.AddAsync(transaction);
            TransactionLog(transaction, ActionPerformed.Created);
            await _unitOfWork.SaveAsync();
        }

        public void TransactionLog(Transaction transaction, ActionPerformed actionPerformed)
        {
            TransactionLog transactionLog = new TransactionLog()
            {
                TransactionId = transaction.TransactionId,
                UserId = transaction.UserId,
                User = transaction.User,
                CategoryId = transaction.CategoryId,
                Category = transaction.Category,
                TransactionTerrority = transaction.TransactionTerrority,
                SelectedCurrency = transaction.SelectedCurrency,
                TransactionTimeLine = transaction.TransactionTimeLine,
                RecurrenceFrequency = transaction.RecurrenceFrequency,
                Amount = transaction.Amount,
                OriginalAmount = transaction.OriginalAmount,
                OriginalCurrency = transaction.OriginalCurrency,
                Description = transaction.Description,
                TransactionDate = transaction.TransactionDate,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt,
                GeneratedFromRecurringId = transaction.GeneratedFromRecurringId,
                GeneratedFromRecurring = transaction.GeneratedFromRecurring,
                IsAutoGenerated = transaction.IsAutoGenerated,
                ActionPerformed = actionPerformed,
                ActionDate = DateTime.UtcNow
            };
            _unitOfWork.TransactionLog.AddAsync(transactionLog);
        }

        public async Task<IEnumerable<Transaction>> DisplayTransactions()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            IEnumerable<Transaction> allTransaction = await _unitOfWork.Transaction.GetAllPopulatedAsync(u => u.UserId == userId, include: q => q.Include(t => t.Category));
            var sortedResult = allTransaction.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Amount);
            return sortedResult;
        }

        public async Task<AddTransactionVM> EditView(Guid transactionId)
        {
            var transaction = await _unitOfWork.Transaction.GetAsync(t => t.TransactionId == transactionId);
            var userId = _loggedInUser.CurrentLoggedInUser();
            var categories = await _unitOfWork.Category.GetAllAsync(u => u.UserId == userId);
            var user = await _unitOfWork.User.GetByIdAsync(userId);

            Currency userCurrency;
            string userBaseCurrency;
            ConvertedRates convertedRates;
            decimal convertedUpdatedRates = 0;
            if (transaction.SelectedCurrency != null && transaction.SelectedCurrency.Length >= 1)
            {
                userCurrency = await _unitOfWork.Currency.GetAsync(t => t.CurrencyId == user.CurrencyId);
                userBaseCurrency = userCurrency.CurrencyCode;
                convertedRates = await _currencyConversion.GetConvertedRates(userBaseCurrency, transaction.SelectedCurrency);

                AddTransactionVM addTransactionVM = new AddTransactionVM()
                {
                    UserId = user.UserId,
                    TransactionId = transaction.TransactionId,
                    SelectedCurrency = transaction.SelectedCurrency,
                    TransactionTerrority = transaction.TransactionTerrority,
                    Categories = (List<Category>)categories,
                    Amount = convertedRates.conversion_rate * transaction.Amount,
                    CategoryId = transaction.CategoryId,
                    Description = transaction.Description,
                    CategoryType = transaction.Category.CategoryType,
                    TransactionDate = transaction.TransactionDate,
                };
                return addTransactionVM;
            }

            AddTransactionVM addTransactionVM1 = new AddTransactionVM()
            {
                UserId = user.UserId,
                TransactionId = transaction.TransactionId,
                SelectedCurrency = transaction.SelectedCurrency,
                TransactionTerrority = transaction.TransactionTerrority,
                Categories = (List<Category>)categories,
                Amount = transaction.Amount,
                CategoryId = transaction.CategoryId,
                Description = transaction.Description,
                CategoryType = transaction.Category.CategoryType,
                TransactionDate = transaction.TransactionDate,
            };
            return addTransactionVM1;
        }

        public async Task Edit(AddTransactionVM addTransactionVM)
        {
            var user = await _unitOfWork.User.GetByIdAsync(addTransactionVM.UserId);
            await _unitOfWork.User.GetAllPopulatedAsync(include: q => q.Include(t => t.Currency));
            if (user == null)
            {
                return;
            }
            var category = await _unitOfWork.Category.GetAsync(u => u.CategoryId == addTransactionVM.CategoryId);
            if (category == null)
            {
                return;
            }
            var transaction = await _unitOfWork.Transaction.GetAsync(t => t.TransactionId == addTransactionVM.TransactionId);
            transaction.TransactionDate = addTransactionVM.TransactionDate;
            transaction.Amount = addTransactionVM.Amount;
            transaction.Description = addTransactionVM.Description;
            transaction.CategoryId = addTransactionVM.CategoryId;
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.TransactionTerrority = addTransactionVM.TransactionTerrority;

            string currencyUsed;
            Currency findUsedCurrency;
            Currency findUserBaseCurrency;
            ConvertedRates convertedAmount;
            if (addTransactionVM.TransactionTerrority.ToString() == "Domestic")
            {
                transaction.SelectedCurrency = user.Currency.CurrencyCode;
            }
            else
            {
                transaction.SelectedCurrency = addTransactionVM.SelectedCurrency;
                currencyUsed = addTransactionVM.SelectedCurrency.ToUpper();
                findUsedCurrency = await _unitOfWork.Currency.GetAsync(c => c.CurrencyCode == currencyUsed);
                findUserBaseCurrency = await _unitOfWork.Currency.GetAsync(c => c.CurrencyId == user.CurrencyId);

                if (findUsedCurrency != findUserBaseCurrency)
                {
                    convertedAmount = await _currencyConversion.GetConvertedRates(findUsedCurrency.CurrencyCode, findUserBaseCurrency.CurrencyCode);
                    addTransactionVM.Amount = convertedAmount.conversion_rate * addTransactionVM.Amount;
                    addTransactionVM.SelectedCurrency = addTransactionVM.SelectedCurrency;
                }
                transaction.SelectedCurrency = addTransactionVM.SelectedCurrency;
                transaction.Amount = addTransactionVM.Amount;

            }
            _unitOfWork.Transaction.Update(transaction);
            TransactionLog(transaction, ActionPerformed.Edited);
            await _unitOfWork.SaveAsync();
        }

        public async Task Delete(Guid transactionId)
        {
            var transaction = await _unitOfWork.Transaction.GetAsync(t => t.TransactionId == transactionId);
            _unitOfWork.Transaction.Delete(transaction);
            TransactionLog(transaction, ActionPerformed.Deleted);
            await _unitOfWork.SaveAsync();
        }
        public List<CurrencyData> GetAllAvailableCurrency()
        {
            var allavailable = CurrencySymbol.GetCultures();

            var allAvailableCurrency = allavailable.Select(c => new CurrencyData
            {
                CurrencyName = c.CurrencyName,
                CurrencySymbol = c.CurrencySymbol,
                CurrencyCode = c.CurrencyCode
            }).ToList();

            return allAvailableCurrency;
        }


    }
}
