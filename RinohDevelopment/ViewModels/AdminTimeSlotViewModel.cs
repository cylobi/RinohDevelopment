using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.ViewModels;

public class AdminTimeSlotViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "تاریخ")]
    public DateTime Date { get; set; }
    
    [Display(Name = "زمان شروع")]
    public TimeSpan StartTime { get; set; }
    
    [Display(Name = "زمان پایان")]
    public TimeSpan EndTime { get; set; }
    
    [Display(Name = "ظرفیت کل")]
    public int Capacity { get; set; }
    
    [Display(Name = "ظرفیت باقی مانده")]
    public int RemainingCapacity { get; set; }
    
    public string DateDisplay => Date.ToString("yyyy/MM/dd");
    public string TimeDisplay => $"{StartTime.ToString(@"hh\:mm")} تا {EndTime.ToString(@"hh\:mm")}";
    public bool IsExpired => Date < DateTime.Now;
}