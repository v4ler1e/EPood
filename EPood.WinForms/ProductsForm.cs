namespace EPood.WinForms;

internal sealed class ProductsForm : Form
{
    private readonly ProductApiClient _api;
    private int _page = 1;
    private int _totalPages;
    private string _activeSearch = "";

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
    private readonly Button _refresh = new() { Text = "Refresh", AutoSize = true };
    private readonly TextBox _search = new() { Width = 160, PlaceholderText = "Search products" };
    private readonly Button _find = new() { Text = "Search", AutoSize = true };
    private readonly Button _previous = new() { Text = "Previous", AutoSize = true, Enabled = false };
    private readonly Button _next = new() { Text = "Next", AutoSize = true, Enabled = false };
    private readonly Label _status = new() { AutoSize = true, Padding = new Padding(4) };

    public ProductsForm(ProductApiClient api)
    {
        _api = api;
        Text = "EPood - Products";
        ClientSize = new Size(820, 480);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
        toolbar.Controls.AddRange([_search, _find, _refresh, _previous, _next, _status]);
        Controls.Add(_products);
        Controls.Add(toolbar);

        Shown += async (_, _) => await LoadProducts(1, "");
        _find.Click += async (_, _) => await LoadProducts(1, _search.Text);
        _refresh.Click += async (_, _) => await LoadProducts(_page, _activeSearch);
        _previous.Click += async (_, _) => await LoadProducts(_page - 1, _activeSearch);
        _next.Click += async (_, _) => await LoadProducts(_page + 1, _activeSearch);
    }

    private async Task LoadProducts(int page, string search)
    {
        _find.Enabled = false;
        _refresh.Enabled = false;
        _previous.Enabled = false;
        _next.Enabled = false;
        _status.Text = "Loading products...";
        try
        {
            var result = await _api.GetProductsAsync(page, search);
            if (page > Math.Max(1, result.TotalPages))
            {
                result = await _api.GetProductsAsync(Math.Max(1, result.TotalPages), search);
            }

            if (!IsDisposed)
            {
                _page = result.Page;
                _totalPages = result.TotalPages;
                _activeSearch = search;
                _products.DataSource = result.Items;
                _status.Text = $"Page {_page} of {Math.Max(1, _totalPages)} · {result.TotalCount} products";
            }
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                _status.Text = $"Could not load products. {ex.Message}";
            }
        }
        finally
        {
            if (!IsDisposed)
            {
                _find.Enabled = true;
                _refresh.Enabled = true;
                _previous.Enabled = _page > 1;
                _next.Enabled = _page < _totalPages;
            }
        }
    }
}
