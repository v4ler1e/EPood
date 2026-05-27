using EPood.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;

namespace EPood.Tests.Integration;

public class ProductsApiTests :
    IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsApiTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_Should_Return_Success()
    {
        var response =
            await _client.GetAsync("/api/Products");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }
}
