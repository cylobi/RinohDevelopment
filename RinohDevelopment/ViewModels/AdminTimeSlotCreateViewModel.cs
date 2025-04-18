using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.ViewModels;

public class AdminTimeSlotCreateViewModel
{
    [Required(ErrorMessage = "وارد کردن تاریخ الزامی است")]
    [Display(Name = "تاریخ")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }
    
    [Required(ErrorMessage = "وارد کردن زمان شروع الزامی است")]
    [Display(Name = "زمان شروع")]
    public TimeSpan StartTime { get; set; }
    
    [Required(ErrorMessage = "وارد کردن زمان پایان الزامی است")]
    [Display(Name = "زمان پایان")]
    public TimeSpan EndTime { get; set; }
    
    [Required(ErrorMessage = "وارد کردن ظرفیت الزامی است")]
    [Display(Name = "ظرفیت")]
    [Range(1, 100, ErrorMessage = "ظرفیت باید بین 1 تا 100 باشد")]
    public int Capacity { get; set; }
}