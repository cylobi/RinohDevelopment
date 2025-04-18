using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.Models;

public class CleaningService
{
    public int Id { get; set; }
        
    [Required]
    [Display(Name = "نام سرویس")]
    public string Name { get; set; } = string.Empty;
        
    [Required]
    [Display(Name = "توضیحات")]
    public string Description { get; set; } = string.Empty;
        
    [Required]
    [Display(Name = "قیمت (میلی گرم طلا)")]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}