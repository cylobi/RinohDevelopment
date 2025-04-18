using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.ViewModels;

public class OrderCleaningServiceViewModel
{
    public int ServiceId { get; set; }
    
    public string ServiceName { get; set; }
    
    public decimal ServicePrice { get; set; }
    
    [Required(ErrorMessage = "وارد کردن تاریخ سرویس الزامی است")]
    [Display(Name = "تاریخ سرویس")]
    [DataType(DataType.Date)]
    public DateTime ServiceDate { get; set; }
    
    [Required(ErrorMessage = "وارد کردن آدرس سرویس الزامی است")]
    [Display(Name = "آدرس سرویس")]
    public string ServiceAddress { get; set; }
}