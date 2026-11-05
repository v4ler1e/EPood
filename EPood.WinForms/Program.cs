namespace EPood.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5298/"),
            Timeout = TimeSpan.FromSeconds(15)
        };
        System.Windows.Forms.Application.Run(new ProductsForm(new ProductApiClient(http)));
    }
}
