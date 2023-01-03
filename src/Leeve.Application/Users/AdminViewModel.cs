using Leeve.Application.Messages;
using Leeve.Client.Users;

namespace Leeve.Application.Users;

public interface IAdminViewModel
{
    void Initialize();
}

public sealed partial class AdminViewModel : ObservableObject, IAdminViewModel
{
    private readonly IMessenger _messenger;
    private readonly IDialog _dialog;
    private readonly IAdminCredentialsViewModel _credentialsViewModel;
    private readonly IAdminClientService _clientService;

    public AdminViewModel(IMessenger messenger,
        IDialog dialog,
        IAdminCredentialsViewModel credentialsViewModel,
        IAdminClientService clientService)
    {
        _messenger = messenger;
        _dialog = dialog;
        _credentialsViewModel = credentialsViewModel;
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

    [RelayCommand]
    private async Task UpdateCredentialsAsync()
    {
        _credentialsViewModel.Initialize();
        await _dialog.CloseAsync(this);
        await _dialog.ShowAsync(_credentialsViewModel);
    }

    [RelayCommand(CanExecute = nameof(CanOk))]
    private async Task Ok()
    {
        var result = await _clientService.LoginAsync(Username, Password);
        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
        }
        else
        {
            _messenger.Send(new AdminLoggedInMessage());
            await _dialog.CloseAsync(this);
        }
    }

    private bool CanOk() =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password);

    [RelayCommand]
    private Task Cancel() => _dialog.CloseAsync(this);

    public void Initialize()
    {
        Username = string.Empty;
        Password = string.Empty;
    }
}