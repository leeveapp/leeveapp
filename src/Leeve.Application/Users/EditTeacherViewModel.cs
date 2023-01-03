using Leeve.Application.Messages;
using Leeve.Client.Users;
using Leeve.Core.Common;
using Leeve.Core.Users;

namespace Leeve.Application.Users;

public interface IEditTeacherViewModel
{
    void Initialize(Teacher entity, string title = "Edit Teacher", bool usingPassword = false);
}

public sealed class EditTeacherViewModel : TeacherViewModel, IEditTeacherViewModel
{
    private readonly IDialog _dialog;
    private readonly IMessenger _messenger;
    private readonly ITeacherClientService _clientService;

    public EditTeacherViewModel(IDialog dialog, IMessenger messenger, ITeacherClientService clientService) : base(dialog)
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
            Id = _id,
            FirstName = FirstName,
            LastName = LastName,
            Department = Department
        };

        Result result;

        if (UsingPassword)
            result = await _clientService.UpdateAsync(entity, Password);
        else
            result = await _clientService.UpdateByAdminAsync(entity);

        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
            return;
        }

        _messenger.Send(new NotificationMessage($"Teacher [{entity.FullName}] updated successfully"));
        await _dialog.CloseAsync(this);
    }

    private string _id = string.Empty;

    public void Initialize(Teacher entity, string title = "Edit Teacher", bool usingPassword = false)
    {
        _id = entity.Id;
        Title = title;
        FirstName = entity.FirstName;
        LastName = entity.LastName;
        Department = entity.Department;
        UsingPassword = usingPassword;
        Password = string.Empty;
    }
}