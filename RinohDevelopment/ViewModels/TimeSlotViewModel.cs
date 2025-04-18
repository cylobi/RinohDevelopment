using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.ViewModels;

public class TimeSlotViewModel
{
    public int Id { get; set; }
        
    [Display(Name = "تاریخ")]
    public DateTime Date { get; set; }
        
    [Display(Name = "زمان شروع")]
    public TimeSpan StartTime { get; set; }
        
    [Display(Name = "زمان پایان")]
    public TimeSpan EndTime { get; set; }
        
    [Display(Name = "ظرفیت باقی مانده")]
    public int RemainingCapacity { get; set; }
        
    public string DisplayText => $"{Date.ToString("yyyy/MM/dd")} - {StartTime.ToString(@"hh\:mm")} تا {EndTime.ToString(@"hh\:mm")} - ظرفیت: {RemainingCapacity}";
}