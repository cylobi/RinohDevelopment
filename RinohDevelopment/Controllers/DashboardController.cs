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
        // First, reload the available time slots to ensure they're always available in the view
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
        
        var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { x.Key, x.Value.Errors })
            .ToList();

        Console.WriteLine(errors);

        if (!ModelState.IsValid)
        {
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
            return View(model);
        }

        // Create a new recyclable request
        var request = new RecyclableRequest
        {
            UserId = user.Id,
            TimeSlotId = model.TimeSlotId,
            RequestDate = DateTime.Now,
            Notes = model.Notes ?? string.Empty, // Handle null Notes
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
    
    [HttpGet]
public async Task<IActionResult> CleaningServices()
{
    var services = await _context.CleaningServices
        .OrderBy(s => s.Name)
        .ToListAsync();
        
    var viewModel = services.Select(s => new CleaningServiceViewModel
    {
        Id = s.Id,
        Name = s.Name,
        Description = s.Description,
        Price = s.Price
    }).ToList();
    
    return View(viewModel);
}

[HttpGet]
public async Task<IActionResult> OrderCleaningService(int id)
{
    var service = await _context.CleaningServices.FindAsync(id);
    if (service == null)
    {
        return NotFound();
    }
    
    var user = await _authService.GetCurrentUserAsync(HttpContext);
    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }
    
    var viewModel = new OrderCleaningServiceViewModel
    {
        ServiceId = service.Id,
        ServiceName = service.Name,
        ServicePrice = service.Price,
        ServiceDate = DateTime.Now.AddDays(1),
        ServiceAddress = user.Address
    };
    
    return View(viewModel);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> OrderCleaningService(OrderCleaningServiceViewModel model)
{
    if (!ModelState.IsValid)
    {
        var service = await _context.CleaningServices.FindAsync(model.ServiceId);
        if (service != null)
        {
            model.ServiceName = service.Name;
            model.ServicePrice = service.Price;
        }
        
        return View(model);
    }
    
    var user = await _authService.GetCurrentUserAsync(HttpContext);
    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }
    
    // Check if user has enough credit
    var credit = await _context.Credits.FirstOrDefaultAsync(c => c.UserId == user.Id);
    if (credit == null || credit.Amount < model.ServicePrice)
    {
        ModelState.AddModelError(string.Empty, "اعتبار شما برای سفارش این سرویس کافی نیست.");
        
        var service = await _context.CleaningServices.FindAsync(model.ServiceId);
        if (service != null)
        {
            model.ServiceName = service.Name;
            model.ServicePrice = service.Price;
        }
        
        return View(model);
    }
    
    using var tsx = await _context.Database.BeginTransactionAsync();
    
    try
    {
        // Create new cleaning order
        var order = new CleaningOrder
        {
            UserId = user.Id,
            ServiceId = model.ServiceId,
            OrderDate = DateTime.Now,
            ServiceDate = model.ServiceDate,
            ServiceAddress = model.ServiceAddress,
            Status = OrderStatus.Pending
        };
        
        _context.CleaningOrders.Add(order);
        
        // Deduct credit from user
        credit.Amount -= model.ServicePrice;
        _context.Credits.Update(credit);
        
        // Create credit transaction
        var transaction = new CreditTransaction
        {
            CreditId = credit.Id,
            TransactionDate = DateTime.Now,
            Amount = model.ServicePrice,
            Type = TransactionType.Debit,
            Description = $"سفارش سرویس نظافت '{model.ServiceName}'"
        };
        
        _context.CreditTransactions.Add(transaction);
        
        await _context.SaveChangesAsync();
        await tsx.CommitAsync();
        
        TempData["SuccessMessage"] = "سفارش سرویس نظافت با موفقیت ثبت شد.";
        return RedirectToAction("MyCleaningOrders");
    }
    catch (Exception ex)
    {
        await tsx.RollbackAsync();
        ModelState.AddModelError(string.Empty, "خطا در ثبت سفارش: " + ex.Message);
        
        var service = await _context.CleaningServices.FindAsync(model.ServiceId);
        if (service != null)
        {
            model.ServiceName = service.Name;
            model.ServicePrice = service.Price;
        }
        
        return View(model);
    }
}

public async Task<IActionResult> MyCleaningOrders()
{
    var user = await _authService.GetCurrentUserAsync(HttpContext);
    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }
    
    var orders = await _context.CleaningOrders
        .Include(o => o.Service)
        .Where(o => o.UserId == user.Id)
        .OrderByDescending(o => o.OrderDate)
        .ToListAsync();
        
    var viewModel = orders.Select(o => new CleaningOrderViewModel
    {
        Id = o.Id,
        ServiceId = o.ServiceId,
        ServiceName = o.Service.Name,
        ServicePrice = o.Service.Price,
        ServiceDate = o.ServiceDate,
        ServiceAddress = o.ServiceAddress,
        Status = o.Status
    }).ToList();
    
    return View(viewModel);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CancelCleaningOrder(int id)
{
    var user = await _authService.GetCurrentUserAsync(HttpContext);
    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }
    
    var order = await _context.CleaningOrders
        .Include(o => o.Service)
        .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);
        
    if (order == null)
    {
        TempData["ErrorMessage"] = "سفارش مورد نظر یافت نشد.";
        return RedirectToAction("MyCleaningOrders");
    }
    
    if (order.Status != OrderStatus.Pending)
    {
        TempData["ErrorMessage"] = "فقط سفارش های در انتظار تایید قابل لغو هستند.";
        return RedirectToAction("MyCleaningOrders");
    }
    
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        // Cancel the order
        order.Status = OrderStatus.Cancelled;
        _context.CleaningOrders.Update(order);
        
        // Refund credit to user
        var credit = await _context.Credits.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (credit != null)
        {
            credit.Amount += order.Service.Price;
            _context.Credits.Update(credit);
            
            // Create credit transaction
            var creditTransaction = new CreditTransaction
            {
                CreditId = credit.Id,
                TransactionDate = DateTime.Now,
                Amount = order.Service.Price,
                Type = TransactionType.Credit,
                Description = $"بازگشت اعتبار از لغو سفارش سرویس نظافت '{order.Service.Name}'"
            };
            
            _context.CreditTransactions.Add(creditTransaction);
        }
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        TempData["SuccessMessage"] = "سفارش با موفقیت لغو شد و اعتبار به حساب شما بازگشت.";
        return RedirectToAction("MyCleaningOrders");
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        TempData["ErrorMessage"] = "خطا در لغو سفارش: " + ex.Message;
        return RedirectToAction("MyCleaningOrders");
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