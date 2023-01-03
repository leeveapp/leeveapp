using Leeve.Application.Messages;
using Leeve.Client.Users;

namespace Leeve.Application.Users;

public interface IAdminCredentialsViewModel
{
    void Initialize();
}

public sealed partial class AdminCredentialsViewModel : ObservableValidator, IAdminCredentialsViewModel
{
    private readonly IDialog _dialog;
    private readonly IMessenger _messenger;
    private readonly IAdminClientService _clientService;

    public AdminCredentialsViewModel(IDialog dialog, IMessenger messenger, IAdminClientService clientService)
    {
        _dialog = dialog;
        _messenger = messenger;
        _clientService = clientService;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OkCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    private string _currentUsername = string.Empty;

    [ObservableProperty]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    private string _newUsername = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [RelayCommand(CanExecute = nameof(CanOk))]
    private async Task Ok()
    {
        var loginResult = await _clientService.LoginAsync(CurrentUsername, CurrentPassword);
        if (loginResult.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(loginResult.ToString()));
            return;
        }

        var updateResult = await _clientService.UpdateCredentialsAsync(NewUsername, NewPassword, CurrentPassword);
        if (updateResult.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(updateResult.ToString()));
        }
        else
        {
            _messenger.Send(new NotificationMessage("Admin credentials updated successfully"));
            await _dialog.CloseAsync(this);
        }
    }

    private bool CanOk() =>
        !string.IsNullOrWhiteSpace(CurrentUsername) &&
        !string.IsNullOrWhiteSpace(CurrentPassword) &&
        !string.IsNullOrWhiteSpace(NewUsername) &&
        !string.IsNullOrWhiteSpace(NewPassword) &&
        !string.IsNullOrWhiteSpace(NewPassword) &&
        NewPassword == ConfirmPassword;

    [RelayCommand]
    private Task Cancel() => _dialog.CloseAsync(this);

    public void Initialize()
    {
        CurrentUsername = string.Empty;
        CurrentPassword = string.Empty;
        NewUsername = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
    }
}