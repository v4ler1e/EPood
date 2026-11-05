using EPood.Application.Common;
using EPood.Application.Products.Commands;
using EPood.Application.Products.Dtos;

namespace EPood.WinForms;

internal sealed class ProductsForm : Form, IProductsView
{
    private readonly DataGridView _products = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };
    private readonly TextBox _search = new() { Width = 160, PlaceholderText = "Search products" };
    private readonly Button _find = new() { Text = "Search", AutoSize = true };
    private readonly Button _refresh = new() { Text = "Refresh", AutoSize = true };
    private readonly Button _previous = new() { Text = "Previous", AutoSize = true, Enabled = false };
    private readonly Button _next = new() { Text = "Next", AutoSize = true, Enabled = false };
    private readonly Button _add = new() { Text = "Add", AutoSize = true };
    private readonly Button _edit = new() { Text = "Edit", AutoSize = true };
    private readonly Button _delete = new() { Text = "Delete", AutoSize = true };
    private readonly ToolStripStatusLabel _status = new();

    public event EventHandler? RefreshRequested;
    public event EventHandler? SearchRequested;
    public event EventHandler? PreviousPageRequested;
    public event EventHandler? NextPageRequested;
    public event EventHandler? AddRequested;
    public event EventHandler? EditRequested;
    public event EventHandler? DeleteRequested;

    public string SearchText => _search.Text;
    public ProductDto? SelectedProduct => _products.CurrentRow?.DataBoundItem as ProductDto;

    public ProductsForm()
    {
        Text = "EPood - Products";
        ClientSize = new Size(920, 500);
        MinimumSize = new Size(650, 380);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
        toolbar.Controls.AddRange([_search, _find, _refresh, _previous, _next, _add, _edit, _delete]);
        var statusBar = new StatusStrip();
        statusBar.Items.Add(_status);
        Controls.Add(_products);
        Controls.Add(toolbar);
        Controls.Add(statusBar);

        Shown += (_, _) => RefreshRequested?.Invoke(this, EventArgs.Empty);
        _refresh.Click += (_, _) => RefreshRequested?.Invoke(this, EventArgs.Empty);
        _find.Click += (_, _) => SearchRequested?.Invoke(this, EventArgs.Empty);
        _previous.Click += (_, _) => PreviousPageRequested?.Invoke(this, EventArgs.Empty);
        _next.Click += (_, _) => NextPageRequested?.Invoke(this, EventArgs.Empty);
        _add.Click += (_, _) => AddRequested?.Invoke(this, EventArgs.Empty);
        _edit.Click += (_, _) => EditRequested?.Invoke(this, EventArgs.Empty);
        _delete.Click += (_, _) => DeleteRequested?.Invoke(this, EventArgs.Empty);
        AcceptButton = _find;
    }

    public void SetBusy(bool busy, bool hasPrevious, bool hasNext)
    {
        _search.Enabled = !busy;
        _find.Enabled = !busy;
        _refresh.Enabled = !busy;
        _previous.Enabled = !busy && hasPrevious;
        _next.Enabled = !busy && hasNext;
        _add.Enabled = !busy;
        _edit.Enabled = !busy;
        _delete.Enabled = !busy;
    }

    public void ShowProducts(PagedResult<ProductDto> products)
    {
        _products.DataSource = products.Items;
        ShowStatus($"Page {products.Page} of {Math.Max(1, products.TotalPages)} · {products.TotalCount} products");
    }

    public void ShowStatus(string message) => _status.Text = message;

    public SaveProductCommand? EditProduct(ProductDto? product, List<CategoryOption> categories)
    {
        using var editor = new ProductEditorForm(product, categories);
        return editor.ShowDialog(this) == DialogResult.OK ? editor.Product : null;
    }

    public bool ConfirmDelete(ProductDto product)
    {
        return MessageBox.Show(this,
            $"Delete {product.Name}? Orders linked to this product will also be deleted.",
            "Delete product", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes;
    }
}
