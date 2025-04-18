using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.ViewModels;

public class ProfileViewModel
{
    public int Id { get; set; }

    [Display(Name = "شماره تلفن")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "نام")]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "آدرس")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "اعتبار (میلی گرم طلا)")]
    public decimal Credit { get; set; }
}