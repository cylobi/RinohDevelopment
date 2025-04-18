using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.Models;

public class RecyclableRequest
{
    public int Id { get; set; }
        
    [Required]
    [Display(Name = "تاریخ درخواست")]
    public DateTime RequestDate { get; set; }
        
    [Display(Name = "وضعیت")]
    public RequestStatus Status { get; set; }
        
    [Display(Name = "توضیحات")]
    public string Notes { get; set; } = string.Empty;
        
    // Foreign key relationships
    public int UserId { get; set; }
    public virtual User User { get; set; }
        
    public int TimeSlotId { get; set; }
    public virtual TimeSlot TimeSlot { get; set; }
        
    // Navigation properties
    public virtual RecyclableCollection Collection { get; set; }
}

public enum RequestStatus
{
    [Display(Name = "در انتظار تایید")]
    Pending,
        
    [Display(Name = "تایید شده")]
    Confirmed,
        
    [Display(Name = "انجام شده")]
    Completed,
        
    [Display(Name = "لغو شده")]
    Cancelled
}