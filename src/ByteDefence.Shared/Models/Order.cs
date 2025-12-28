using System.Collections.Generic;
using System.Linq;

namespace ByteDefence.Shared.Models;

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public User? CreatedBy { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Price * i.Quantity);
}
