using EPood.WpfApplication.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace EPood.WpfApplication.Api;

public class ProductApiClient
{
    private readonly HttpClient _httpClient;

    public ProductApiClient()
    {
        _httpClient = new HttpClient();

        _httpClient.BaseAddress =
            new Uri("https://localhost:7106/");
    }

    public async Task<List<ProductModel>> GetProducts(string? search = null)
    {
        var url = "api/Products";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"?search={search}";
        }

        var response =
            await _httpClient.GetFromJsonAsync<ProductListResponse>(url);

        return response?.Items ?? new List<ProductModel>();
    }

    public async Task DeleteProduct(int id)
    {
        await _httpClient.DeleteAsync($"api/Products/{id}");
    }
    public async Task AddProduct(ProductModel product)
    {
        await _httpClient.PostAsJsonAsync("api/Products", product);
    }

    public async Task UpdateProduct(ProductModel product)
    {
        await _httpClient.PutAsJsonAsync(
            $"api/Products/{product.Id}",
            product);
    }
    public async Task<List<CategoryModel>> GetCategories()
    {
        var categories =
            await _httpClient.GetFromJsonAsync<List<CategoryModel>>(
                "api/Categories");

        return categories ?? new List<CategoryModel>();
    }
}

public class ProductListResponse
{
    public List<ProductModel> Items { get; set; } =
        new();
}