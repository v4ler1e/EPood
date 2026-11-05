using EPood.WpfApplication.Api;
using EPood.WpfApplication.Dialogs;
using EPood.WpfApplication.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace EPood.WpfApplication.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ProductApiClient _apiClient;
    private readonly IDialogProvider _dialogProvider;

    public ObservableCollection<ProductModel> Products { get; set; } = new();
    public ObservableCollection<CategoryModel> Categories { get; set; } = new();
    public ObservableCollection<OrderModel> Orders { get; set; } = new();

    private ProductModel? _selectedProduct;
    private OrderModel? _selectedOrder;

    public ProductModel NewProduct { get; set; } = new();
    public SaveOrderModel NewOrder { get; set; } = new();

    private string _searchText = "";

    private string _orderSearchText = "";

    public string OrderSearchText
    {
        get => _orderSearchText;
        set
        {
            _orderSearchText = value;
            OnPropertyChanged(nameof(OrderSearchText));
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged(nameof(SearchText));
        }
    }

    public ProductModel? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            _selectedProduct = value;
            OnPropertyChanged(nameof(SelectedProduct));
        }
    }

    public OrderModel? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            _selectedOrder = value;
            OnPropertyChanged(nameof(SelectedOrder));
        }
    }

    public ICommand LoadCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand UpdateCommand { get; }

    public ICommand AddOrderCommand { get; }
    public ICommand UpdateOrderCommand { get; }
    public ICommand DeleteOrderCommand { get; }

    public MainWindowViewModel() : this(new ProductApiClient(), new DialogProvider())
    {
    }

    public MainWindowViewModel(ProductApiClient apiClient, IDialogProvider dialogProvider)
    {
        _apiClient = apiClient;
        _dialogProvider = dialogProvider;
        LoadCommand = new RelayCommand(Reload);
        DeleteCommand = new RelayCommand(Delete);
        AddCommand = new RelayCommand(Add);
        UpdateCommand = new RelayCommand(Update);

        AddOrderCommand = new RelayCommand(AddOrder);
        UpdateOrderCommand = new RelayCommand(UpdateOrder);
        DeleteOrderCommand = new RelayCommand(DeleteOrder);
    }

    private async Task Reload()
    {
        try
        {
            await LoadData();
        }
        catch (Exception ex)
        {
            _dialogProvider.ShowError(ex.Message);
        }
    }

    private async Task LoadData()
    {
        var products = await _apiClient.GetProducts(SearchText);
        var categories = await _apiClient.GetCategories();
        var orders = await _apiClient.GetOrders(OrderSearchText);

        Products.Clear();

        foreach (var product in products)
        {
            Products.Add(product);
        }

        Categories.Clear();

        foreach (var category in categories)
        {
            Categories.Add(category);
        }

        Orders.Clear();

        foreach (var order in orders)
        {
            Orders.Add(order);
        }

        if (Products.Any() && NewOrder.ProductId == 0)
        {
            NewOrder.ProductId = Products.First().Id;
            OnPropertyChanged(nameof(NewOrder));
        }
    }

    private async Task AddOrder()
    {
        try
        {
            await _apiClient.AddOrder(NewOrder);

            _dialogProvider.ShowMessage("Order added successfully");

            NewOrder = new SaveOrderModel
            {
                ProductId = Products.Any() ? Products.First().Id : 0,
                Quantity = 1
            };

            OnPropertyChanged(nameof(NewOrder));

            await LoadData();
        }
        catch (Exception ex)
        {
            _dialogProvider.ShowError(ex.Message);
        }
    }

    private async Task UpdateOrder()
    {
        if (SelectedOrder == null)
        {
            _dialogProvider.ShowError("Select order first");
            return;
        }

        try
        {
            var order = new SaveOrderModel
            {
                Id = SelectedOrder.Id,
                CustomerName = SelectedOrder.CustomerName,
                ProductId = SelectedOrder.ProductId,
                Quantity = SelectedOrder.Quantity
            };

            await _apiClient.UpdateOrder(order);

            _dialogProvider.ShowMessage("Order updated");

            await LoadData();
        }
        catch (Exception ex)
        {
            _dialogProvider.ShowError(ex.Message);
        }
    }

    private async Task DeleteOrder()
    {
        var order = SelectedOrder;
        if (order == null)
        {
            _dialogProvider.ShowError("Select order first");
            return;
        }

        var confirm = _dialogProvider.Confirm(
            $"Delete order #{order.Id}?");

        if (!confirm)
        {
            return;
        }

        try
        {
            await _apiClient.DeleteOrder(order.Id);

            Orders.Remove(order);

            _dialogProvider.ShowMessage("Order deleted");
        }
        catch (Exception ex)
        {
            _dialogProvider.ShowError(ex.Message);
        }
    }

    private async Task Delete()
    {
        var product = SelectedProduct;
        if (product == null)
        {
            _dialogProvider.ShowError("Select product first");
            return;
        }

        var confirm = _dialogProvider.Confirm(
            $"Delete {product.Name}?");

        if (!confirm)
        {
            return;
        }

        try
        {
            await _apiClient.DeleteProduct(product.Id);

            Products.Remove(product);

            _dialogProvider.ShowMessage("Product deleted");
        }
        catch (Exception ex)
        {
            _dialogProvider.ShowError(ex.Message);
        }
    }

    private async Task Add()
    {
        try
        {
            await _apiClient.AddProduct(NewProduct);

            _dialogProvider.ShowMessage("Product added successfully");

            NewProduct = new ProductModel();

            OnPropertyChanged(nameof(NewProduct));

            await LoadData();
        }
        catch (Exception ex)
        {
            _dialogProvider.ShowError(ex.Message);
        }
    }

    private async Task Update()
    {
        if (SelectedProduct == null)
        {
            _dialogProvider.ShowError("Select product first");
            return;
        }

        try
        {
            await _apiClient.UpdateProduct(SelectedProduct);

            _dialogProvider.ShowMessage("Product updated");
        }
        catch (Exception ex)
        {
            _dialogProvider.ShowError(ex.Message);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
