using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "وارد کردن شماره تلفن الزامی است")]
    [Display(Name = "شماره تلفن")]
    [Phone(ErrorMessage = "لطفا یک شماره تلفن معتبر وارد کنید")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "وارد کردن رمز عبور الزامی است")]
    [Display(Name = "رمز عبور")]
    [MinLength(6, ErrorMessage = "رمز عبور باید حداقل 6 کاراکتر باشد")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "وارد کردن تکرار رمز عبور الزامی است")]
    [Display(Name = "تکرار رمز عبور")]
    [Compare("Password", ErrorMessage = "رمز عبور و تکرار آن باید یکسان باشند")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "نام")]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "آدرس")]
    public string Address { get; set; } = string.Empty;
}