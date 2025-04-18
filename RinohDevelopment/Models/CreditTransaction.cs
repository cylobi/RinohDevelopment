using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.Models;

public class CreditTransaction
{
    public int Id { get; set; }
        
    [Required]
    [Display(Name = "تاریخ تراکنش")]
    public DateTime TransactionDate { get; set; }
        
    [Required]
    [Display(Name = "مقدار")]
    public decimal Amount { get; set; }
        
    [Required]
    [Display(Name = "نوع تراکنش")]
    public TransactionType Type { get; set; }
        
    [Display(Name = "توضیحات")]
    public string Description { get; set; }
        
    // Foreign key relationship
    public int CreditId { get; set; }
    public virtual Credit Credit { get; set; }
}

public enum TransactionType
{
    [Display(Name = "افزایش اعتبار")]
    Credit,
        
    [Display(Name = "کاهش اعتبار")]
    Debit
}