using AspNetCoreGeneratedDocument;
using FinanceManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FinanceManagement.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        public async Task<IActionResult> Index()
        {
            var last5 = await _dashboardService.Last5Transaction();
            var income = await _dashboardService.TotalIncome();
            var expense = await _dashboardService.TotalExpense();
            var available = income - expense;

            ViewBag.Income = income;
            ViewBag.Expense = expense;
            ViewBag.Available = available;
            return View(last5);
        }
    }
}
