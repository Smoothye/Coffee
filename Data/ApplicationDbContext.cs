using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, int>(options)
{
    public DbSet<UserEvent> UserEvents { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<WeddingTable> WeddingTables { get; set; }
    public DbSet<Guest> Guests { get; set; }
    public DbSet<CheckListTask> CheckListTasks { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<EventSupplier> EventSuppliers { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<ScheduleItem> ScheduleItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Guest>()
            .HasOne(g => g.WeddingTable)
            .WithMany(w => w.Guests)
            .HasForeignKey(g => g.TableId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Supplier)
            .WithMany(s => s.Expenses)
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<EventSupplier>()
            .HasOne(es => es.Supplier)
            .WithMany(s => s.EventSuppliers)
            .HasForeignKey(es => es.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<CheckListTask>()
            .Property(t => t.Status)
            .HasConversion<string>();
        
        modelBuilder.Entity<CheckListTask>()
            .Property(t => t.Priority)
            .HasConversion<string>();
        
        modelBuilder.Entity<CheckListTask>()
            .Property(t => t.Category)
            .HasConversion<string>();
        
        modelBuilder.Entity<Guest>()
            .Property(t => t.RsvpStatus)
            .HasConversion<string>();
        
        modelBuilder.Entity<Guest>()
            .Property(t => t.Group)
            .HasConversion<string>();
        
        modelBuilder.Entity<Guest>()
            .Property(t => t.DietaryRequirements)
            .HasConversion<string>();
        
        modelBuilder.Entity<Expense>()
            .Property(t => t.PaymentStatus)
            .HasConversion<string>();
    }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // set default length of all string fields
        configurationBuilder.Properties<string>()
            .HaveMaxLength(256);

        // set default precision for all decimal fields: ######.##
        configurationBuilder.Properties<decimal>()
            .HavePrecision(8, 2);
    }
}