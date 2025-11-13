using System.ComponentModel.DataAnnotations;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;

namespace FinanceManagement.Application.ViewModels;

public class InvestmentTransactionVM
{
    [Required]
    public string StockSymbol { get; set; } = string.Empty;
    [Required]
    public DateTime TransactionDate { get; set; }

    public string AssetType { get; set; } 

    [Required]
    public TransactionType  TransactionType { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public decimal PricePerUnit { get; set; } 
    public List<StockHoldings> StockHoldings { get; set; }
}