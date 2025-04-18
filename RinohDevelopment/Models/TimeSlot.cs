using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.Models;

public class TimeSlot
{
    public int Id { get; set; }
        
    [Required]
    [Display(Name = "تاریخ")]
    public DateTime Date { get; set; }
        
    [Required]
    [Display(Name = "زمان شروع")]
    public TimeSpan StartTime { get; set; }
        
    [Required]
    [Display(Name = "زمان پایان")]
    public TimeSpan EndTime { get; set; }
        
    [Required]
    [Display(Name = "ظرفیت")]
    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }
        
    [Display(Name = "ظرفیت باقی مانده")]
    public int RemainingCapacity { get; set; }
        
    // Navigation properties
    public virtual ICollection<RecyclableRequest> RecyclableRequests { get; set; }
}