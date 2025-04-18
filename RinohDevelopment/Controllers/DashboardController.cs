using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RinohDevelopment.Context;
using RinohDevelopment.Filters;
using RinohDevelopment.Models;
using RinohDevelopment.Services;
using RinohDevelopment.ViewModels;

namespace RinohDevelopment.Controllers;

[AuthFilter]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;

    public DashboardController(ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _authService.GetCurrentUserAsync(HttpContext);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get user credit
        var credit = await _context.Credits
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        // Get user's recent recyclable requests
        var requests = await _context.RecyclableRequests
            .Include(r => r.TimeSlot)
            .Where(r => r.UserId == user.Id)
            .OrderByDescending(r => r.RequestDate)
            .Take(5)
            .ToListAsync();

        var viewModel = new DashboardViewModel
        {
            UserName = $"{user.FirstName} {user.LastName}",
            Credit = credit?.Amount ?? 0,
            RecentRequests = requests.Select(r => new RecyclableRequestListItemViewModel
            {
                Id = r.Id,
                RequestDate = r.RequestDate,
                PickupDate = r.TimeSlot.Date,
                PickupTimeStart = r.TimeSlot.StartTime,
                PickupTimeEnd = r.TimeSlot.EndTime,
                Status = r.Status
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> RequestRecyclablePickup()
    {
        var user = await _authService.GetCurrentUserAsync(HttpContext);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get available time slots that are at least 72 hours in the future
        var cutoffDate = DateTime.Now.AddHours(72);
        var availableTimeSlots = await _context.TimeSlots
            .Where(ts => ts.Date > cutoffDate && ts.RemainingCapacity > 0)
            .OrderBy(ts => ts.Date)
            .ThenBy(ts => ts.StartTime)
            .ToListAsync();

        var viewModel = new RecyclableRequestViewModel
        {
            AvailableTimeSlots = availableTimeSlots.Select(ts => new TimeSlotViewModel
            {
                Id = ts.Id,
                Date = ts.Date,
                StartTime = ts.StartTime,
                EndTime = ts.EndTime,
                RemainingCapacity = ts.RemainingCapacity
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestRecyclablePickup(RecyclableRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload available time slots
            var cutoffDate = DateTime.Now.AddHours(72);
            var availableTimeSlots = await _context.TimeSlots
                .Where(ts => ts.Date > cutoffDate && ts.RemainingCapacity > 0)
                .OrderBy(ts => ts.Date)
                .ThenBy(ts => ts.StartTime)
                .ToListAsync();

            model.AvailableTimeSlots = availableTimeSlots.Select(ts => new TimeSlotViewModel
            {
                Id = ts.Id,
                Date = ts.Date,
                StartTime = ts.StartTime,
                EndTime = ts.EndTime,
                RemainingCapacity = ts.RemainingCapacity
            }).ToList();

            return View(model);
        }

        var user = await _authService.GetCurrentUserAsync(HttpContext);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Check if the time slot exists and has capacity
        var timeSlot = await _context.TimeSlots
            .FirstOrDefaultAsync(ts => ts.Id == model.TimeSlotId && ts.RemainingCapacity > 0);

        if (timeSlot == null)
        {
            ModelState.AddModelError("TimeSlotId", "این زمان انتخاب شده در دسترس نیست یا ظرفیت آن تکمیل شده است.");
            
            // Reload available time slots
            var cutoffDate = DateTime.Now.AddHours(72);
            var availableTimeSlots = await _context.TimeSlots
                .Where(ts => ts.Date > cutoffDate && ts.RemainingCapacity > 0)
                .OrderBy(ts => ts.Date)
                .ThenBy(ts => ts.StartTime)
                .ToListAsync();

            model.AvailableTimeSlots = availableTimeSlots.Select(ts => new TimeSlotViewModel
            {
                Id = ts.Id,
                Date = ts.Date,
                StartTime = ts.StartTime,
                EndTime = ts.EndTime,
                RemainingCapacity = ts.RemainingCapacity
            }).ToList();

            return View(model);
        }

        // Create a new recyclable request
        var request = new RecyclableRequest
        {
            UserId = user.Id,
            TimeSlotId = model.TimeSlotId,
            RequestDate = DateTime.Now,
            Notes = model.Notes,
            Status = RequestStatus.Pending
        };

        _context.RecyclableRequests.Add(request);

        // Update the time slot's remaining capacity
        timeSlot.RemainingCapacity--;
        _context.TimeSlots.Update(timeSlot);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "درخواست جمع آوری با موفقیت ثبت شد.";
        return RedirectToAction("MyRequests");
    }

    public async Task<IActionResult> MyRequests()
    {
        var user = await _authService.GetCurrentUserAsync(HttpContext);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var requests = await _context.RecyclableRequests
            .Include(r => r.TimeSlot)
            .Where(r => r.UserId == user.Id)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();

        var viewModel = requests.Select(r => new RecyclableRequestListItemViewModel
        {
            Id = r.Id,
            RequestDate = r.RequestDate,
            PickupDate = r.TimeSlot.Date,
            PickupTimeStart = r.TimeSlot.StartTime,
            PickupTimeEnd = r.TimeSlot.EndTime,
            Status = r.Status,
            Notes = r.Notes
        }).ToList();

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelRequest(int id)
    {
        var user = await _authService.GetCurrentUserAsync(HttpContext);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var request = await _context.RecyclableRequests
            .Include(r => r.TimeSlot)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

        if (request == null)
        {
            TempData["ErrorMessage"] = "درخواست مورد نظر یافت نشد.";
            return RedirectToAction("MyRequests");
        }

        if (request.Status == RequestStatus.Completed)
        {
            TempData["ErrorMessage"] = "امکان لغو درخواست انجام شده وجود ندارد.";
            return RedirectToAction("MyRequests");
        }

        // Cancel the request
        request.Status = RequestStatus.Cancelled;
        _context.RecyclableRequests.Update(request);

        // Increase the time slot's remaining capacity
        request.TimeSlot.RemainingCapacity++;
        _context.TimeSlots.Update(request.TimeSlot);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "درخواست با موفقیت لغو شد.";
        return RedirectToAction("MyRequests");
    }

    public async Task<IActionResult> CreditHistory()
    {
        var user = await _authService.GetCurrentUserAsync(HttpContext);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var credit = await _context.Credits
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (credit == null)
        {
            return View(new CreditHistoryViewModel
            {
                CurrentCredit = 0,
                Transactions = new List<CreditTransactionViewModel>()
            });
        }

        var transactions = credit.Transactions.OrderByDescending(t => t.TransactionDate)
            .Select(t => new CreditTransactionViewModel
            {
                Date = t.TransactionDate,
                Amount = t.Amount,
                Type = t.Type,
                Description = t.Description
            }).ToList();

        var viewModel = new CreditHistoryViewModel
        {
            CurrentCredit = credit.Amount,
            Transactions = transactions
        };

        return View(viewModel);
    }
}

public class DashboardViewModel
{
    public string UserName { get; set; }
    public decimal Credit { get; set; }
    public List<RecyclableRequestListItemViewModel> RecentRequests { get; set; }
}

public class RecyclableRequestListItemViewModel
{
    public int Id { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime PickupDate { get; set; }
    public TimeSpan PickupTimeStart { get; set; }
    public TimeSpan PickupTimeEnd { get; set; }
    public RequestStatus Status { get; set; }
    public string Notes { get; set; }
    
    public string StatusDisplay => GetStatusDisplay(Status);
    public string TimeDisplay => $"{PickupDate.ToString("yyyy/MM/dd")} - {PickupTimeStart.ToString(@"hh\:mm")} تا {PickupTimeEnd.ToString(@"hh\:mm")}";
    
    private string GetStatusDisplay(RequestStatus status)
    {
        switch (status)
        {
            case RequestStatus.Pending:
                return "در انتظار تایید";
            case RequestStatus.Confirmed:
                return "تایید شده";
            case RequestStatus.Completed:
                return "انجام شده";
            case RequestStatus.Cancelled:
                return "لغو شده";
            default:
                return "";
        }
    }
}

public class CreditTransactionViewModel
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string Description { get; set; }
    
    public string TypeDisplay => Type == TransactionType.Credit ? "افزایش اعتبار" : "کاهش اعتبار";
}

public class CreditHistoryViewModel
{
    public decimal CurrentCredit { get; set; }
    public List<CreditTransactionViewModel> Transactions { get; set; }
}