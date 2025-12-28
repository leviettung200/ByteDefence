using ByteDefence.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ByteDefence.Api.Data;

public class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.CreatedBy)
            .WithMany();

        var admin = new User
        {
            Id = "user-admin",
            Username = "admin",
            DisplayName = "Administrator",
            Role = UserRole.Admin
        };

        var analyst = new User
        {
            Id = "user-analyst",
            Username = "user",
            DisplayName = "Analyst",
            Role = UserRole.User
        };

        var order1 = new Order
        {
            Id = "order-1",
            Title = "Network refresh",
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedByUserId = admin.Id
        };

        var order2 = new Order
        {
            Id = "order-2",
            Title = "SOC tooling uplift",
            Status = OrderStatus.Approved,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-2),
            CreatedByUserId = analyst.Id
        };

        var items = new List<OrderItem>
        {
            new()
            {
                Id = "item-1",
                OrderId = order1.Id,
                Name = "Firewall appliance",
                Quantity = 2,
                Price = 1200m
            },
            new()
            {
                Id = "item-2",
                OrderId = order1.Id,
                Name = "Switch stack",
                Quantity = 4,
                Price = 750m
            },
            new()
            {
                Id = "item-3",
                OrderId = order2.Id,
                Name = "SIEM license",
                Quantity = 50,
                Price = 15m
            },
            new()
            {
                Id = "item-4",
                OrderId = order2.Id,
                Name = "SOAR playbook build",
                Quantity = 1,
                Price = 5500m
            }
        };

        modelBuilder.Entity<User>().HasData(admin, analyst);
        modelBuilder.Entity<Order>().HasData(order1, order2);
        modelBuilder.Entity<OrderItem>().HasData(items);
    }
}
