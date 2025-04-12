using System.ComponentModel.DataAnnotations;

namespace RinohDevelopment.ViewModels;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "وارد کردن رمز عبور فعلی الزامی است")]
    [Display(Name = "رمز عبور فعلی")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "وارد کردن رمز عبور جدید الزامی است")]
    [Display(Name = "رمز عبور جدید")]
    [MinLength(6, ErrorMessage = "رمز عبور باید حداقل 6 کاراکتر باشد")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "وارد کردن تکرار رمز عبور جدید الزامی است")]
    [Display(Name = "تکرار رمز عبور جدید")]
    [Compare("NewPassword", ErrorMessage = "رمز عبور جدید و تکرار آن باید یکسان باشند")]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}