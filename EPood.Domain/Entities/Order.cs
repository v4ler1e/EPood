using System.ComponentModel.DataAnnotations;

namespace EPood.Domain.Entities;

public class Order
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Range(1, 10000)]
    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }
}