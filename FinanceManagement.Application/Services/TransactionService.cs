using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.Utility;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.External;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggedInUser _loggedInUser;
        private readonly ICurrencyConversionService _currencyConversion;
        private readonly IRecurringTransactionService _recurringTransaction;
        private readonly ILogger<TransactionService> _logger;
        public TransactionService(ILogger<TransactionService> logger, IRecurringTransactionService recurringTransactions, IUnitOfWork unitOfWork, ILoggedInUser loggedInUser, ICurrencyConversionService currencyConversion)
        {
            _unitOfWork = unitOfWork;
            _loggedInUser = loggedInUser;
            _currencyConversion = currencyConversion;
            _recurringTransaction = recurringTransactions;
            _logger = logger;
        }

        public async Task<AddTransactionVM> CreateView()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            try
            {
                var user = await _unitOfWork.User.GetAsync(u => u.UserId == userId);
                var categories = await _unitOfWork.Category.GetAllAsync(u => u.UserId == userId);
                var userBaseCurrencySymbol = await _unitOfWork.Currency.GetAsync(c => c.CurrencyId == user.CurrencyId);
                if (user?.Currency == null)
                {
                    _logger.LogError("User {UserId} has no base currency configured.", userId);
                    throw new InvalidCurrencyException("User base currency not set.");
                }

                var userSavings = await _unitOfWork.Saving.GetAllAsync(s => s.UserId == userId);
                var userBudget = await _unitOfWork.Budget.GetAllAsync(b => b.UserId == userId);
                AddTransactionVM addTransactionVM = new AddTransactionVM()
                {
                    UserId = userId,
                    Categories = (List<Category>)categories,
                    TransactionTerrority = TransactionTerrority.Domestic,
                    SelectedCurrency = userBaseCurrencySymbol?.CurrencyCode,
                    TransactionTimeLine = TransactionTimeLine.OneTime,
                    IsStepUpTransaction = false
                };

                if (userSavings != null)
                {
                    addTransactionVM.AvailableSavings = userSavings;
                }
                if (userBudget != null)
                {
                    addTransactionVM.AvailableBudgets = userBudget;
                }

                return addTransactionVM;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro loading CreateView for user {UserId}", userId);
                throw new DataRetrievalException("Error loading transaction data.", ex);
            }
        }

        private async Task<List<TransactionSavingsOrBudgetsMapping>> HandleSavingOrBudgetTransaction(AddTransactionVM addTransactionVM, Guid transactionId, decimal? convertedAmount)
        {
            List<TransactionSavingsOrBudgetsMapping> transactionSavingsOrBudgets = new List<TransactionSavingsOrBudgetsMapping>();
            var userId = _loggedInUser.CurrentLoggedInUser();
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError($"HandleSavingOrBudgetTransaction: Requested user not found: {userId}");
                throw new UserNotFoundException($"Requested User Not Found{userId}");
            }
            if (addTransactionVM.IsForSaving)
            {
                foreach (var item in addTransactionVM.SavingAllocationVms)
                {
                    if (item.Amount.HasValue && item.Amount > 0)
                    {
                        var userSaving = await _unitOfWork.Saving.GetAsync(s => s.SavingId == item.AllocatedSavigId);
                        if (userSaving == null)
                        {
                            _logger.LogError($"HandleSavingOrBudgetTransaction: Requested Saving not found {item.AllocatedSavigId}");
                            throw new SavingNotFoundException($"Requested Saving not found {item.AllocatedSavigId}");
                        }
                        if (convertedAmount != null)
                        {
                            item.Amount *= (decimal)convertedAmount;
                        }
                        userSaving.AlreadySavedAmount += (decimal)item.Amount;
                        TransactionSavingsOrBudgetsMapping transactionSavingsOrBudgetsMapping = new TransactionSavingsOrBudgetsMapping()
                        {
                            SavingId = item.AllocatedSavigId,
                            SavedAmount = item.Amount,
                            TransactionId = transactionId
                        };
                        _unitOfWork.Saving.Update(userSaving);
                        transactionSavingsOrBudgets.Add(transactionSavingsOrBudgetsMapping);
                    }
                }
            }
            if (addTransactionVM.IsForBudget)
            {
                foreach (var item in addTransactionVM?.BudgetAllocationVMs)
                {

                    if (item.Amount > 0M)
                    {
                        var userBudget = await _unitOfWork.Budget.GetAsync(s => s.BudgetId == item.AllocatedBudgetId);
                        if (userBudget == null)
                        {
                            _logger.LogError($"HandleSavingOrBudgetTransaction: Requested budget not found {item.AllocatedBudgetId}");
                            throw new SavingNotFoundException($"Requested Saving not found {item.AllocatedBudgetId}");
                        }
                        if (convertedAmount != null)
                        {
                            item.Amount *= (decimal)convertedAmount;
                        }
                        userBudget.AlreadySpendAmount += (decimal)item.Amount;
                        TransactionSavingsOrBudgetsMapping transactionSavingsOrBudgetsMapping = new TransactionSavingsOrBudgetsMapping()
                        {
                            BudgetId = item.AllocatedBudgetId,
                            SavedAmount = item.Amount,
                            TransactionId = transactionId
                        };
                        _unitOfWork.Budget.Update(userBudget);
                        transactionSavingsOrBudgets.Add(transactionSavingsOrBudgetsMapping);
                    }
                }
            }
            return transactionSavingsOrBudgets;
        }

        public async Task AddTransactionAsync(AddTransactionVM addTransactionVM)
        {
            var user = await _unitOfWork.User.GetByIdAsync(addTransactionVM.UserId);
            if (user == null)
            {
                _logger.LogError("AddTransaction: User not found");
                throw new UserNotFoundException("Requested User Not Found");
            }
            var category = await _unitOfWork.Category.GetAsync(u => u.CategoryId == addTransactionVM.CategoryId);
            if (category == null)
            {
                _logger.LogError("AddTransaction: Category not found");
                throw new CategoryNotFoundException("Requested Category not Found");
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
            decimal convertedRates = 1m;
            if (addTransactionVM.TransactionTerrority.ToString() == "International")
            {
                currencyUsed = addTransactionVM.SelectedCurrency.ToUpper();
                findUsedCurrency = await _unitOfWork.Currency.GetAsync(c => c.CurrencyCode == currencyUsed);

                if (findUsedCurrency != findUserBaseCurrency)
                {
                    convertedAmount = await _currencyConversion.GetConvertedRates(findUsedCurrency.CurrencyCode, findUserBaseCurrency.CurrencyCode);
                    addTransactionVM.Amount = convertedAmount.conversion_rate * addTransactionVM.Amount;
                    addTransactionVM.SelectedCurrency = addTransactionVM.SelectedCurrency;
                    convertedRates = convertedAmount.conversion_rate;
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

            if (addTransactionVM.IsForSaving || addTransactionVM.IsForBudget)
            {
                var mappedTransaction = await HandleSavingOrBudgetTransaction(addTransactionVM, transaction.TransactionId, convertedRates);
                foreach (var item in mappedTransaction)
                {
                    await _unitOfWork.Mapping.AddAsync(item);

                }
            }

            if (addTransactionVM.TransactionTimeLine.ToString() == "Recurring" && addTransactionVM.RecurringStartDate.HasValue)
            {
                var recurringTransaction = await _recurringTransaction.CreateRecurringTransaction(addTransactionVM, originalAmount);
                transaction.GeneratedFromRecurringId = recurringTransaction.RecurringTransactionId;
                await _unitOfWork.RecurringTransaction.AddAsync(recurringTransaction);
            }
            await _unitOfWork.Transaction.AddAsync(transaction);
            TransactionLog(transaction, ActionPerformed.Created);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database Update Exception");
            }
            catch (Exception ex)
            {
                _logger.LogError("AddTransaction: Unexpected Error Occured");
            }
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
            try
            {
                IEnumerable<Transaction> allTransaction = await _unitOfWork.Transaction.GetAllPopulatedAsync(u => u.UserId == userId, include: q => q.Include(t => t.Category));
                var sortedResult = allTransaction.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Amount);
                return sortedResult;
            }
            catch (TransactionNotFoundException ex)
            {
                _logger.LogError(ex, "Display Transaction: Transaction Not Found");
                throw new TransactionNotFoundException("Transaction Not Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error displaying transactions for user {UserId}", userId);
                throw new DataRetrievalException("Error loading transactions.", ex);
            }
        }

        public async Task<AddTransactionVM> EditView(Guid transactionId)
        {
            var transaction = await _unitOfWork.Transaction.GetAsync(t => t.TransactionId == transactionId);
            var userId = _loggedInUser.CurrentLoggedInUser();
            var categories = await _unitOfWork.Category.GetAllAsync(u => u.UserId == userId);
            var user = await _unitOfWork.User.GetByIdAsync(userId);

            var savingMapped = await _unitOfWork.Mapping.GetAllPopulatedAsync(sm => sm.TransactionId == transactionId && sm.SavingId.HasValue, include: q => q.Include(s => s.Saving));
            var budgetMapped = await _unitOfWork.Mapping.GetAllPopulatedAsync(bm => bm.TransactionId == transactionId && bm.BudgetId.HasValue, include: q => q.Include(b => b.Budget));

            var savings = savingMapped.Select(x => x.Saving).Where(s => s != null).ToList();
            var budgets = budgetMapped.Select(x => x.Budget).Where(b => b != null).ToList();


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

                if (savingMapped != null)
                {
                    List<SavingAllocationVm> savingAllocationVms = new List<SavingAllocationVm>();
                    foreach (var item in savingMapped)
                    {
                        SavingAllocationVm savingAllocationVm = new SavingAllocationVm()
                        {
                            AllocatedSavigId = (Guid)item.SavingId,
                            Amount = item.SavedAmount * convertedRates.conversion_rate
                        };
                        savingAllocationVms.Add(savingAllocationVm);
                    }
                    addTransactionVM.AvailableSavings = savings;
                    addTransactionVM.SavingAllocationVms = savingAllocationVms;
                }

                if (budgetMapped != null)
                {
                    List<BudgetAllocationVM> budgetAllocationVMs = new List<BudgetAllocationVM>();
                    foreach (var item in budgetMapped)
                    {
                        BudgetAllocationVM budgetAllocationVM = new BudgetAllocationVM()
                        {
                            AllocatedBudgetId = (Guid)item.BudgetId,
                            Amount = item.SavedAmount * convertedRates.conversion_rate
                        };
                        budgetAllocationVMs.Add(budgetAllocationVM);
                    }
                    addTransactionVM.AvailableBudgets = budgets;
                    addTransactionVM.BudgetAllocationVMs = budgetAllocationVMs;
                }

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

            if (savingMapped != null)
            {
                List<SavingAllocationVm> savingAllocationVms = new List<SavingAllocationVm>();
                foreach (var item in savingMapped)
                {
                    SavingAllocationVm savingAllocationVm = new SavingAllocationVm()
                    {
                        AllocatedSavigId = (Guid)item.SavingId,
                        Amount = item.SavedAmount * convertedUpdatedRates
                    };
                    addTransactionVM1.AvailableSavings = savings;
                    savingAllocationVms.Add(savingAllocationVm);
                }
                addTransactionVM1.SavingAllocationVms = savingAllocationVms;
            }

            if (budgetMapped != null)
            {
                List<BudgetAllocationVM> budgetAllocationVMs = new List<BudgetAllocationVM>();
                foreach (var item in budgetMapped)
                {
                    BudgetAllocationVM budgetAllocationVM = new BudgetAllocationVM()
                    {
                        AllocatedBudgetId = (Guid)item.BudgetId,
                        Amount = item.SavedAmount * convertedUpdatedRates
                    };
                    budgetAllocationVMs.Add(budgetAllocationVM);
                }
                addTransactionVM1.BudgetAllocationVMs = budgetAllocationVMs;
            }

            return addTransactionVM1;
        }

        public async Task Edit(AddTransactionVM addTransactionVM)
        {
            var user = await _unitOfWork.User.GetByIdAsync(addTransactionVM.UserId);
            await _unitOfWork.User.GetAllPopulatedAsync(include: q => q.Include(t => t.Currency));
            if (user == null)
            {
                _logger.LogError("AddTransaction: User not found");
                throw new UserNotFoundException("Requested User Not Found");
            }
            var category = await _unitOfWork.Category.GetAsync(u => u.CategoryId == addTransactionVM.CategoryId);
            if (category == null)
            {
                _logger.LogError("AddTransaction: Category not found");
                throw new CategoryNotFoundException("Requested Category not Found");
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
            decimal convertedRates = 1;
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
                    convertedRates = convertedAmount.conversion_rate;
                }
                transaction.SelectedCurrency = addTransactionVM.SelectedCurrency;
                transaction.Amount = addTransactionVM.Amount;

            }

            if (addTransactionVM.BudgetAllocationVMs?.Count > 0)
            {
                foreach (var item in addTransactionVM.BudgetAllocationVMs)
                {
                    if (item != null)
                    {
                        if (item.Amount == null) item.Amount = 0;
                        var userMappedBudget = await _unitOfWork.Mapping.GetAsync(m => m.TransactionId == addTransactionVM.TransactionId && m.BudgetId == item.AllocatedBudgetId);
                        userMappedBudget.SavedAmount = item.Amount*=convertedRates;
                        if (userMappedBudget.SavedAmount == 0)
                        {
                           _unitOfWork.Mapping.Delete(userMappedBudget);
                        }
                        else
                        {
                            _unitOfWork.Mapping.Update(userMappedBudget);
                        }
                    }
                }
            }
            else if (addTransactionVM.SavingAllocationVms?.Count > 0)
            {
                foreach (var item in addTransactionVM.SavingAllocationVms)
                {
                    if (item != null)
                    {
                        if (item.Amount == null) item.Amount = 0;
                        var userMappingSaving = await _unitOfWork.Mapping.GetAsync(m => m.TransactionId == addTransactionVM.TransactionId && m.SavingId == item.AllocatedSavigId);
                        userMappingSaving.SavedAmount = item.Amount*convertedRates;
                        if (userMappingSaving.SavedAmount == 0)
                        {
                            _unitOfWork.Mapping.Delete(userMappingSaving);
                        }
                        else
                        {
                            _unitOfWork.Mapping.Update(userMappingSaving);
                        }
                    }
                }
            }

            _unitOfWork.Transaction.Update(transaction);
            TransactionLog(transaction, ActionPerformed.Edited);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database Update Exception");
            }
            catch (Exception ex)
            {
                _logger.LogError("AddTransaction: Unexpected Error Occured");
            }
        }

        public async Task Delete(Guid transactionId)
        {
            try
            {
                var transaction = await _unitOfWork.Transaction.GetAsync(t => t.TransactionId == transactionId);
                _unitOfWork.Transaction.Delete(transaction);
                TransactionLog(transaction, ActionPerformed.Deleted);
                await _unitOfWork.SaveAsync();
            }
            catch (TransactionNotFoundException ex)
            {
                _logger.LogError(ex, "Display Transaction: Transaction Not Found");
                throw new TransactionNotFoundException("Transaction Not Found");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database Update Exception");
            }
            catch (Exception ex)
            {
                _logger.LogError("AddTransaction: Unexpected Error Occured");
            }

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
