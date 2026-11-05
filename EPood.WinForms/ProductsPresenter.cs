using EPood.Application.Products.Dtos;

namespace EPood.WinForms;

internal sealed class ProductsPresenter
{
    private readonly IProductsView _view;
    private readonly ProductApiClient _api;
    private int _page = 1;
    private int _totalPages;
    private string _search = "";
    private bool _busy;

    public ProductsPresenter(IProductsView view, ProductApiClient api)
    {
        _view = view;
        _api = api;

        view.RefreshRequested += async (_, _) => await RunAsync(() => LoadAsync(_page, _search));
        view.SearchRequested += async (_, _) => await RunAsync(() => LoadAsync(1, _view.SearchText));
        view.PreviousPageRequested += async (_, _) => await RunAsync(() => LoadAsync(_page - 1, _search));
        view.NextPageRequested += async (_, _) => await RunAsync(() => LoadAsync(_page + 1, _search));
        view.AddRequested += async (_, _) => await RunAsync(() => SaveAsync(null));
        view.EditRequested += async (_, _) => await RunAsync(EditAsync);
        view.DeleteRequested += async (_, _) => await RunAsync(DeleteAsync);
    }

    private async Task RunAsync(Func<Task> action)
    {
        if (_busy || _view.IsDisposed)
        {
            return;
        }

        _busy = true;
        _view.SetBusy(true, false, false);
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ShowStatus($"Request failed. {ex.Message}");
        }
        finally
        {
            _busy = false;
            if (!_view.IsDisposed)
            {
                _view.SetBusy(false, _page > 1, _page < _totalPages);
            }
        }
    }

    private async Task LoadAsync(int page, string search)
    {
        ShowStatus("Loading products...");
        var result = await _api.GetProductsAsync(page, search);
        if (page > Math.Max(1, result.TotalPages))
        {
            result = await _api.GetProductsAsync(Math.Max(1, result.TotalPages), search);
        }

        _page = result.Page;
        _totalPages = result.TotalPages;
        _search = search;
        if (!_view.IsDisposed)
        {
            _view.ShowProducts(result);
        }
    }

    private async Task EditAsync()
    {
        var product = _view.SelectedProduct;
        if (product == null)
        {
            ShowStatus("Select a product to edit.");
            return;
        }

        await SaveAsync(product);
    }

    private async Task SaveAsync(ProductDto? product)
    {
        var categories = await _api.GetCategoriesAsync();
        if (_view.IsDisposed)
        {
            return;
        }

        if (categories.Count == 0)
        {
            ShowStatus("A category is required to save a product.");
            return;
        }

        var command = _view.EditProduct(product, categories);
        if (command == null)
        {
            return;
        }

        await _api.SaveProductAsync(command);
        await ReloadAfterWriteAsync(product == null ? "Product added." : "Product updated.");
    }

    private async Task DeleteAsync()
    {
        var product = _view.SelectedProduct;
        if (product == null)
        {
            ShowStatus("Select a product to delete.");
            return;
        }

        if (_view.ConfirmDelete(product))
        {
            await _api.DeleteProductAsync(product.Id);
            await ReloadAfterWriteAsync("Product deleted.");
        }
    }

    private async Task ReloadAfterWriteAsync(string confirmation)
    {
        if (_view.IsDisposed)
        {
            return;
        }

        try
        {
            await LoadAsync(_page, _search);
        }
        catch (Exception ex)
        {
            ShowStatus($"{confirmation} Could not refresh products. {ex.Message}");
        }
    }

    private void ShowStatus(string message)
    {
        if (!_view.IsDisposed)
        {
            _view.ShowStatus(message);
        }
    }
}
