using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.Models;

public class RecyclableCollection
{
    public int Id { get; set; }
        
    [Display(Name = "تاریخ جمع آوری")]
    public DateTime CollectionDate { get; set; }
        
    [Display(Name = "وزن (کیلوگرم)")]
    [Range(0, double.MaxValue)]
    public decimal Weight { get; set; }
        
    [Display(Name = "اعتبار اختصاص داده شده (میلی گرم طلا)")]
    [Range(0, double.MaxValue)]
    public decimal CreditAssigned { get; set; }
        
    // Foreign key relationship
    public int RequestId { get; set; }
    public virtual RecyclableRequest Request { get; set; }
}