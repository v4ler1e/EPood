using EPood.Application.Common;
using EPood.Application.Orders.Dtos;
using EPood.Application.Products.Dtos;
using System.Net;
using System.Net.Http.Json;

namespace EPood.Tests.Integration;

public class OrdersApiTests : IClassFixture<EpoodApiFactory>
{
    private readonly HttpClient _client;

    public OrdersApiTests(EpoodApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Order_Can_Be_Created_Updated_And_Deleted_Through_Api()
    {
        var products = await _client.GetFromJsonAsync<PagedResult<ProductDto>>("/api/Products");
        Assert.NotNull(products);
        var product = Assert.Single(products.Items, item => item.Name == "Laptop");

        using var create = await _client.PostAsJsonAsync("/api/Orders", new
        {
            CustomerName = "Alice",
            ProductId = product.Id,
            Quantity = 2
        });

        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var id = await create.Content.ReadFromJsonAsync<int>();
        Assert.True(id > 0);
        var url = $"/api/Orders/{id}";

        var created = await _client.GetFromJsonAsync<OrderDto>(url);
        Assert.NotNull(created);
        Assert.Equal(id, created.Id);
        Assert.Equal("Alice", created.CustomerName);
        Assert.Equal(product.Id, created.ProductId);
        Assert.Equal(product.Name, created.ProductName);
        Assert.Equal(2, created.Quantity);
        Assert.Equal(product.Price * 2, created.TotalPrice);

        using var update = await _client.PutAsJsonAsync(url, new
        {
            CustomerName = "Alice Updated",
            ProductId = product.Id,
            Quantity = 3
        });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(id, await update.Content.ReadFromJsonAsync<int>());
        var updated = await _client.GetFromJsonAsync<OrderDto>(url);
        Assert.NotNull(updated);
        Assert.Equal("Alice Updated", updated.CustomerName);
        Assert.Equal(3, updated.Quantity);
        Assert.Equal(product.Price * 3, updated.TotalPrice);

        using var delete = await _client.DeleteAsync(url);
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
        using var afterDelete = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }
}
