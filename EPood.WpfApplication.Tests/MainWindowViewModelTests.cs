using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Input;
using EPood.WpfApplication.Api;
using EPood.WpfApplication.Dialogs;
using EPood.WpfApplication.Models;
using EPood.WpfApplication.ViewModels;

namespace EPood.WpfApplication.Tests;

public class MainWindowViewModelTests
{
    [Fact]
    public void Search_And_Selection_Raise_PropertyChanged()
    {
        using var http = CreateHttp(new StubApi());
        var viewModel = new MainWindowViewModel(new ProductApiClient(http), new Dialogs());
        var changes = new List<string?>();
        viewModel.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

        viewModel.SearchText = "Phone";
        viewModel.OrderSearchText = "Alice";
        viewModel.SelectedProduct = new ProductModel { Id = 1 };
        viewModel.SelectedOrder = new OrderModel { Id = 2 };

        Assert.Equal(new[]
        {
            nameof(viewModel.SearchText), nameof(viewModel.OrderSearchText),
            nameof(viewModel.SelectedProduct), nameof(viewModel.SelectedOrder)
        }, changes);
    }

    [Fact]
    public async Task Load_Populates_All_Lists_And_Initializes_New_Order()
    {
        var api = new StubApi();
        using var http = CreateHttp(api);
        var dialogs = new Dialogs();
        var viewModel = new MainWindowViewModel(new ProductApiClient(http), dialogs)
        {
            SearchText = "Phone & case",
            OrderSearchText = "Alice & Bob"
        };
        var changes = new List<string?>();
        viewModel.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

        await Execute(viewModel.LoadCommand);

        Assert.Equal(7, Assert.Single(viewModel.Products).Id);
        Assert.Equal("Electronics", Assert.Single(viewModel.Categories).Name);
        Assert.Equal("Alice", Assert.Single(viewModel.Orders).CustomerName);
        Assert.Equal(7, viewModel.NewOrder.ProductId);
        Assert.Contains(nameof(viewModel.NewOrder), changes);
        Assert.Contains(api.Requests, request => request.Path == "/api/Products?search=Phone%20%26%20case");
        Assert.Contains(api.Requests, request => request.Path.EndsWith("&search=Alice%20%26%20Bob"));
        Assert.Empty(dialogs.Errors);
    }

    [Fact]
    public async Task Load_Failure_Shows_Error_And_Keeps_Displayed_Data()
    {
        var api = new StubApi { FailedPath = "/api/Categories" };
        using var http = CreateHttp(api);
        var dialogs = new Dialogs();
        var viewModel = new MainWindowViewModel(new ProductApiClient(http), dialogs);
        var existing = new ProductModel { Id = 3 };
        viewModel.Products.Add(existing);

        await Execute(viewModel.LoadCommand);

        Assert.Same(existing, Assert.Single(viewModel.Products));
        Assert.Single(dialogs.Errors);
        Assert.Empty(dialogs.Messages);
    }

    [Fact]
    public async Task Delete_Without_A_Selection_Does_Not_Contact_Api()
    {
        var api = new StubApi();
        using var http = CreateHttp(api);
        var dialogs = new Dialogs();
        var viewModel = new MainWindowViewModel(new ProductApiClient(http), dialogs);

        await Execute(viewModel.DeleteCommand);
        await Execute(viewModel.DeleteOrderCommand);

        Assert.Empty(api.Requests);
        Assert.Equal(new[] { "Select product first", "Select order first" }, dialogs.Errors);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Delete_Only_Removes_A_Product_After_Confirmed_Success(bool confirm, bool fail)
    {
        const string path = "/api/Products/7";
        var api = new StubApi { FailedPath = fail ? path : null };
        using var http = CreateHttp(api);
        var dialogs = new Dialogs { Confirmed = confirm };
        var product = new ProductModel { Id = 7, Name = "Phone" };
        var viewModel = new MainWindowViewModel(new ProductApiClient(http), dialogs) { SelectedProduct = product };
        viewModel.Products.Add(product);

        await Execute(viewModel.DeleteCommand);

        Assert.Equal("Delete Phone?", Assert.Single(dialogs.Confirmations));
        if (confirm)
        {
            var request = Assert.Single(api.Requests);
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(path, request.Path);
        }
        else
        {
            Assert.Empty(api.Requests);
        }

        if (confirm && !fail)
        {
            Assert.Empty(viewModel.Products);
            Assert.Equal("Product deleted", Assert.Single(dialogs.Messages));
        }
        else
        {
            Assert.Same(product, Assert.Single(viewModel.Products));
            Assert.Empty(dialogs.Messages);
        }

        Assert.Equal(fail ? 1 : 0, dialogs.Errors.Count);
    }

    [Fact]
    public async Task Add_Product_Sends_Fields_And_Resets_The_Form()
    {
        var api = new StubApi();
        using var http = CreateHttp(api);
        var dialogs = new Dialogs();
        var viewModel = new MainWindowViewModel(new ProductApiClient(http), dialogs)
        {
            NewProduct = new ProductModel { Name = "Monitor", Price = 125, Stock = 3, CategoryId = 1 }
        };
        var changes = new List<string?>();
        viewModel.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

        await Execute(viewModel.AddCommand);

        var request = Assert.Single(api.Requests, item => item.Method == HttpMethod.Post);
        Assert.Equal("/api/Products", request.Path);
        using var body = JsonDocument.Parse(request.Body!);
        Assert.Equal("Monitor", body.RootElement.GetProperty("name").GetString());
        Assert.Equal(125m, body.RootElement.GetProperty("price").GetDecimal());
        Assert.Empty(viewModel.NewProduct.Name);
        Assert.Contains(nameof(viewModel.NewProduct), changes);
        Assert.Single(viewModel.Products);
        Assert.Empty(dialogs.Errors);
    }

    [Fact]
    public async Task Update_Order_Uses_The_Selected_Order_And_Refreshes_The_List()
    {
        var api = new StubApi();
        using var http = CreateHttp(api);
        var dialogs = new Dialogs();
        var viewModel = new MainWindowViewModel(new ProductApiClient(http), dialogs)
        {
            SelectedOrder = new OrderModel { Id = 8, CustomerName = "Alice", ProductId = 7, Quantity = 3 }
        };

        await Execute(viewModel.UpdateOrderCommand);

        var request = Assert.Single(api.Requests, item => item.Method == HttpMethod.Put);
        Assert.Equal("/api/Orders/8", request.Path);
        using var body = JsonDocument.Parse(request.Body!);
        Assert.Equal("Alice", body.RootElement.GetProperty("customerName").GetString());
        Assert.Equal(7, body.RootElement.GetProperty("productId").GetInt32());
        Assert.Equal(3, body.RootElement.GetProperty("quantity").GetInt32());
        Assert.Single(viewModel.Orders);
        Assert.Equal("Order updated", Assert.Single(dialogs.Messages));
        Assert.Empty(dialogs.Errors);
    }

    private static Task Execute(ICommand command) => Assert.IsType<RelayCommand>(command).ExecuteAsync();

    private static HttpClient CreateHttp(StubApi api) => new(api)
    {
        BaseAddress = new Uri("http://epood.test/")
    };

    private sealed class Dialogs : IDialogProvider
    {
        public bool Confirmed { get; init; }
        public List<string> Confirmations { get; } = new();
        public List<string> Messages { get; } = new();
        public List<string> Errors { get; } = new();

        public bool Confirm(string message)
        {
            Confirmations.Add(message);
            return Confirmed;
        }

        public void ShowMessage(string message) => Messages.Add(message);
        public void ShowError(string message) => Errors.Add(message);
    }

    private sealed record Request(HttpMethod Method, string Path, string? Body);

    private sealed class StubApi : HttpMessageHandler
    {
        public string? FailedPath { get; init; }
        public List<Request> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri!;
            var body = request.Content == null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new Request(request.Method, uri.PathAndQuery, body));
            if (uri.AbsolutePath == FailedPath)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            if (request.Method != HttpMethod.Get)
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = uri.AbsolutePath switch
                {
                    "/api/Products" => JsonContent.Create(new { Items = new[] { new ProductModel { Id = 7, Name = "Phone" } } }),
                    "/api/Categories" => JsonContent.Create(new[] { new CategoryModel { Id = 1, Name = "Electronics" } }),
                    "/api/Orders" => JsonContent.Create(new { Items = new[] { new OrderModel { Id = 8, CustomerName = "Alice" } } }),
                    _ => throw new InvalidOperationException($"Unexpected GET {uri.AbsolutePath}")
                }
            };
        }
    }
}
