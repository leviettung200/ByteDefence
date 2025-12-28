using System.ComponentModel.DataAnnotations;
using ByteDefence.Shared.Models;

namespace ByteDefence.Shared.DTOs;

public class UpdateOrderInput
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public List<UpdateOrderItemInput> Items { get; set; } = new();
}
