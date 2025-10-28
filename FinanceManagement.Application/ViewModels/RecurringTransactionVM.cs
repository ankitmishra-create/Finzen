using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;

namespace FinanceManagement.Application.ViewModels
{
    public class RecurringTransactionVM
    {
        public Guid RecurringTransactionId { get; set; }
        public RecurrenceFrequency RecurrenceFrequency { get; set; }
        public string Description { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public string OriginalCurrency { get; set; }
        public string TransactionCurrency { get; set; }
        public TransactionTerrority TransactionTerrority { get; set; }

    }
}
