using EPood.WpfApplication.Api;
using EPood.WpfApplication.Dialogs;
using EPood.WpfApplication.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace EPood.WpfApplication.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ProductApiClient _apiClient = new();
    private readonly IDialogProvider _dialogProvider = new DialogProvider();

    public ObservableCollection<ProductModel> Products { get; set; } = new();
    public ObservableCollection<CategoryModel> Categories { get; set; } = new();
    public ObservableCollection<OrderModel> Orders { get; set; } = new();

    private ProductModel? _selectedProduct;
    private OrderModel? _selectedOrder;

    public ProductModel NewProduct { get; set; } = new();
    public SaveOrderModel NewOrder { get; set; } = new();

    private string _searchText = "";

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

    public MainWindowViewModel()
    {
        LoadCommand = new RelayCommand(LoadData);
        DeleteCommand = new RelayCommand(Delete);
        AddCommand = new RelayCommand(Add);
        UpdateCommand = new RelayCommand(Update);

        AddOrderCommand = new RelayCommand(AddOrder);
        UpdateOrderCommand = new RelayCommand(UpdateOrder);
        DeleteOrderCommand = new RelayCommand(DeleteOrder);
    }

    private async Task LoadData()
    {
        Products.Clear();

        var products = await _apiClient.GetProducts(SearchText);

        foreach (var product in products)
        {
            Products.Add(product);
        }

        Categories.Clear();

        var categories = await _apiClient.GetCategories();

        foreach (var category in categories)
        {
            Categories.Add(category);
        }

        Orders.Clear();

        var orders = await _apiClient.GetOrders();

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
        if (SelectedOrder == null)
        {
            _dialogProvider.ShowError("Select order first");
            return;
        }

        var confirm = _dialogProvider.Confirm(
            $"Delete order #{SelectedOrder.Id}?");

        if (!confirm)
        {
            return;
        }

        try
        {
            await _apiClient.DeleteOrder(SelectedOrder.Id);

            Orders.Remove(SelectedOrder);

            _dialogProvider.ShowMessage("Order deleted");
        }
        catch (Exception ex)
        {
            _dialogProvider.ShowError(ex.Message);
        }
    }

    private async Task Delete()
    {
        if (SelectedProduct == null)
        {
            _dialogProvider.ShowError("Select product first");
            return;
        }

        var confirm = _dialogProvider.Confirm(
            $"Delete {SelectedProduct.Name}?");

        if (!confirm)
        {
            return;
        }

        try
        {
            await _apiClient.DeleteProduct(SelectedProduct.Id);

            Products.Remove(SelectedProduct);

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