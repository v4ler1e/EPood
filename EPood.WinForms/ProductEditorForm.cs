using EPood.Application.Products.Commands;
using EPood.Application.Products.Dtos;

namespace EPood.WinForms;

internal sealed class ProductEditorForm : Form
{
    private readonly TextBox _name = new() { MaxLength = 100 };
    private readonly TextBox _description = new() { MaxLength = 500 };
    private readonly NumericUpDown _price = new() { DecimalPlaces = 2, Minimum = 0.01m, Maximum = 100_000 };
    private readonly NumericUpDown _stock = new() { Maximum = 10_000 };
    private readonly ComboBox _category = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    public SaveProductCommand? Product { get; private set; }

    public ProductEditorForm(ProductDto? product, List<CategoryOption> categories)
    {
        Text = product == null ? "Add product" : "Edit product";
        ClientSize = new Size(420, 230);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        _name.Text = product?.Name ?? "";
        _description.Text = product?.Description ?? "";
        _price.Value = Math.Clamp(product?.Price ?? 1m, _price.Minimum, _price.Maximum);
        _stock.Value = Math.Clamp(product?.Stock ?? 0, _stock.Minimum, _stock.Maximum);
        _category.DisplayMember = nameof(CategoryOption.Name);
        _category.ValueMember = nameof(CategoryOption.Id);
        _category.DataSource = categories;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddField(layout, "Name", _name);
        AddField(layout, "Description", _description);
        AddField(layout, "Price", _price);
        AddField(layout, "Stock", _stock);
        AddField(layout, "Category", _category);

        var save = new Button { Text = "Save", AutoSize = true };
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
        buttons.Controls.AddRange([cancel, save]);
        layout.Controls.Add(buttons, 0, layout.RowCount++);
        layout.SetColumnSpan(buttons, 2);
        Controls.Add(layout);

        AcceptButton = save;
        CancelButton = cancel;
        save.Click += (_, _) => Save(product?.Id ?? 0);
        if (product != null)
        {
            Load += (_, _) => _category.SelectedValue = product.CategoryId;
        }
    }

    private static void AddField(TableLayoutPanel layout, string label, Control input)
    {
        var row = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        input.Dock = DockStyle.Fill;
        layout.Controls.Add(input, 1, row);
    }

    private void Save(int id)
    {
        var name = _name.Text.Trim();
        if (name.Length == 0 || _category.SelectedItem is not CategoryOption category)
        {
            MessageBox.Show(this, "Enter a product name and choose a category.", Text,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Product = new SaveProductCommand
        {
            Id = id,
            Name = name,
            Description = _description.Text.Trim(),
            Price = _price.Value,
            Stock = (int)_stock.Value,
            CategoryId = category.Id
        };
        DialogResult = DialogResult.OK;
    }
}
