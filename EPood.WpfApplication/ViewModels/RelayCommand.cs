using System.Windows.Input;

namespace EPood.WpfApplication.ViewModels;

public class RelayCommand : ICommand
{
    private readonly Func<Task> _execute;

    public RelayCommand(Func<Task> execute)
    {
        _execute = execute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public async void Execute(object? parameter)
    {
        await _execute();
    }
}