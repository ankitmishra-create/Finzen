using FinanceManagement.Core.Enums;

namespace FinanceManagement.Application.DTO
{
    public class SavingDto
    {
        public Guid SavingId { get; set; }
        public CategoryType? CategoryType { get; set; }

        public string SavingName { get; set; }

        public string? CategoryName { get; set; }
        public decimal SavingAmount { get; set; }
        public decimal AlreadySavedAmount { get; set; }
        public DateTime SavingStartDate { get; set; }
        public DateTime SavingEndDate { get; set; }
        public string Description { get; set; }
    }
}
