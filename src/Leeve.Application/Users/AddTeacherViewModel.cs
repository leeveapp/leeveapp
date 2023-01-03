using Leeve.Application.Messages;
using Leeve.Client.Users;
using Leeve.Core.Users;

namespace Leeve.Application.Users;

public interface IAddTeacherViewModel
{
    void Initialize();
}

public sealed class AddTeacherViewModel : TeacherViewModel, IAddTeacherViewModel
{
    private readonly IDialog _dialog;
    private readonly IMessenger _messenger;
    private readonly ITeacherClientService _clientService;

    public AddTeacherViewModel(IDialog dialog, IMessenger messenger, ITeacherClientService clientService) : base(dialog)
    {
        _dialog = dialog;
        _messenger = messenger;
        _clientService = clientService;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

    protected override async Task SaveAsync()
    {
        var entity = new Teacher
        {
            FirstName = FirstName,
            LastName = LastName,
            Department = Department
        };

        var result = await _clientService.AddAsync(entity);

        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
            return;
        }

        _messenger.Send(new NotificationMessage($"Teacher [{entity.FullName}] added successfully"));
        await _dialog.CloseAsync(this);
    }

    public void Initialize()
    {
        Title = "Add New Teacher";
        FirstName = string.Empty;
        LastName = string.Empty;
        Department = string.Empty;
    }
}