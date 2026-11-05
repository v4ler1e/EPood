using System.Net.Http.Json;
using EPood.Application.Common;
using EPood.Application.Products.Dtos;

namespace EPood.WinForms;

internal sealed class ProductsForm : Form
{
    private static readonly HttpClient Http = new()
    {
        BaseAddress = new Uri("http://localhost:5298/"),
        Timeout = TimeSpan.FromSeconds(15)
    };

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
    private readonly Label _status = new() { AutoSize = true, Padding = new Padding(4) };

    public ProductsForm()
    {
        Text = "EPood - Products";
        ClientSize = new Size(820, 480);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
        toolbar.Controls.Add(_refresh);
        toolbar.Controls.Add(_status);
        Controls.Add(_products);
        Controls.Add(toolbar);

        Shown += async (_, _) => await LoadProducts();
        _refresh.Click += async (_, _) => await LoadProducts();
    }

    private async Task LoadProducts()
    {
        _refresh.Enabled = false;
        _status.Text = "Loading products...";
        try
        {
            var page = await Http.GetFromJsonAsync<PagedResult<ProductDto>>("api/Products");
            if (!IsDisposed)
            {
                _products.DataSource = page?.Items;
                _status.Text = $"Showing {page?.Items.Count ?? 0} of {page?.TotalCount ?? 0} products.";
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
                _refresh.Enabled = true;
            }
        }
    }
}
