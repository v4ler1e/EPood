using EPood.Application.Products.Commands;
using EPood.Application.Products.Dtos;
using EPood.Domain.Entities;
using System.Net;
using System.Net.Http.Json;

namespace EPood.Tests.Integration;

public class ProductsApiTests : IClassFixture<EpoodApiFactory>
{
    private readonly HttpClient _client;

    public ProductsApiTests(
        EpoodApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_Should_Return_Success()
    {
        using var response =
            await _client.GetAsync("/api/Products");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task Product_Can_Be_Created_Updated_And_Deleted_Through_Api()
    {
        var categories = await _client.GetFromJsonAsync<List<Category>>("/api/Categories");
        Assert.NotNull(categories);
        var category = Assert.Single(categories, item => item.Name == "Electronics");
        var product = new SaveProductCommand
        {
            Name = "Test Monitor",
            Description = "Integration test product",
            Price = 150,
            Stock = 5,
            CategoryId = category.Id
        };

        using var create = await _client.PostAsJsonAsync("/api/Products", product);
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var id = await create.Content.ReadFromJsonAsync<int>();
        Assert.True(id > 0);
        var url = $"/api/Products/{id}";

        var created = await _client.GetFromJsonAsync<ProductDto>(url);
        Assert.NotNull(created);
        Assert.Equal(id, created.Id);
        Assert.Equal(product.Name, created.Name);
        Assert.Equal(product.Description, created.Description);
        Assert.Equal(product.Price, created.Price);
        Assert.Equal(product.Stock, created.Stock);
        Assert.Equal(product.CategoryId, created.CategoryId);

        product.Name = "Updated Monitor";
        product.Price = 175;
        product.Stock = 8;
        using var update = await _client.PutAsJsonAsync(url, product);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(id, await update.Content.ReadFromJsonAsync<int>());

        var updated = await _client.GetFromJsonAsync<ProductDto>(url);
        Assert.NotNull(updated);
        Assert.Equal(product.Name, updated.Name);
        Assert.Equal(product.Price, updated.Price);
        Assert.Equal(product.Stock, updated.Stock);

        using var delete = await _client.DeleteAsync(url);
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
        using var afterDelete = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }
}
