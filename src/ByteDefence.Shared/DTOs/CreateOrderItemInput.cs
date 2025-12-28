using System.ComponentModel.DataAnnotations;

namespace ByteDefence.Shared.DTOs;

public class CreateOrderItemInput
{
    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Price { get; set; }
}
