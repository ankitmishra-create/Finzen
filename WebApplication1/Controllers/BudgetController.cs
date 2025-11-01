using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Web.Controllers
{
    public class BudgetController : Controller
    {
        private readonly IBudgetService _budgetService;
        private readonly ILogger<BudgetController> _logger;

        public BudgetController(IBudgetService budgetService,ILogger<BudgetController> logger)
        {
            _budgetService = budgetService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var budgets = await _budgetService.GetUserBudgets();
            return View(budgets);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var budgetVM = await _budgetService.CreateVM();
                return View(budgetVM);
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogError(ex,"User Not Found for the expected User");
                return View("Error");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Unexpected Error Occured");
                return View("Error");
            }   
        }
        [HttpPost]

        public async Task<IActionResult> Create(BudgetVM budgetVM)
        {
            if (budgetVM.CustomFrequency)
            {
                if (!budgetVM.BudgetStartDate.HasValue || !budgetVM.BudgetEndDate.HasValue)
                {
                    ModelState.AddModelError(nameof(budgetVM.BudgetStartDate), "Start and End Dates are required when using custom frequency.");
                }
                else if (budgetVM.BudgetEndDate <= budgetVM.BudgetStartDate)
                {
                    ModelState.AddModelError(nameof(budgetVM.BudgetEndDate), "End Date must be greater than Start Date.");
                }
            }
            else
            {
                budgetVM.BudgetStartDate = null;
                budgetVM.BudgetEndDate = null;
                if (budgetVM.Frequency == null)
                {
                    ModelState.AddModelError(nameof(budgetVM.Frequency), "Budget Frequency is required when not using custom frequency.");
                }
            }

            if (budgetVM.CategorySwitch && !budgetVM.CategoryId.HasValue)
            {
                ModelState.AddModelError(nameof(budgetVM.CategoryId), "Select Category is required when assigning to a category.");
            }
            if (budgetVM.Amount <= 0)
            {
                ModelState.AddModelError(nameof(budgetVM.Amount), "Budget amount must be greater than zero.");
            }
            if (string.IsNullOrWhiteSpace(budgetVM.Description))
            {
                ModelState.AddModelError(nameof(budgetVM.Description), "Description is required.");
            }
            await _budgetService.CreateBudget(budgetVM);
            return RedirectToAction("Index");
        }

    }
}
