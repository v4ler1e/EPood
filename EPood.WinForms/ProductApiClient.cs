using System.Net.Http.Json;
using EPood.Application.Common;
using EPood.Application.Products.Dtos;

namespace EPood.WinForms;

internal sealed class ProductApiClient
{
    private readonly HttpClient _http;

    public ProductApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(int page, string search)
    {
        var url = $"api/Products?page={page}&pageSize=10&search={Uri.EscapeDataString(search.Trim())}";
        return await _http.GetFromJsonAsync<PagedResult<ProductDto>>(url)
            ?? throw new InvalidOperationException("The API returned an empty product list response.");
    }
}
