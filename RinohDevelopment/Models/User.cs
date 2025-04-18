using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "شماره تلفن")]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
        
    [Required]
    [Display(Name = "رمز عبور")]
    public string PasswordHash { get; set; } = string.Empty;
        
    [Display(Name = "نام")]
    public string FirstName { get; set; } = string.Empty;
        
    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; } = string.Empty;
        
    [Display(Name = "آدرس")]
    public string Address { get; set; }  = string.Empty;
    
    [Display(Name = "کد پستی")]
    public string PostalCode { get; set; }  = string.Empty;
        
    [Display(Name = "مدیر")]
    public bool IsAdmin { get; set; }
        
    // Navigation properties
    public virtual Credit Credit { get; set; }
    public virtual ICollection<RecyclableRequest> RecyclableRequests { get; set; }
}