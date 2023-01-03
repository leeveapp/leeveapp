using Leeve.Application.Evaluations;
using Leeve.Application.Messages;

namespace Leeve.Application.Main;

public sealed partial class UserSelectionViewModel : ObservableObject
{
    private readonly IMessenger _messenger;
    private readonly IDialog _dialog;
    private readonly IEvaluationCodeViewModel _evaluationCodeViewModel;

    public UserSelectionViewModel(IMessenger messenger, IDialog dialog,
        IEvaluationCodeViewModel evaluationCodeViewModel)
    {
        _messenger = messenger;
        _dialog = dialog;
        _evaluationCodeViewModel = evaluationCodeViewModel;
    }

    [RelayCommand]
    private void OpenTeacherSelection()
    {
        _messenger.Send(new TeacherSelectionMessage());
    }

    [RelayCommand]
    private async Task EvaluateAsync()
    {
        _evaluationCodeViewModel.Initialize();
        await _dialog.ShowAsync(_evaluationCodeViewModel);
    }
}