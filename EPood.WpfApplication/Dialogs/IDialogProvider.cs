namespace EPood.WpfApplication.Dialogs;

public interface IDialogProvider
{
    void ShowMessage(string message);

    void ShowError(string message);

    bool Confirm(string message);
}