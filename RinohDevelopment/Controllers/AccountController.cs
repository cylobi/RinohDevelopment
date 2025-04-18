using Microsoft.AspNetCore.Mvc;
using RinohDevelopment.Context;
using RinohDevelopment.Models;
using RinohDevelopment.Services;
using RinohDevelopment.ViewModels;

namespace RinohDevelopment.Controllers;

public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        public AccountController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _authService.AuthenticateAsync(model.PhoneNumber, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "شماره تلفن یا رمز عبور اشتباه است.");
                return View(model);
            }

            await _authService.SignInAsync(HttpContext, user);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.IsAdmin)
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!await _authService.IsPhoneNumberUniqueAsync(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "این شماره تلفن قبلا ثبت شده است.");
                return View(model);
            }

            try
            {
                var user = new User
                {
                    PhoneNumber = model.PhoneNumber,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    IsAdmin = false
                };

                await _authService.RegisterUserAsync(user, model.Password);
                await _authService.SignInAsync(HttpContext, user);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "خطا در ثبت نام: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutAsync(HttpContext);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _authService.GetCurrentUserAsync(HttpContext);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                Id = user.Id,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                Credit = user.Credit?.Amount ?? 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var user = await _authService.GetCurrentUserAsync(HttpContext);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Address = model.Address;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "اطلاعات پروفایل با موفقیت به روز شد.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _authService.GetCurrentUserAsync(HttpContext);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                var profileModel = new ProfileViewModel
                {
                    Id = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = user.Address,
                    Credit = user.Credit?.Amount ?? 0
                };
                
                return View("Profile", profileModel);
            }

            if (!_authService.VerifyPassword(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError("CurrentPassword", "رمز عبور فعلی اشتباه است.");
                
                var profileModel = new ProfileViewModel
                {
                    Id = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = user.Address,
                    Credit = user.Credit?.Amount ?? 0
                };
                
                return View("Profile", profileModel);
            }

            user.PasswordHash = _authService.HashPassword(model.NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "رمز عبور با موفقیت تغییر کرد.";
            return RedirectToAction("Profile");
        }
    }