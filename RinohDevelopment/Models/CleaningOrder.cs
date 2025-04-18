using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.Models;

public class CleaningOrder
{
    public int Id { get; set; }
        
    [Required]
    [Display(Name = "تاریخ سفارش")]
    public DateTime OrderDate { get; set; }
        
    [Display(Name = "وضعیت")]
    public OrderStatus Status { get; set; }
        
    [Display(Name = "آدرس سرویس")]
    public string ServiceAddress { get; set; }
        
    [Display(Name = "تاریخ سرویس")]
    public DateTime ServiceDate { get; set; }
        
    // Foreign key relationships
    public int UserId { get; set; }
    public virtual User User { get; set; }
        
    public int ServiceId { get; set; }
    public virtual CleaningService Service { get; set; }
}

public enum OrderStatus
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