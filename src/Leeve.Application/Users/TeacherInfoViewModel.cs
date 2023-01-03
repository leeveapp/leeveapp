using Leeve.Application.Messages;
using Leeve.Client.Users;
using Leeve.Core.Common;
using Leeve.Core.Users;

namespace Leeve.Application.Users;

public interface ITeacherInfoViewModel
{
}

public sealed partial class TeacherInfoViewModel : ObservableObject, ITeacherInfoViewModel
{
    private readonly IMessenger _messenger;
    private readonly IDialog _dialog;
    private readonly IBrowserDialog _browserDialog;
    private readonly ITeacherClientService _clientService;
    private readonly IEditTeacherViewModel _editTeacherViewModel;
    private readonly ITeacherCredentialsViewModel _teacherCredentialsViewModel;

    public TeacherInfoViewModel(IMessenger messenger,
        IDialog dialog,
        IBrowserDialog browserDialog,
        ITeacherClientService clientService,
        IEditTeacherViewModel editTeacherViewModel,
        ITeacherCredentialsViewModel teacherCredentialsViewModel)
    {
        _messenger = messenger;
        _dialog = dialog;
        _browserDialog = browserDialog;
        _clientService = clientService;
        _editTeacherViewModel = editTeacherViewModel;
        _teacherCredentialsViewModel = teacherCredentialsViewModel;
    }

    [RelayCommand]
    private async Task Logout()
    {
        var dialogResult = await _dialog.ShowAsync("Do you really want to logout?", "Logout",
            button: DialogButton.AffirmativeAndNegative);

        if (dialogResult == DialogResult.Yes)
            _messenger.Send(new UserLoggedOutMessage());
    }

    [RelayCommand]
    private async Task UpdateUserInfoAsync()
    {
        if (TeacherAssist.Teacher == null) return;

        var entity = new Teacher
        {
            Id = TeacherAssist.Id,
            FirstName = TeacherAssist.Teacher.FirstName,
            LastName = TeacherAssist.Teacher.LastName,
            Department = TeacherAssist.Teacher.Department
        };

        _editTeacherViewModel.Initialize(entity, "Update Info", true);
        await _dialog.ShowAsync(_editTeacherViewModel);
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        _teacherCredentialsViewModel.Initialize();
        await _dialog.ShowAsync(_teacherCredentialsViewModel);
    }

    [RelayCommand]
    private async Task ChangePhotoAsync()
    {
        var selected = _browserDialog.BrowseFile("Select your photo", "Image file|*.jpg;*.jpeg;*.png;*.gif;*.bmp");

        if (!selected) return;

        var loading = await _dialog.ShowLoadingAsync();

        try
        {
            var source = _browserDialog.Path;
            var result = await _clientService.UpdateTeacherImageAsync(source, TeacherAssist.Id);
            if (result.IsFaulted) _messenger.Send(new ActionFailedMessage("Failed to update user photo"));
        }
        finally
        {
            await _dialog.CloseLoadingAsync(loading);
        }
    }
}