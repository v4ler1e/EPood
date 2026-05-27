using EPood.WpfApplication.ViewModels;
using System.Windows;

namespace EPood.WpfApplication;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();
    }
}