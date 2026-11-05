using EPood.Application.Common;
using EPood.Application.Products.Commands;
using EPood.Application.Products.Dtos;

namespace EPood.WinForms;

internal interface IProductsView
{
    event EventHandler? RefreshRequested;
    event EventHandler? SearchRequested;
    event EventHandler? PreviousPageRequested;
    event EventHandler? NextPageRequested;
    event EventHandler? AddRequested;
    event EventHandler? EditRequested;
    event EventHandler? DeleteRequested;

    bool IsDisposed { get; }
    string SearchText { get; }
    ProductDto? SelectedProduct { get; }
    void SetBusy(bool busy, bool hasPrevious, bool hasNext);
    void ShowProducts(PagedResult<ProductDto> products);
    void ShowStatus(string message);
    SaveProductCommand? EditProduct(ProductDto? product, List<CategoryOption> categories);
    bool ConfirmDelete(ProductDto product);
}
