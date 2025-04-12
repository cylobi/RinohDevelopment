using RinohDevelopment.Models;
using Microsoft.EntityFrameworkCore;
namespace RinohDevelopment.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Credit> Credits { get; set; }
    public DbSet<TimeSlot> TimeSlots { get; set; }
    public DbSet<RecyclableRequest> RecyclableRequests { get; set; }
    public DbSet<RecyclableCollection> RecyclableCollections { get; set; }
    public DbSet<CreditTransaction> CreditTransactions { get; set; }
    public DbSet<CleaningService> CleaningServices { get; set; }
    public DbSet<CleaningOrder> CleaningOrders { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships and constraints
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();
            
        modelBuilder.Entity<User>()
            .HasOne(u => u.Credit)
            .WithOne(c => c.User)
            .HasForeignKey<Credit>(c => c.UserId);
            
        modelBuilder.Entity<TimeSlot>()
            .HasMany(ts => ts.RecyclableRequests)
            .WithOne(rr => rr.TimeSlot)
            .HasForeignKey(rr => rr.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<RecyclableRequest>()
            .HasOne(rr => rr.Collection)
            .WithOne(rc => rc.Request)
            .HasForeignKey<RecyclableCollection>(rc => rc.RequestId);
            
        // Seed initial admin user
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                PhoneNumber = "09123456789",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FirstName = "مدیر",
                LastName = "سیستم",
                IsAdmin = true
            }
        );
        
        modelBuilder.Entity<Credit>().HasData(
            new Credit
            {
                Id = 1,
                UserId = 1,
                Amount = 0
            }
        );
        
        base.OnModelCreating(modelBuilder);
    }
}