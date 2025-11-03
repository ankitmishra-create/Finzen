using FinanceManagement.Application.DTO;
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
    public class BudgetService : IBudgetService
    {
        private readonly ILoggedInUser _loggedInUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BudgetService> _logger;
        public BudgetService(ILoggedInUser loggedInUser, IUnitOfWork unitOfWork, ILogger<BudgetService> logger)
        {
            _loggedInUser = loggedInUser;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BudgetVM> CreateVM()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            if (userId == Guid.Empty)
            {
                _logger.LogError("BudgetService: User Id not Found");
                throw new UserNotFoundException("User Id not Found while creating view for Budget");
            }

            var userBaseCurrencySymbol = await _unitOfWork.User.GetPopulatedAsync(u => u.UserId == userId, include: q => q.Include(c => c.Currency));
            var userCategories = await _unitOfWork.Category.GetAllAsync(c => c.UserId == userId);
            if (userCategories.ToList().Count == 0)
            {
                userCategories = new List<Category>();
            }
            BudgetVM budgetVM = new BudgetVM()
            {
                UserId = userId,
                UserCategories = userCategories.ToList(),
                CustomFrequency = false,
                UserBaseCurrency = userBaseCurrencySymbol?.Currency?.CurrencyCode,
            };
            return budgetVM;
        }

        public async Task<Budget> CreateBudget(BudgetVM budgetVM)
        {
            if (budgetVM == null)
            {
                _logger.LogError("Null BudgetVm is passed to create a budget");
                throw new NullReferenceException("Budget VM is null");
            }
            Budget budget = new Budget()
            {
                UserId = budgetVM.UserId,
                CategoryId = budgetVM.CategoryId,
                FrequencyOfBudget = budgetVM.Frequency,
                BudgetAmount = budgetVM.Amount,
                CustomBudget = budgetVM.CustomFrequency,
                Description = budgetVM.Description,
                AlreadySpendAmount = budgetVM.AlreadySpentAmount,
                BudgetName = budgetVM.BudgetName
            };

            if (budgetVM.Frequency.HasValue && budgetVM.CustomFrequency == false)
            {
                var budgetEndDate = budgetVM.Frequency;
                switch ((Frequency)budgetEndDate)
                {
                    case Frequency.Daily:
                        budget.BudgetEndDate = budget.BudgetStartDate.AddDays(1);
                        break;
                    case Frequency.Weekly:
                        budget.BudgetEndDate = budget.BudgetStartDate.AddDays(7);
                        break;
                    case Frequency.Monthly:
                        budget.BudgetEndDate = budget.BudgetStartDate.AddMonths(1);
                        break;
                    case Frequency.Quarterly:
                        budget.BudgetEndDate = budget.BudgetStartDate.AddMonths(3);
                        break;
                    case Frequency.Yearly:
                        budget.BudgetEndDate = budget.BudgetStartDate.AddYears(1);
                        break;
                }
            }
            else
            {
                budget.BudgetStartDate = (DateTime)budgetVM.BudgetStartDate;
                budget.BudgetEndDate = (DateTime)budgetVM.BudgetEndDate;
            }

            if (budgetVM.CategorySwitch == false)
            {
                budget.CategoryId = null;
            }
            await _unitOfWork.Budget.AddAsync(budget);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "CreateBudget: Db update Exception occured");
                throw;
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "CreateBudget: Some Database Exception Occured");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateBudget: Some unexpected exception occured");
                throw;
            }
            return budget;
        }

        public async Task<List<BudgetDto>> GetUserBudgets()
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            if (userId == Guid.Empty)
            {
                _logger.LogError("GetUserBudgets: Invalid User accessing budgets");
                throw new UserNotFoundException("Invalid user id accessing budgets");
            }
            var budgets = await _unitOfWork.Budget.GetAllPopulatedAsync(u => u.UserId == userId, include: q => q.Include(c => c.Category));
            List<BudgetDto> userBudgetsList = new List<BudgetDto>();

            foreach (var budget in budgets)
            {
                if (budget != null)
                {
                    BudgetDto budgetDto = new BudgetDto()
                    {
                        CategoryName = budget?.Category?.CategoryName,
                        CategoryType = budget?.Category?.CategoryType,
                        BudgetAmount = budget.BudgetAmount,
                        AlreadySpendAmount = budget.AlreadySpendAmount,
                        BudgetEndDate = budget.BudgetEndDate,
                        BudgetStartDate = budget.BudgetStartDate,
                        BudgetName = budget.BudgetName,
                        Description = budget.Description,
                        BudgetId = budget.BudgetId
                    };
                    userBudgetsList.Add(budgetDto);
                }
            }
            return userBudgetsList;
        }

        public async Task<BudgetVM> EditView(Guid BudgetId)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var budgetToEdit = await _unitOfWork.Budget.GetPopulatedAsync(b => b.BudgetId == BudgetId && b.UserId == userId, include: q => q.Include(u => u.User));
            if (budgetToEdit == null)
            {
                _logger.LogError("EditVM: Requested budget is not found");
                throw new BudgetNotFoundException("The requested budget is not found");
            }
            BudgetVM budgetVM = new BudgetVM()
            {
                BudgetId = budgetToEdit.BudgetId,
                UserId = budgetToEdit.UserId,
                CategoryId = budgetToEdit.CategoryId,
                Frequency = budgetToEdit.FrequencyOfBudget,
                BudgetName = budgetToEdit.BudgetName,
                CustomFrequency = (bool)budgetToEdit.CustomBudget,
                BudgetEndDate = budgetToEdit.BudgetEndDate,
                BudgetStartDate = budgetToEdit.BudgetStartDate,
                Amount = budgetToEdit.BudgetAmount,
                AlreadySpentAmount = budgetToEdit.AlreadySpendAmount,
                Description = budgetToEdit.Description,
                UserBaseCurrency = budgetToEdit?.User?.Currency?.CurrencyCode
            };
            if (budgetVM.CategoryId != null)
            {
                budgetVM.CategorySwitch = true;
            }
            else
            {
                budgetVM.CategorySwitch = false;
                budgetVM.CategoryId = null;
            }
            var userCategories = await _unitOfWork.Category.GetAllAsync(c => c.UserId == userId && c.CategoryType == CategoryType.Expense);
            budgetVM.UserCategories = userCategories.ToList();
            return budgetVM;
        }

        public async Task<Budget> EditBudget(BudgetVM budgetVM)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var originalbudget = await _unitOfWork.Budget.GetAsync(b => b.BudgetId == budgetVM.BudgetId && b.UserId == userId);
            if (originalbudget == null)
            {
                _logger.LogError("Original Budget not found");
                throw new BudgetNotFoundException("Edit Budget: Requested Budget not found to edit");
            }
            originalbudget.BudgetName = budgetVM.BudgetName;
            if (originalbudget.CategoryId == null && budgetVM.CategorySwitch == true)
            {
                originalbudget.CategoryId = budgetVM.CategoryId;
            }
            else if (originalbudget.CategoryId != null)
            {
                originalbudget.CategoryId = budgetVM.CategoryId;
            }
            originalbudget.BudgetAmount = budgetVM.Amount;
            originalbudget.AlreadySpendAmount = budgetVM.AlreadySpentAmount;
            originalbudget.Description = budgetVM.Description;

            _unitOfWork.Budget.Update(originalbudget);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "EditBudget: Db Update failed");
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EditBudget: Unexpected exception occured");
                throw ex;
            }
            return originalbudget;
        }

        public async Task DeleteBudget(Guid budgetId)
        {
            var userId = _loggedInUser.CurrentLoggedInUser();
            var budgetToDelete = await _unitOfWork.Budget.GetAsync(b => b.BudgetId == budgetId && b.UserId == userId);
            if (budgetToDelete == null)
            {
                _logger.LogError($"Delete Budget: Budget to delete not found for budgetId {budgetId}");
                throw new BudgetNotFoundException($"Budget not found for id {budgetId}");
            }
            _unitOfWork.Budget.Delete(budgetToDelete);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DeleteBudget: Db Update failed");
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteBudget: Unexpected exception occured");
                throw ex;
            }
        }
    }
}
