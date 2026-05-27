using EPood.Application.Products.Commands;
using EPood.Application.Products.Handlers;
using EPood.Domain.Entities;
using EPood.Infrastructure.Data;
using EPood.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EPood.Tests.Products;

public class SaveProductCommandHandlerTests
{
    [Fact]
    public async Task SaveProduct_Should_Create_Product()
    {
        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        var dbContext = new ApplicationDbContext(options);

        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Electronics"
        });

        await dbContext.SaveChangesAsync();

        var repository = new ProductRepository(dbContext);

        var handler = new SaveProductCommandHandler(repository);

        var command = new SaveProductCommand
        {
            Name = "Laptop",
            Description = "Gaming laptop",
            Price = 1500,
            Stock = 5,
            CategoryId = 1
        };

        var result = await handler.Handle(command);

        Assert.True(result.Success);

        var product =
            await dbContext.Products.FirstOrDefaultAsync();

        Assert.NotNull(product);

        Assert.Equal("Laptop", product.Name);
    }
}