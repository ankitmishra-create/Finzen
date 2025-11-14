using FinanceManagement.Application.Exceptions;
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
    public async Task<IActionResult> Index()
    {
        try
        {
            var investmentTransactionVm = await _investmentService.PrepareView();
            return View(investmentTransactionVm);
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogError(ex,"User not found");
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Unexpected error"); 
            return View("Error");
        }
    }

    public async Task<IActionResult> Add(InvestmentTransactionVM investmentTransactionVm)
    {
        try
        {
            var investment = await _investmentService.AddInvestment(investmentTransactionVm);
            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Unexpected error in StockHoldingController Add method");
            return View("Error");
        }
    }
    
}