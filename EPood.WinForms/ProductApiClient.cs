using System.Net.Http.Json;
using EPood.Application.Common;
using EPood.Application.Products.Commands;
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

    public async Task<List<CategoryOption>> GetCategoriesAsync()
    {
        return await _http.GetFromJsonAsync<List<CategoryOption>>("api/Categories")
            ?? throw new InvalidOperationException("The API returned an empty category response.");
    }

    public async Task SaveProductAsync(SaveProductCommand product)
    {
        using var response = product.Id == 0
            ? await _http.PostAsJsonAsync("api/Products", product)
            : await _http.PutAsJsonAsync($"api/Products/{product.Id}", product);
        await CheckResponseAsync(response);
    }

    public async Task DeleteProductAsync(int id)
    {
        using var response = await _http.DeleteAsync($"api/Products/{id}");
        await CheckResponseAsync(response);
    }

    private static async Task CheckResponseAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error,
                null, response.StatusCode);
        }
    }
}

internal sealed record CategoryOption(int Id, string Name);
