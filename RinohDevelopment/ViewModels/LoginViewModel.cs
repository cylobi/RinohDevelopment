using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "وارد کردن شماره تلفن الزامی است")]
    [Display(Name = "شماره تلفن")]
    [Phone(ErrorMessage = "لطفا یک شماره تلفن معتبر وارد کنید")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "وارد کردن رمز عبور الزامی است")]
    [Display(Name = "رمز عبور")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}