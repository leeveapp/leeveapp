using Leeve.Application.Main;
using Leeve.Application.Messages;
using Leeve.Client.Users;

namespace Leeve.Application.Users;

public interface ILoginPageViewModel
{
    void Initialize();
}

public sealed partial class TeacherLoginPageViewModel : ObservableObject, ILoginPageViewModel
{
    private readonly IMessenger _messenger;
    private readonly ITeacherClientService _clientService;

    public TeacherLoginPageViewModel(IMessenger messenger, ITeacherClientService clientService)
    {
        _messenger = messenger;
        _clientService = clientService;

        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        LoginCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        var result = await _clientService.LoginAsync(Username, Password);
        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
        }
        else
        {
            TeacherAssist.Teacher = result;
            _messenger.Send(new TeacherLoggedInMessage());
        }
    }

    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password);

    [RelayCommand]
    private void Back()
    {
        _messenger.Send(new UserSelectionMessage());
    }

    public void Initialize()
    {
        Username = string.Empty;
        Password = string.Empty;
    }
}