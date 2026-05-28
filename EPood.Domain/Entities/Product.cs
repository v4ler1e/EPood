using System.ComponentModel.DataAnnotations;

namespace EPood.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Range(0, 10000)]
    public int Stock { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }
    public List<Order> Orders { get; set; } = new();
}