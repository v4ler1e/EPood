using System.ComponentModel.DataAnnotations;

namespace EPood.Application.Orders.Commands;

public class SaveOrderCommand
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 10000)]
    public int Quantity { get; set; }
}
