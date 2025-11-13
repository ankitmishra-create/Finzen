using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;

namespace FinanceManagement.Application.ViewModels;

public class ReportVM
{
    [Display(Name = "Start Date")]
    public DateTime? StartDate { get; set; }
    [Display(Name = "End Date")]
    public DateTime? EndDate { get; set; }   
    
    public CategoryType? CategoryType { get; set; }
    public Guid? SubCategory { get; set; }
    public List<Category> Categories { get; set; } = new List<Category>();
    public List<Transaction> Transactions { get; set; } = new List<Transaction>();
}