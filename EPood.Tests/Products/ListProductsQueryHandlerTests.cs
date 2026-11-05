using EPood.Application.Products.Handlers;
using EPood.Application.Products.Queries;
using EPood.Domain.Entities;
using EPood.Infrastructure.Data;
using EPood.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EPood.Tests.Products;

public class ListProductsQueryHandlerTests
{
    [Theory]
    [InlineData("ALPHA", "Alpha Phone")]
    [InlineData("WIRELESS", "Beta Headphones")]
    [InlineData("BOOKS", "Notebook")]
    public async Task ListProducts_Should_Search_Name_Description_And_Category(
        string search, string expectedName)
    {
        await using var dbContext = await CreateDatabase();
        var handler = new ListProductsQueryHandler(new ProductRepository(dbContext));

        var result = await handler.Handle(new ListProductsQuery { Search = search });

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(expectedName, Assert.Single(result.Value.Items).Name);
    }

    [Theory]
    [InlineData(false, "Beta Headphones")]
    [InlineData(true, "Alpha Phone")]
    public async Task ListProducts_Should_Sort_Before_Paging(
        bool descending, string expectedName)
    {
        await using var dbContext = await CreateDatabase();
        var handler = new ListProductsQueryHandler(new ProductRepository(dbContext));

        var result = await handler.Handle(new ListProductsQuery
        {
            Search = "electronics",
            SortBy = "price",
            Descending = descending,
            Page = 2,
            PageSize = 1
        });

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(1, result.Value.PageSize);
        Assert.Equal(expectedName, Assert.Single(result.Value.Items).Name);
    }

    [Theory]
    [InlineData(0, 10, "Page must be greater than zero")]
    [InlineData(1, -1, "Page size must be greater than zero")]
    public async Task ListProducts_Should_Reject_Invalid_Paging(
        int page, int pageSize, string expectedError)
    {
        await using var dbContext = await CreateDatabase();
        var handler = new ListProductsQueryHandler(new ProductRepository(dbContext));

        var result = await handler.Handle(new ListProductsQuery { Page = page, PageSize = pageSize });

        Assert.False(result.Success);
        Assert.Equal(expectedError, result.Error);
    }

    private static async Task<ApplicationDbContext> CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ApplicationDbContext(options);

        var electronics = new Category { Name = "Electronics" };
        var books = new Category { Name = "Books" };
        dbContext.Products.AddRange(
            new Product { Name = "Beta Headphones", Description = "Wireless audio", Price = 150, Category = electronics },
            new Product { Name = "Notebook", Price = 10, Category = books },
            new Product { Name = "Alpha Phone", Price = 100, Category = electronics });
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        return dbContext;
    }
}
