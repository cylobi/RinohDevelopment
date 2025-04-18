
using System.ComponentModel.DataAnnotations;
using RinohDevelopment.Models;

namespace RinohDevelopment.ViewModels;

public class ManualCreditTransactionViewModel
{
    [Required(ErrorMessage = "انتخاب کاربر الزامی است")]
    [Display(Name = "کاربر")]
    public int UserId { get; set; }
    
    [Required(ErrorMessage = "وارد کردن مقدار اعتبار الزامی است")]
    [Display(Name = "مقدار (میلی گرم طلا)")]
    [Range(0.1, 10000, ErrorMessage = "مقدار باید بین 0.1 تا 10000 میلی گرم طلا باشد")]
    public decimal Amount { get; set; }
    
    [Required(ErrorMessage = "انتخاب نوع تراکنش الزامی است")]
    [Display(Name = "نوع تراکنش")]
    public TransactionType Type { get; set; }
    
    [Required(ErrorMessage = "وارد کردن توضیحات الزامی است")]
    [Display(Name = "توضیحات")]
    public string Description { get; set; }
    
    public List<UserListItemViewModel> AvailableUsers { get; set; } = new();
}