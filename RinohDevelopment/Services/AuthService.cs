using Microsoft.EntityFrameworkCore;
using RinohDevelopment.Context;
using RinohDevelopment.Models;

namespace RinohDevelopment.Services;

public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string phoneNumber, string password);
        Task<User> RegisterUserAsync(User user, string password);
        Task<bool> IsPhoneNumberUniqueAsync(string phoneNumber);
        bool VerifyPassword(string password, string passwordHash);
        string HashPassword(string password);
        Task SignInAsync(HttpContext httpContext, User user);
        Task SignOutAsync(HttpContext httpContext);
        Task<User?> GetCurrentUserAsync(HttpContext httpContext);
        bool IsAdmin(User user);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string UserSessionKey = "UserId";

        public AuthService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User?> AuthenticateAsync(string phoneNumber, string password)
        {
            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(password))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (user == null)
                return null;

            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<User> RegisterUserAsync(User user, string password)
        {
            if (user == null || string.IsNullOrEmpty(password))
                throw new ArgumentException("User and password are required");

            if (await _context.Users.AnyAsync(u => u.PhoneNumber == user.PhoneNumber))
                throw new InvalidOperationException("Phone number already exists");

            user.PasswordHash = HashPassword(password);
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            var credit = new Credit
            {
                UserId = user.Id,
                Amount = 0
            };
            
            _context.Credits.Add(credit);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> IsPhoneNumberUniqueAsync(string phoneNumber)
        {
            return !await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public Task SignInAsync(HttpContext httpContext, User user)
        {
            httpContext.Session.SetInt32(UserSessionKey, user.Id);
            return Task.CompletedTask;
        }

        public Task SignOutAsync(HttpContext httpContext)
        {
            httpContext.Session.Remove(UserSessionKey);
            return Task.CompletedTask;
        }

        public async Task<User?> GetCurrentUserAsync(HttpContext httpContext)
        {
            var userId = httpContext.Session.GetInt32(UserSessionKey);
            if (!userId.HasValue)
                return null;

            return (await _context.Users
                .Include(u => u.Credit)
                .FirstOrDefaultAsync(u => u.Id == userId.Value))!;
        }

        public bool IsAdmin(User user)
        {
            return user is { IsAdmin: true };
        }
    }