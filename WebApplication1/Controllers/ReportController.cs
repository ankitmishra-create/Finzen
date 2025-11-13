using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Web.Controllers;

public class ReportController : Controller
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;
    public ReportController(IReportService reportService,ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var reportVm = await _reportService.PrepareView();
            return View(reportVm);
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogError(ex,"Report Controller: User not found");
            return RedirectToAction("Index", "Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Report Controller: Error");
            return RedirectToAction("Index", "Error");
        }
        
    }

    [HttpPost]
    public async Task<IActionResult> Index(ReportVM  reportVM)
    {
        try
        {
            var reportVm = await _reportService.PrepareReport(reportVM);
            if (reportVm.StartDate.HasValue)
                reportVm.StartDate = reportVm.StartDate.Value.Date;

            if (reportVm.EndDate.HasValue)
                reportVm.EndDate = reportVm.EndDate.Value.Date;
            
            return View(reportVm);
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogError(ex,"Report Controller: User not found");
            return RedirectToAction("Index", "Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Report Controller: Error");
            return RedirectToAction("Index", "Error");
        }
    }
}