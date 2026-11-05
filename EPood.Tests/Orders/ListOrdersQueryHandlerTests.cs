using EPood.Application.Orders.Handlers;
using EPood.Application.Orders.Queries;
using EPood.Domain.Entities;
using EPood.Infrastructure.Data;
using EPood.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EPood.Tests.Orders;

public class ListOrdersQueryHandlerTests
{
    [Theory]
    [InlineData("ALICE", "Alice", "Phone")]
    [InlineData("LAPTOP", "Charlie", "Laptop")]
    public async Task ListOrders_Should_Search_Customer_And_Product(
        string search, string expectedCustomer, string expectedProduct)
    {
        await using var dbContext = await CreateDatabase();
        var handler = new ListOrdersQueryHandler(new OrderRepository(dbContext));

        var result = await handler.Handle(new ListOrdersQuery { Search = search });

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.TotalCount);
        var order = Assert.Single(result.Value.Items);
        Assert.Equal(expectedCustomer, order.CustomerName);
        Assert.Equal(expectedProduct, order.ProductName);
    }

    [Fact]
    public async Task ListOrders_Should_Page_Filtered_Orders_Newest_First()
    {
        await using var dbContext = await CreateDatabase();
        var handler = new ListOrdersQueryHandler(new OrderRepository(dbContext));

        var result = await handler.Handle(new ListOrdersQuery
        {
            Search = "phone",
            Page = 2,
            PageSize = 1
        });

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(1, result.Value.PageSize);
        Assert.Equal("Alice", Assert.Single(result.Value.Items).CustomerName);
    }

    [Theory]
    [InlineData(0, 10, "Page must be greater than zero")]
    [InlineData(1, -1, "Page size must be greater than zero")]
    public async Task ListOrders_Should_Reject_Invalid_Paging(
        int page, int pageSize, string expectedError)
    {
        await using var dbContext = await CreateDatabase();
        var handler = new ListOrdersQueryHandler(new OrderRepository(dbContext));

        var result = await handler.Handle(new ListOrdersQuery { Page = page, PageSize = pageSize });

        Assert.False(result.Success);
        Assert.Equal(expectedError, result.Error);
    }

    private static async Task<ApplicationDbContext> CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ApplicationDbContext(options);

        var category = new Category { Name = "Electronics" };
        var phone = new Product { Name = "Phone", Price = 100, Category = category };
        var laptop = new Product { Name = "Laptop", Price = 500, Category = category };
        var firstDay = new DateTime(2026, 1, 1);

        dbContext.Orders.AddRange(
            new Order { CustomerName = "Alice", Product = phone, CreatedAt = firstDay, Quantity = 1, TotalPrice = phone.Price },
            new Order { CustomerName = "Charlie", Product = laptop, CreatedAt = firstDay.AddDays(2), Quantity = 1, TotalPrice = laptop.Price },
            new Order { CustomerName = "Bob", Product = phone, CreatedAt = firstDay.AddDays(1), Quantity = 1, TotalPrice = phone.Price });
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        return dbContext;
    }
}
