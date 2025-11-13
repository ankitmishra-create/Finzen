using Azure.Identity;
using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork  _unitOfWork;
    private readonly ILoggedInUser _loggedInUser;
    private readonly ILogger<ReportService> _logger;   
    
    public ReportService(IUnitOfWork unitOfWork,ILoggedInUser loggedInUser,ILogger<ReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _loggedInUser = loggedInUser;
        _logger=logger;
    }
    
    public async Task<ReportVM> PrepareView()
    {
        var userId = _loggedInUser.CurrentLoggedInUser();
        if (userId == null)
        {
            _logger.LogError($"PrepareView: User not logged in");
            throw new UserNotFoundException($"Requested user not found in db userid: {userId}");
        }
        var userCategories = await _unitOfWork.Category.GetAllAsync(c => c.UserId == userId);
        ReportVM reportVm = new ReportVM();
        reportVm.Categories = userCategories;
        return reportVm;
    }

    public async Task<ReportVM> PrepareReport(ReportVM reportVm)
    {
        var userId = _loggedInUser.CurrentLoggedInUser();
        if (userId == null)
        {
            _logger.LogError($"PrepareReport: User not logged in");
            throw new UserNotFoundException($"Requested user not found in db userid: {userId}");
        }
    
        var categories = await _unitOfWork.Category.GetAllAsync(c => c.UserId == userId);
    
        var transactions = await _unitOfWork.Transaction.GetAllPopulatedAsync(t => t.UserId == userId 
                && t.TransactionDate >= reportVm.StartDate && t.TransactionDate <= reportVm.EndDate 
            ,include: q=>q.Include(c => c.Category));
        
        if (reportVm.CategoryType.HasValue)
        {
            transactions = transactions.Where(t => t.Category != null && 
                                                   t.Category.CategoryType == reportVm.CategoryType.Value);
        }
        if (reportVm.SubCategory.HasValue)
        {
            transactions = transactions.Where(t => t.Category != null && t.Category.CategoryId == reportVm.SubCategory.Value);
        }
        reportVm.Transactions = transactions.OrderBy(t => t.TransactionDate).ToList();
        reportVm.Categories = categories.ToList();
       
        return reportVm;
    }
}