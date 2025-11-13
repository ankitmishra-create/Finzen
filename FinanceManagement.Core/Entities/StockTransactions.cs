using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinanceManagement.Core.Enums;

namespace FinanceManagement.Core.Entities;

public class StockTransactions
{
    [Key]
    public Guid StockTransactionId { get; set; } = Guid.NewGuid();
    
    public string StockSymbol { get; set; } 
    
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public int Quantity { get; set; }
    public decimal AmountPerUnit { get; set; }  
}