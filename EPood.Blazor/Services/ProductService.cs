using EPood.Blazor.Models;
using System.Net.Http.Json;

namespace EPood.Blazor.Services;

public class ProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductListResponse> GetProducts(
        string? search = null,
        int page = 1,
        int pageSize = 5)
    {
        var url =
            $"https://localhost:7106/api/Products?page={page}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={search}";
        }

        var response =
            await _httpClient.GetFromJsonAsync<ProductListResponse>(url);

        return response ?? new ProductListResponse();
    }

    public async Task<List<CategoryModel>> GetCategories()
    {
        var response = await _httpClient.GetFromJsonAsync<List<CategoryModel>>(
            "https://localhost:7106/api/Categories");

        return response ?? new List<CategoryModel>();
    }

    public async Task AddProduct(SaveProductModel product)
    {
        await _httpClient.PostAsJsonAsync(
            "https://localhost:7106/api/Products",
            product);
    }

    public async Task UpdateProduct(SaveProductModel product)
    {
        await _httpClient.PutAsJsonAsync(
            $"https://localhost:7106/api/Products/{product.Id}",
            product);
    }

    public async Task DeleteProduct(int id)
    {
        await _httpClient.DeleteAsync(
            $"https://localhost:7106/api/Products/{id}");
    }

    public async Task<List<OrderModel>> GetOrders()
    {
        var response = await _httpClient.GetFromJsonAsync<OrderListResponse>(
            "https://localhost:7106/api/Orders?page=1&pageSize=100");

        return response?.Items ?? new List<OrderModel>();
    }

    public async Task AddOrder(SaveOrderModel order)
    {
        await _httpClient.PostAsJsonAsync(
            "https://localhost:7106/api/Orders",
            order);
    }

    public async Task UpdateOrder(SaveOrderModel order)
    {
        await _httpClient.PutAsJsonAsync(
            $"https://localhost:7106/api/Orders/{order.Id}",
            order);
    }

    public async Task DeleteOrder(int id)
    {
        await _httpClient.DeleteAsync(
            $"https://localhost:7106/api/Orders/{id}");
    }
}

public class OrderListResponse
{
    public List<OrderModel> Items { get; set; } = new();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}