using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.ViewModels;

public class RecyclableCollectionViewModel
{
    public int RequestId { get; set; }
    
    [Required(ErrorMessage = "وارد کردن وزن الزامی است")]
    [Display(Name = "وزن (کیلوگرم)")]
    [Range(0.1, 1000, ErrorMessage = "وزن باید بین 0.1 تا 1000 کیلوگرم باشد")]
    public decimal Weight { get; set; }
    
    [Required(ErrorMessage = "وارد کردن مقدار اعتبار الزامی است")]
    [Display(Name = "اعتبار (میلی گرم طلا)")]
    [Range(0.1, 1000, ErrorMessage = "اعتبار باید بین 0.1 تا 1000 میلی گرم طلا باشد")]
    public decimal CreditAssigned { get; set; }
}

