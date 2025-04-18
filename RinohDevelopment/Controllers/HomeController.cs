using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RinohDevelopment.Filters;
using RinohDevelopment.Models;
using RinohDevelopment.Services;

namespace RinohDevelopment.Controllers;

public class HomeController(ILogger<HomeController> logger, IAuthService authService) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    public async Task<IActionResult> Index()
    {
        var user = await authService.GetCurrentUserAsync(HttpContext);
        if (user != null)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}