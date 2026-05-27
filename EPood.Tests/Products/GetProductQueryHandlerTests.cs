using EPood.Application.Products.Handlers;
using EPood.Application.Products.Queries;
using EPood.Domain.Entities;
using EPood.Infrastructure.Data;
using EPood.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EPood.Tests.Products;

public class GetProductQueryHandlerTests
{
    [Fact]
    public async Task GetProduct_Should_Return_Product()
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
        var handler = new GetProductQueryHandler(repository);

        var result = await handler.Handle(new GetProductQuery
        {
            Id = 1
        });

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal("Phone", result.Value.Name);
    }
}
