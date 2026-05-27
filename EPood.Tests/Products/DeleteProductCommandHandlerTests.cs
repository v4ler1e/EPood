using EPood.Application.Products.Commands;
using EPood.Application.Products.Handlers;
using EPood.Domain.Entities;
using EPood.Infrastructure.Data;
using EPood.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EPood.Tests.Products;

public class DeleteProductCommandHandlerTests
{
    [Fact]
    public async Task DeleteProduct_Should_Remove_Product()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);

        var category = new Category
        {
            Id = 1,
            Name = "Electronics"
        };

        dbContext.Categories.Add(category);

        dbContext.Products.Add(new Product
        {
            Id = 1,
            Name = "Phone",
            Description = "Smartphone",
            Price = 500,
            Stock = 10,
            CategoryId = 1,
            Category = category
        });

        await dbContext.SaveChangesAsync();

        var repository = new ProductRepository(dbContext);
        var handler = new DeleteProductCommandHandler(repository);

        var result = await handler.Handle(new DeleteProductCommand
        {
            Id = 1
        });

        Assert.True(result.Success);
        Assert.Empty(dbContext.Products);
    }
}
