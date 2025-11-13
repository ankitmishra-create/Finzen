using FinanceManagement.Application.ViewModels;

namespace FinanceManagement.Application.Interfaces;

public interface IReportService
{
    Task<ReportVM> PrepareView();
    Task<ReportVM> PrepareReport(ReportVM  reportVM);    
}