using ByteDefence.Api.Data;
using ByteDefence.Shared.DTOs;
using ByteDefence.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ByteDefence.Api.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext dbContext, INotificationService notificationService, ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Order>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        // Concurrently fetch the orders and aggregated counts to demonstrate parallel EF queries.
        var ordersTask = _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.CreatedBy)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var statsTask = GetStatisticsAsync(cancellationToken);

        await Task.WhenAll(ordersTask, statsTask);
        _logger.LogInformation("Order stats: {@Stats}", statsTask.Result);

        return ordersTask.Result;
    }

    public async Task<Order?> GetOrderAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.CreatedBy)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<OrderStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var grouped = await _dbContext.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var stats = new OrderStatistics();
        foreach (var entry in grouped)
        {
            switch (entry.Status)
            {
                case OrderStatus.Draft:
                    stats.Draft = entry.Count;
                    break;
                case OrderStatus.Pending:
                    stats.Pending = entry.Count;
                    break;
                case OrderStatus.Approved:
                    stats.Approved = entry.Count;
                    break;
                case OrderStatus.Completed:
                    stats.Completed = entry.Count;
                    break;
                case OrderStatus.Cancelled:
                    stats.Cancelled = entry.Count;
                    break;
            }
        }

        return stats;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderInput input, User currentUser, CancellationToken cancellationToken = default)
    {
        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            Title = input.Title,
            Status = input.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUser.Id,
            Items = input.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = string.Empty, // will be assigned after order has an id
                Name = i.Name,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };

        foreach (var item in order.Items)
        {
            item.OrderId = order.Id;
        }

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificationService.BroadcastOrderCreated(order);
        return await GetOrderAsync(order.Id, cancellationToken) ?? order;
    }

    public async Task<Order> UpdateOrderAsync(UpdateOrderInput input, User currentUser, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.CreatedBy)
            .FirstOrDefaultAsync(o => o.Id == input.Id, cancellationToken);

        if (order is null)
        {
            throw new InvalidOperationException($"Order {input.Id} not found");
        }

        order.Title = input.Title;
        order.Status = input.Status;
        order.UpdatedAt = DateTime.UtcNow;

        // Remove items not present anymore
        var toRemove = order.Items.Where(existing => input.Items.All(i => i.Id != existing.Id)).ToList();
        _dbContext.OrderItems.RemoveRange(toRemove);

        // Update existing and add new
        foreach (var itemInput in input.Items)
        {
            var existing = order.Items.FirstOrDefault(i => i.Id == itemInput.Id);
            if (existing is null)
            {
                order.Items.Add(new OrderItem
                {
                    Id = string.IsNullOrWhiteSpace(itemInput.Id) ? Guid.NewGuid().ToString() : itemInput.Id,
                    OrderId = order.Id,
                    Name = itemInput.Name,
                    Quantity = itemInput.Quantity,
                    Price = itemInput.Price
                });
            }
            else
            {
                existing.Name = itemInput.Name;
                existing.Quantity = itemInput.Quantity;
                existing.Price = itemInput.Price;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _notificationService.BroadcastOrderUpdated(order);

        return await GetOrderAsync(order.Id, cancellationToken) ?? order;
    }

    public async Task<bool> DeleteOrderAsync(string id, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            return false;
        }

        _dbContext.OrderItems.RemoveRange(order.Items);
        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificationService.BroadcastOrderDeleted(id);
        return true;
    }

    public async Task<Order> AddOrderItemAsync(AddOrderItemInput input, User currentUser, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == input.OrderId, cancellationToken);

        if (order is null)
        {
            throw new InvalidOperationException($"Order {input.OrderId} not found");
        }

        order.Items.Add(new OrderItem
        {
            Id = Guid.NewGuid().ToString(),
            OrderId = order.Id,
            Name = input.Name,
            Quantity = input.Quantity,
            Price = input.Price
        });

        order.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificationService.BroadcastOrderUpdated(order);
        return await GetOrderAsync(order.Id, cancellationToken) ?? order;
    }
}
