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
        private readonly ICurrencyConversion _currencyConversion;
        public TransactionService(IUnitOfWork unitOfWork, ILoggedInUser loggedInUser, ICurrencyConversion currencyConversion)
        {
            _unitOfWork = unitOfWork;
            _loggedInUser = loggedInUser;
            _currencyConversion = currencyConversion;
        }

        public async Task<AddTransactionVM> CreateView()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var categories = await _unitOfWork.Category.GetAllAsync(u => u.UserId == userId);
            AddTransactionVM addTransactionVM = new AddTransactionVM()
            {
                UserId = userId,
                Categories = (List<Category>)categories,
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
            string currencyUsed;
            Currency findUsedCurrency;
            Currency findUserBaseCurrency;
            ConvertedRates convertedAmount;

            if (addTransactionVM.TransactionTerrority.ToString() == "International")
            {
                currencyUsed = addTransactionVM.TransactionCurrency.ToUpper();
                findUsedCurrency = await _unitOfWork.Currency.GetAsync(c => c.CurrencyCode == currencyUsed);
                findUserBaseCurrency = await _unitOfWork.Currency.GetAsync(c => c.CurrencyId == user.CurrencyId);

                if (findUsedCurrency != findUserBaseCurrency)
                {
                    convertedAmount = await _currencyConversion.GetConvertedRates(findUsedCurrency.CurrencyCode, findUserBaseCurrency.CurrencyCode);
                    addTransactionVM.Amount = convertedAmount.conversion_rate * addTransactionVM.Amount;
                    addTransactionVM.TransactionCurrency = addTransactionVM.TransactionCurrency;
                }
            }

            Transaction transaction = new Transaction()
            {
                UserId = user.UserId,
                Amount = addTransactionVM.Amount,
                Description = addTransactionVM.Description,
                TransactionDate = addTransactionVM.TransactionDate,
                CategoryId = addTransactionVM.CategoryId,
                TransactionCurrency = addTransactionVM.TransactionCurrency,
                TransactionTerrority = addTransactionVM.TransactionTerrority,
                TransactionTimeLine = addTransactionVM.TransactionTimeLine,
                RecurrenceFrequency = addTransactionVM.RecurrenceFrequency
            };

            if (addTransactionVM.TransactionTimeLine.ToString() == "Recurring" && addTransactionVM.RecurringStartDate.HasValue)
            {
                RecurringTransactions recurringTransactions = new RecurringTransactions()
                {
                    Transaction = transaction,
                    TransactionId = transaction.TransactionId,
                    User = user,
                    UserId = addTransactionVM.UserId,
                    Frequency = (Core.Enums.RecurrenceFrequency)addTransactionVM.RecurrenceFrequency,
                    StartDate = addTransactionVM.RecurringStartDate.Value,
                    EndDate = addTransactionVM.RecurringEndDate.Value,
                    LastExecutedDate = DateTime.UtcNow
                };
                var nextTransactionDate = addTransactionVM.RecurrenceFrequency;

                switch ((RecurrenceFrequency)nextTransactionDate)
                {
                    case RecurrenceFrequency.Daily:
                        recurringTransactions.NextTransactionDate = recurringTransactions.LastExecutedDate.Value.AddDays(1);
                        break;
                    case RecurrenceFrequency.Weekly:
                        recurringTransactions.NextTransactionDate = recurringTransactions.LastExecutedDate.Value.AddDays(7);
                        break;
                    case RecurrenceFrequency.Monthly:
                        recurringTransactions.NextTransactionDate = recurringTransactions.LastExecutedDate.Value.AddMonths(1);
                        break;
                    case RecurrenceFrequency.Quarterly:
                        recurringTransactions.NextTransactionDate = recurringTransactions.LastExecutedDate.Value.AddMonths(3);
                        break;
                    case RecurrenceFrequency.Yearly:
                        recurringTransactions.NextTransactionDate = recurringTransactions.LastExecutedDate.Value.AddYears(1);
                        break;
                }

                await _unitOfWork.RecurringTransaction.AddAsync(recurringTransactions);
            }

            await _unitOfWork.Transaction.AddAsync(transaction);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<Transaction>> DisplayTransactions()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            IEnumerable<Transaction> allTransaction = await _unitOfWork.Transaction.GetAllPopulatedAsync(u => u.UserId == userId, include: q => q.Include(t => t.Category));

            return allTransaction;
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
            if (transaction.TransactionCurrency != null && transaction.TransactionCurrency.Length>=1)
            {
                userCurrency = await _unitOfWork.Currency.GetAsync(t => t.CurrencyId == user.CurrencyId);
                userBaseCurrency = userCurrency.CurrencyCode;
                convertedRates = await _currencyConversion.GetConvertedRates(userBaseCurrency, transaction.TransactionCurrency);

                AddTransactionVM addTransactionVM = new AddTransactionVM()
                {
                    UserId=user.UserId,
                    TransactionId=transaction.TransactionId,
                    TransactionCurrency = transaction.TransactionCurrency,
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
                UserId=user.UserId,
                TransactionId=transaction.TransactionId,
                TransactionCurrency = transaction.TransactionCurrency,
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

                _unitOfWork.Transaction.Update(transaction);
                await _unitOfWork.SaveAsync();
            }

        public async Task Delete(Guid transactionId)
        {
            var transaction = await _unitOfWork.Transaction.GetAsync(t => t.TransactionId == transactionId);
            _unitOfWork.Transaction.Delete(transaction);
            await _unitOfWork.SaveAsync();
        }

        public List<CurrencyList> GetAllAvailableCurrency()
        {
            var allavailable = CurrencySymbol.GetCultures();

            var allAvailableCurrency = allavailable.Select(c => new CurrencyList
            {
                CurrencyName = c.CurrencyName,
                CurrencySymbol = c.CurrencySymbol,
                CurrencyCode = c.CurrencyCode
            }).ToList();

            return allAvailableCurrency;
        }


    }
}
