using Leeve.Application.Messages;
using Leeve.Client.Evaluations;
using Leeve.Core.Evaluations;

namespace Leeve.Application.Evaluations;

public interface IEvaluationCodeViewModel
{
    void Initialize();
}

public sealed partial class EvaluationCodeViewModel : ObservableObject, IEvaluationCodeViewModel
{
    private readonly IMessenger _messenger;
    private readonly IDialog _dialog;
    private readonly IEvaluationClientService _clientService;

    public EvaluationCodeViewModel(IMessenger messenger, IDialog dialog, IEvaluationClientService clientService)
    {
        _messenger = messenger;
        _dialog = dialog;
        _clientService = clientService;
        PropertyChanged += (_, _) => RequestCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    private string _code = string.Empty;

    [RelayCommand(CanExecute = nameof(CanRequest))]
    private async Task Request()
    {
        var result = await _clientService.RequestEvaluationAsync(Code);

        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
            return;
        }

        _messenger.Send(new EvaluationRequestedMessage { Evaluation = result });
        await _dialog.CloseAsync(this);
    }

    private bool CanRequest() => !string.IsNullOrWhiteSpace(Code);

    [RelayCommand]
    private Task Cancel() => _dialog.CloseAsync(this);

    public void Initialize()
    {
        Code = string.Empty;
    }
}