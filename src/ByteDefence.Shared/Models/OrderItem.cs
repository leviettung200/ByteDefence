namespace ByteDefence.Shared.Models;

public class OrderItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
