using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.ViewModels;

public class RecyclableRequestViewModel
{
    public int Id { get; set; }
        
    [Required(ErrorMessage = "انتخاب زمان الزامی است")]
    [Display(Name = "زمان جمع آوری")]
    public int TimeSlotId { get; set; }
        
    [Display(Name = "توضیحات")]
    public string? Notes { get; set; } = string.Empty;

    public List<TimeSlotViewModel> AvailableTimeSlots { get; set; } = new List<TimeSlotViewModel>();
}