using EPood.WpfApplication.Api;
using EPood.WpfApplication.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using EPood.WpfApplication.Dialogs;

namespace EPood.WpfApplication.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ProductApiClient _apiClient = new();
    private readonly IDialogProvider _dialogProvider = new DialogProvider();

    public ObservableCollection<ProductModel> Products { get; set; } = new();
    public ObservableCollection<CategoryModel> Categories { get; set; } = new();

    private ProductModel? _selectedProduct;
    public ProductModel NewProduct { get; set; } = new();
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

    public ICommand LoadCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand UpdateCommand { get; }

    public MainWindowViewModel()
    {
        LoadCommand = new RelayCommand(LoadData);
        DeleteCommand = new RelayCommand(Delete);
        AddCommand = new RelayCommand(Add);
        UpdateCommand = new RelayCommand(Update);
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