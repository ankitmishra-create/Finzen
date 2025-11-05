using FinanceManagement.Application.DTO;

namespace FinanceManagement.Application.ViewModels
{
    public class DashboardVM
    {
        public DashboardDto DashboardDto { get; set; }
        public YearlyTransactionSummary YearlySummary { get; set; }
    }
}
