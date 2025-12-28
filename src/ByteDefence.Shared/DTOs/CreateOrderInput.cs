using System.ComponentModel.DataAnnotations;
using ByteDefence.Shared.Models;

namespace ByteDefence.Shared.DTOs;

public class CreateOrderInput
{
    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    public List<CreateOrderItemInput> Items { get; set; } = new();
}
