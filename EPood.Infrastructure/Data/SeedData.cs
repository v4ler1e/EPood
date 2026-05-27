using EPood.Domain.Entities;

namespace EPood.Infrastructure.Data;

public static class SeedData
{
    public static void Generate(ApplicationDbContext dbContext)
    {
        if (dbContext.Categories.Any())
        {
            return;
        }

        var electronics = new Category
        {
            Name = "Electronics"
        };

        var clothes = new Category
        {
            Name = "Clothes"
        };

        dbContext.Categories.AddRange(
            electronics,
            clothes);

        dbContext.Products.AddRange(
            new Product
            {
                Name = "Laptop",
                Description = "Gaming laptop",
                Price = 1200,
                Stock = 5,
                Category = electronics
            },
            new Product
            {
                Name = "T-Shirt",
                Description = "White t-shirt",
                Price = 20,
                Stock = 50,
                Category = clothes
            }
        );

        dbContext.SaveChanges();
    }
}
