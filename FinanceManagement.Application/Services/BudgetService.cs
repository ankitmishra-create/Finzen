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
                UserBaseCurrency = userBaseCurrencySymbol?.Currency?.CurrencyCode
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
                BudgetName=budgetVM.BudgetName
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
                        BudgetName=budget.BudgetName
                    };
                    userBudgetsList.Add(budgetDto);
                }
            }
            return userBudgetsList;
        }
}
}
