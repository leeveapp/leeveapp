using Leeve.Application.Messages;
using Leeve.Client.Users;

namespace Leeve.Application.Users;

public interface ITeacherCredentialsViewModel
{
    void Initialize();
}

public sealed partial class TeacherCredentialsViewModel : ObservableValidator, ITeacherCredentialsViewModel
{
    private readonly IDialog _dialog;
    private readonly IMessenger _messenger;
    private readonly ITeacherClientService _clientService;

    public TeacherCredentialsViewModel(IDialog dialog, IMessenger messenger, ITeacherClientService clientService)
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
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [RelayCommand(CanExecute = nameof(CanOk))]
    private async Task Ok()
    {
        var updateResult = await _clientService.UpdateCredentialsAsync(Username, Password, CurrentPassword);
        if (updateResult.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(updateResult.ToString()));
        }
        else
        {
            _messenger.Send(new NotificationMessage("Your credentials has been update successfully"));
            await _dialog.CloseAsync(this);
        }
    }

    private bool CanOk() =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(CurrentPassword) &&
        Password == ConfirmPassword;

    [RelayCommand]
    private Task Cancel() => _dialog.CloseAsync(this);

    public void Initialize()
    {
        Username = string.Empty;
        Password = string.Empty;
        CurrentPassword = string.Empty;
        ConfirmPassword = string.Empty;
    }
}