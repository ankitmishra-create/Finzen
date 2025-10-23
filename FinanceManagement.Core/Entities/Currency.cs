using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Core.Entities
{
    public class Currency
    {
        [Key]
        public Guid CurrencyId { get; set; } = Guid.NewGuid();
        [Required]
        public string CountryName { get; set; } 
        [Required]
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
    }
}