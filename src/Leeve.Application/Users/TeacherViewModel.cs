using System.ComponentModel.DataAnnotations;

namespace Leeve.Application.Users;

public abstract partial class TeacherViewModel : ObservableValidator
{
    private readonly IDialog _dialog;

    protected TeacherViewModel(IDialog dialog)
    {
        _dialog = dialog;
        PropertyChanged += (_, _) => SaveCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Required field")]
    private string _firstName = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Required field")]
    private string _lastName = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Required field")]
    private string _department = string.Empty;

    [ObservableProperty]
    private bool _usingPassword;

    [ObservableProperty]
    private string _password = string.Empty;

    [RelayCommand(CanExecute = nameof(CanSave))]
    protected abstract Task SaveAsync();

    private bool CanSave()
    {
        if (UsingPassword && string.IsNullOrWhiteSpace(Password)) return false;
        return !HasErrors;
    }

    [RelayCommand]
    private Task CancelAsync() => _dialog.CloseAsync(this);
}