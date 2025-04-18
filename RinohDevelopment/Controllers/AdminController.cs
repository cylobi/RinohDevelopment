using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RinohDevelopment.Context;
using RinohDevelopment.Filters;
using RinohDevelopment.Models;
using RinohDevelopment.Services;
using RinohDevelopment.ViewModels;

namespace RinohDevelopment.Controllers;

[AuthFilter(RequireAdmin = true)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;

    public AdminController(ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        // Get count of pending recyclable requests
        var pendingRequestsCount = await _context.RecyclableRequests
            .CountAsync(r => r.Status == RequestStatus.Pending);

        // Get count of upcoming time slots
        var upcomingTimeSlotsCount = await _context.TimeSlots
            .CountAsync(ts => ts.Date > DateTime.Now && ts.RemainingCapacity > 0);

        // Get count of admins
        var adminsCount = await _context.Users
            .CountAsync(u => u.IsAdmin);
        
        // Get count of cleaning services
        var cleaningServicesCount = await _context.CleaningServices
            .CountAsync();
        
        // Get count of pending cleaning orders
        var pendingCleaningOrdersCount = await _context.CleaningOrders
            .CountAsync(o => o.Status == OrderStatus.Pending);

        var viewModel = new AdminDashboardViewModel
        {
            PendingRequestsCount = pendingRequestsCount,
            UpcomingTimeSlotsCount = upcomingTimeSlotsCount,
            AdminsCount = adminsCount,
            CleaningServicesCount = cleaningServicesCount,
            PendingCleaningOrdersCount = pendingCleaningOrdersCount
        };

        return View(viewModel);
    }

    #region Time Slot Management
    
    public async Task<IActionResult> TimeSlots()
    {
        var timeSlots = await _context.TimeSlots
            .OrderByDescending(ts => ts.Date)
            .ThenBy(ts => ts.StartTime)
            .ToListAsync();

        var viewModel = timeSlots.Select(ts => new AdminTimeSlotViewModel
        {
            Id = ts.Id,
            Date = ts.Date,
            StartTime = ts.StartTime,
            EndTime = ts.EndTime,
            Capacity = ts.Capacity,
            RemainingCapacity = ts.RemainingCapacity
        }).ToList();

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult CreateTimeSlot()
    {
        var minDate = DateTime.Now.AddHours(72);
        ViewBag.MinDate = minDate.ToString("yyyy-MM-dd");
        
        return View(new AdminTimeSlotCreateViewModel
        {
            Date = minDate
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTimeSlot(AdminTimeSlotCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var minDate = DateTime.Now.AddHours(72);
            ViewBag.MinDate = minDate.ToString("yyyy-MM-dd");
            return View(model);
        }

        // Validate that end time is after start time
        if (model.EndTime <= model.StartTime)
        {
            ModelState.AddModelError("EndTime", "زمان پایان باید بعد از زمان شروع باشد.");
            var minDate = DateTime.Now.AddHours(72);
            ViewBag.MinDate = minDate.ToString("yyyy-MM-dd");
            return View(model);
        }

        // Create the time slot
        var timeSlot = new TimeSlot
        {
            Date = model.Date,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            Capacity = model.Capacity,
            RemainingCapacity = model.Capacity
        };

        _context.TimeSlots.Add(timeSlot);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "زمان سرویس با موفقیت اضافه شد.";
        return RedirectToAction("TimeSlots");
    }

    [HttpGet]
    public async Task<IActionResult> EditTimeSlot(int id)
    {
        var timeSlot = await _context.TimeSlots.FindAsync(id);
        if (timeSlot == null)
        {
            return NotFound();
        }

        var model = new AdminTimeSlotEditViewModel
        {
            Id = timeSlot.Id,
            Date = timeSlot.Date,
            StartTime = timeSlot.StartTime,
            EndTime = timeSlot.EndTime,
            Capacity = timeSlot.Capacity,
            RemainingCapacity = timeSlot.RemainingCapacity
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTimeSlot(AdminTimeSlotEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var timeSlot = await _context.TimeSlots.FindAsync(model.Id);
        if (timeSlot == null)
        {
            return NotFound();
        }

        // Validate that end time is after start time
        if (model.EndTime <= model.StartTime)
        {
            ModelState.AddModelError("EndTime", "زمان پایان باید بعد از زمان شروع باشد.");
            return View(model);
        }

        // Calculate new available capacity
        var usedCapacity = timeSlot.Capacity - timeSlot.RemainingCapacity;
        var newRemainingCapacity = model.Capacity - usedCapacity;

        if (newRemainingCapacity < 0)
        {
            ModelState.AddModelError("Capacity", "ظرفیت جدید کمتر از تعداد درخواست های ثبت شده است.");
            return View(model);
        }

        // Update the time slot
        timeSlot.Date = model.Date;
        timeSlot.StartTime = model.StartTime;
        timeSlot.EndTime = model.EndTime;
        timeSlot.Capacity = model.Capacity;
        timeSlot.RemainingCapacity = newRemainingCapacity;

        _context.TimeSlots.Update(timeSlot);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "زمان سرویس با موفقیت ویرایش شد.";
        return RedirectToAction("TimeSlots");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTimeSlot(int id)
    {
        var timeSlot = await _context.TimeSlots
            .Include(ts => ts.RecyclableRequests)
            .FirstOrDefaultAsync(ts => ts.Id == id);

        if (timeSlot == null)
        {
            return NotFound();
        }

        // Check if there are any requests for this time slot
        if (timeSlot.RecyclableRequests != null && timeSlot.RecyclableRequests.Count > 0)
        {
            TempData["ErrorMessage"] = "این زمان سرویس دارای درخواست است و نمی توان آن را حذف کرد.";
            return RedirectToAction("TimeSlots");
        }

        _context.TimeSlots.Remove(timeSlot);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "زمان سرویس با موفقیت حذف شد.";
        return RedirectToAction("TimeSlots");
    }
    
    #endregion

    #region Recyclable Requests Management
    
    public async Task<IActionResult> Requests()
    {
        var requests = await _context.RecyclableRequests
            .Include(r => r.User)
            .Include(r => r.TimeSlot)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();

        var viewModel = requests.Select(r => new RecyclableRequestListViewModel
        {
            Id = r.Id,
            UserName = $"{r.User.FirstName} {r.User.LastName}",
            PhoneNumber = r.User.PhoneNumber,
            Address = r.User.Address,
            RequestDate = r.RequestDate,
            PickupDate = r.TimeSlot.Date,
            PickupTimeStart = r.TimeSlot.StartTime,
            PickupTimeEnd = r.TimeSlot.EndTime,
            Notes = r.Notes,
            Status = r.Status
        }).ToList();

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> RequestDetails(int id)
    {
        var request = await _context.RecyclableRequests
            .Include(r => r.User)
            .Include(r => r.TimeSlot)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound();
        }

        var viewModel = new RecyclableRequestDetailViewModel
        {
            Id = request.Id,
            UserName = $"{request.User.FirstName} {request.User.LastName}",
            PhoneNumber = request.User.PhoneNumber,
            Address = request.User.Address,
            RequestDate = request.RequestDate,
            PickupDate = request.TimeSlot.Date,
            PickupTimeStart = request.TimeSlot.StartTime,
            PickupTimeEnd = request.TimeSlot.EndTime,
            Notes = request.Notes,
            Status = request.Status
        };

        // Check if there's an existing collection for this request
        var collection = await _context.RecyclableCollections
            .FirstOrDefaultAsync(c => c.RequestId == id);

        if (collection != null)
        {
            viewModel.HasCollection = true;
            viewModel.CollectionDate = collection.CollectionDate;
            viewModel.Weight = collection.Weight;
            viewModel.CreditAssigned = collection.CreditAssigned;
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmRequest(int id)
    {
        var request = await _context.RecyclableRequests.FindAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        if (request.Status != RequestStatus.Pending)
        {
            TempData["ErrorMessage"] = "فقط درخواست های در انتظار تایید را می توان تایید کرد.";
            return RedirectToAction("RequestDetails", new { id });
        }

        request.Status = RequestStatus.Confirmed;
        _context.RecyclableRequests.Update(request);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "درخواست با موفقیت تایید شد.";
        return RedirectToAction("RequestDetails", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteRequest(RecyclableCollectionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("RequestDetails", new { id = model.RequestId });
        }

        var request = await _context.RecyclableRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == model.RequestId);

        if (request == null)
        {
            return NotFound();
        }

        if (request.Status != RequestStatus.Confirmed)
        {
            TempData["ErrorMessage"] = "فقط درخواست های تایید شده را می توان به عنوان انجام شده ثبت کرد.";
            return RedirectToAction("RequestDetails", new { id = model.RequestId });
        }

        using var tsx = await _context.Database.BeginTransactionAsync();

        try
        {
            // Update request status
            request.Status = RequestStatus.Completed;
            _context.RecyclableRequests.Update(request);

            // Create collection record
            var collection = new RecyclableCollection
            {
                RequestId = model.RequestId,
                CollectionDate = DateTime.Now,
                Weight = model.Weight,
                CreditAssigned = model.CreditAssigned
            };

            _context.RecyclableCollections.Add(collection);

            // Update user's credit
            var userCredit = await _context.Credits.FirstOrDefaultAsync(c => c.UserId == request.UserId);
            if (userCredit != null)
            {
                userCredit.Amount += model.CreditAssigned;
                _context.Credits.Update(userCredit);

                // Record credit transaction
                var transaction = new CreditTransaction
                {
                    CreditId = userCredit.Id,
                    TransactionDate = DateTime.Now,
                    Amount = model.CreditAssigned,
                    Type = TransactionType.Credit,
                    Description = $"افزایش اعتبار بابت جمع آوری بازیافت - {model.Weight} کیلوگرم"
                };

                _context.CreditTransactions.Add(transaction);
            }

            await _context.SaveChangesAsync();
            await tsx.CommitAsync();

            TempData["SuccessMessage"] = "درخواست با موفقیت انجام شد و اعتبار به کاربر افزوده شد.";
        }
        catch (Exception ex)
        {
            await tsx.RollbackAsync();
            TempData["ErrorMessage"] = "خطا در ثبت اطلاعات: " + ex.Message;
        }

        return RedirectToAction("RequestDetails", new { id = model.RequestId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelRequest(int id)
    {
        var request = await _context.RecyclableRequests
            .Include(r => r.TimeSlot)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound();
        }

        if (request.Status == RequestStatus.Completed)
        {
            TempData["ErrorMessage"] = "امکان لغو درخواست انجام شده وجود ندارد.";
            return RedirectToAction("RequestDetails", new { id });
        }

        // Cancel the request
        request.Status = RequestStatus.Cancelled;
        _context.RecyclableRequests.Update(request);

        // Increase the time slot's remaining capacity
        request.TimeSlot.RemainingCapacity++;
        _context.TimeSlots.Update(request.TimeSlot);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "درخواست با موفقیت لغو شد.";
        return RedirectToAction("Requests");
    }
    
    #endregion

    #region Admin Management
    
    public async Task<IActionResult> Administrators()
    {
        var admins = await _context.Users
            .Where(u => u.IsAdmin)
            .OrderBy(u => u.PhoneNumber)
            .ToListAsync();

        var viewModel = admins.Select(a => new AdminListViewModel
        {
            Id = a.Id,
            PhoneNumber = a.PhoneNumber,
            Name = $"{a.FirstName} {a.LastName}"
        }).ToList();

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult CreateAdmin()
    {
        return View(new AdminUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(AdminUserViewModel model)
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
            var admin = new User
            {
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsAdmin = true
            };

            await _authService.RegisterUserAsync(admin, model.Password);

            TempData["SuccessMessage"] = "مدیر جدید با موفقیت اضافه شد.";
            return RedirectToAction("Administrators");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "خطا در ثبت اطلاعات: " + ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAdmin(int id)
    {
        var currentUser = await _authService.GetCurrentUserAsync(HttpContext);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (currentUser.Id == id)
        {
            TempData["ErrorMessage"] = "شما نمی توانید خودتان را از دسترسی مدیریت حذف کنید.";
            return RedirectToAction("Administrators");
        }

        var admin = await _context.Users.FindAsync(id);
        if (admin == null || !admin.IsAdmin)
        {
            return NotFound();
        }

        // Remove admin privileges
        admin.IsAdmin = false;
        _context.Users.Update(admin);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "دسترسی مدیریت با موفقیت حذف شد.";
        return RedirectToAction("Administrators");
    }
    
    #endregion

    #region Cleaning Service

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
    public IActionResult CreateCleaningService()
    {
        return View(new CleaningServiceViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCleaningService(CleaningServiceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var service = new CleaningService
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price
        };
        
        _context.CleaningServices.Add(service);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "سرویس نظافت با موفقیت اضافه شد.";
        return RedirectToAction("CleaningServices");
    }

    [HttpGet]
    public async Task<IActionResult> EditCleaningService(int id)
    {
        var service = await _context.CleaningServices.FindAsync(id);
        if (service == null)
        {
            return NotFound();
        }
        
        var model = new CleaningServiceViewModel
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Price = service.Price
        };
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCleaningService(CleaningServiceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var service = await _context.CleaningServices.FindAsync(model.Id);
        if (service == null)
        {
            return NotFound();
        }
        
        service.Name = model.Name;
        service.Description = model.Description;
        service.Price = model.Price;
        
        _context.CleaningServices.Update(service);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "سرویس نظافت با موفقیت ویرایش شد.";
        return RedirectToAction("CleaningServices");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCleaningService(int id)
    {
        var service = await _context.CleaningServices
            .FirstOrDefaultAsync(s => s.Id == id);

        var orders = await _context.CleaningOrders
            .Where(o => o.ServiceId == service.Id)
            .ToListAsync();
            
        if (service == null)
        {
            return NotFound();
        }
        
        if (orders.Count > 0)
        {
            TempData["ErrorMessage"] = "این سرویس دارای سفارش است و نمی توان آن را حذف کرد.";
            return RedirectToAction("CleaningServices");
        }
        
        _context.CleaningServices.Remove(service);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "سرویس نظافت با موفقیت حذف شد.";
        return RedirectToAction("CleaningServices");
    }

    #endregion
    
    #region Cleaning Orders Management

    public async Task<IActionResult> CleaningOrders()
    {
        var orders = await _context.CleaningOrders
            .Include(o => o.User)
            .Include(o => o.Service)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
            
        var viewModel = orders.Select(o => new AdminCleaningOrderViewModel
        {
            Id = o.Id,
            UserName = $"{o.User.FirstName} {o.User.LastName}",
            PhoneNumber = o.User.PhoneNumber,
            ServiceName = o.Service.Name,
            ServicePrice = o.Service.Price,
            ServiceDate = o.ServiceDate,
            ServiceAddress = o.ServiceAddress,
            OrderDate = o.OrderDate,
            Status = o.Status
        }).ToList();
        
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> CleaningOrderDetails(int id)
    {
        var order = await _context.CleaningOrders
            .Include(o => o.User)
            .Include(o => o.Service)
            .FirstOrDefaultAsync(o => o.Id == id);
            
        if (order == null)
        {
            return NotFound();
        }
        
        var viewModel = new AdminCleaningOrderViewModel
        {
            Id = order.Id,
            UserName = $"{order.User.FirstName} {order.User.LastName}",
            PhoneNumber = order.User.PhoneNumber,
            Address = order.User.Address,
            ServiceName = order.Service.Name,
            ServicePrice = order.Service.Price,
            ServiceDate = order.ServiceDate,
            ServiceAddress = order.ServiceAddress,
            OrderDate = order.OrderDate,
            Status = order.Status
        };
        
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmCleaningOrder(int id)
    {
        var order = await _context.CleaningOrders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }
        
        if (order.Status != OrderStatus.Pending)
        {
            TempData["ErrorMessage"] = "فقط درخواست های در انتظار تایید را می توان تایید کرد.";
            return RedirectToAction("CleaningOrderDetails", new { id });
        }
        
        order.Status = OrderStatus.Confirmed;
        _context.CleaningOrders.Update(order);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "سفارش با موفقیت تایید شد.";
        return RedirectToAction("CleaningOrderDetails", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteCleaningOrder(int id)
    {
        var order = await _context.CleaningOrders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }
        
        if (order.Status != OrderStatus.Confirmed)
        {
            TempData["ErrorMessage"] = "فقط سفارش های تایید شده را می توان انجام شده ثبت کرد.";
            return RedirectToAction("CleaningOrderDetails", new { id });
        }
        
        order.Status = OrderStatus.Completed;
        _context.CleaningOrders.Update(order);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "سفارش با موفقیت انجام شده ثبت شد.";
        return RedirectToAction("CleaningOrderDetails", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelCleaningOrder(int id)
    {
        var order = await _context.CleaningOrders
            .Include(o => o.Service)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);
            
        if (order == null)
        {
            return NotFound();
        }
        
        if (order.Status == OrderStatus.Completed)
        {
            TempData["ErrorMessage"] = "امکان لغو سفارش انجام شده وجود ندارد.";
            return RedirectToAction("CleaningOrderDetails", new { id });
        }
        
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Cancel the order
            order.Status = OrderStatus.Cancelled;
            _context.CleaningOrders.Update(order);
            
            // If the order was pending or confirmed, refund the credit
            if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Confirmed)
            {
                // Find user credit
                var credit = await _context.Credits.FirstOrDefaultAsync(c => c.UserId == order.UserId);
                if (credit != null)
                {
                    // Add refund
                    credit.Amount += order.Service.Price;
                    _context.Credits.Update(credit);
                    
                    // Create credit transaction for refund
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
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            TempData["SuccessMessage"] = "سفارش با موفقیت لغو شد.";
            return RedirectToAction("CleaningOrders");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["ErrorMessage"] = "خطا در لغو سفارش: " + ex.Message;
            return RedirectToAction("CleaningOrderDetails", new { id });
        }
    }

#endregion

    #region ManualCreditTransaction

    [HttpGet]
    public async Task<IActionResult> ManualCreditTransaction()
    {
        var users = await _context.Users
            .Where(u => !u.IsAdmin)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
            
        var viewModel = new ManualCreditTransactionViewModel
        {
            AvailableUsers = users.Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                PhoneNumber = u.PhoneNumber,
                FullName = $"{u.FirstName} {u.LastName}"
            }).ToList()
        };
        
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManualCreditTransaction(ManualCreditTransactionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var users = await _context.Users
                .Where(u => !u.IsAdmin)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            model.AvailableUsers = users.Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                PhoneNumber = u.PhoneNumber,
                FullName = $"{u.FirstName} {u.LastName}"
            }).ToList();

            return View(model);
        }

        var userCredit = await _context.Credits
            .FirstOrDefaultAsync(c => c.UserId == model.UserId);

        if (userCredit == null)
        {
            ModelState.AddModelError("UserId", "کاربر مورد نظر یافت نشد.");

            var users = await _context.Users
                .Where(u => !u.IsAdmin)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            model.AvailableUsers = users.Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                PhoneNumber = u.PhoneNumber,
                FullName = $"{u.FirstName} {u.LastName}"
            }).ToList();

            return View(model);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Update user's credit
            if (model.Type == TransactionType.Credit)
            {
                userCredit.Amount += model.Amount;
            }
            else
            {
                if (userCredit.Amount < model.Amount)
                {
                    ModelState.AddModelError("Amount", "مقدار اعتبار کاربر کافی نیست.");

                    var users = await _context.Users
                        .Where(u => !u.IsAdmin)
                        .OrderBy(u => u.LastName)
                        .ThenBy(u => u.FirstName)
                        .ToListAsync();

                    model.AvailableUsers = users.Select(u => new UserListItemViewModel
                    {
                        Id = u.Id,
                        PhoneNumber = u.PhoneNumber,
                        FullName = $"{u.FirstName} {u.LastName}"
                    }).ToList();

                    return View(model);
                }

                userCredit.Amount -= model.Amount;
            }

            _context.Credits.Update(userCredit);

            // Create credit transaction
            var creditTransaction = new CreditTransaction
            {
                CreditId = userCredit.Id,
                TransactionDate = DateTime.Now,
                Amount = model.Amount,
                Type = model.Type,
                Description = model.Description
            };

            _context.CreditTransactions.Add(creditTransaction);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] = "تراکنش اعتبار با موفقیت ثبت شد.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "خطا در ثبت تراکنش: " + ex.Message);

            var users = await _context.Users
                .Where(u => !u.IsAdmin)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            model.AvailableUsers = users.Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                PhoneNumber = u.PhoneNumber,
                FullName = $"{u.FirstName} {u.LastName}"
            }).ToList();

            return View(model);
        }
    }

    #endregion
}