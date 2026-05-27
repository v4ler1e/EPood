using System.ComponentModel.DataAnnotations;

namespace EPood.Blazor.Models;

public class SaveProductModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = "";

    public string? Description { get; set; }

    [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Range(0, 10000, ErrorMessage = "Stock cannot be negative")]
    public int Stock { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }
}