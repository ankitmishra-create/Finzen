using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Web.Controllers;

public class InvestmentController : Controller
{
    private readonly IInvestmentService _investmentService;
    private readonly ILogger<InvestmentController> _logger;
    public InvestmentController(IInvestmentService investmentService, ILogger<InvestmentController> logger)
    {
        _investmentService = investmentService;
        _logger = logger;
    }
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Add(InvestmentTransactionVM investmentTransactionVm)
    {
        try
        {
            var investment = await _investmentService.AddInvestment(investmentTransactionVm);
            return View();
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Unexpected error in StockHoldingController Add method");
            return View("Error");
        }
    }
    
}