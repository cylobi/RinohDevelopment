using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.Models;

public class Credit
{
    public int Id { get; set; }
        
    [Required]
    [Display(Name = "مقدار اعتبار (میلی گرم طلا)")]
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
    
    public int UserId { get; set; }
    public virtual User User { get; set; }
        
    // Navigation properties
    public virtual ICollection<CreditTransaction> Transactions { get; set; }
}