using Leeve.Application.Messages;
using Leeve.Client.Evaluations;
using Leeve.Core.Evaluations;
using Leeve.Core.Questionnaires;

namespace Leeve.Application.Questionnaires;

public interface IAddEvaluationViewModel
{
    void Initialize(Questionnaire entity);
}

public sealed partial class AddEvaluationViewModel : ObservableObject, IAddEvaluationViewModel
{
    private readonly IMessenger _messenger;
    private readonly IDialog _dialog;
    private readonly IEvaluationClientService _clientService;

    public AddEvaluationViewModel(IMessenger messenger,
        IDialog dialog,
        IEvaluationClientService clientService)
    {
        _messenger = messenger;
        _dialog = dialog;
        _clientService = clientService;

        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        CreateCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _questionnaireTitle = string.Empty;

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private async Task Create()
    {
        var entity = new Evaluation
        {
            Title = Title,
            Description = Description,
            Questionnaire = _entity,
            CreatedOn = DateTime.Now
        };

        var result = await _clientService.AddAsync(entity);
        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
        }
        else
        {
            var message = $"Evaluation [{entity.Title}] has been added successfully";
            _messenger.Send(new NotificationMessage(message));
            await _dialog.CloseAsync(this);
        }
    }

    private bool CanAdd() => !string.IsNullOrWhiteSpace(Title);

    [RelayCommand]
    private void Cancel() => _dialog.CloseAsync(this);

    private Questionnaire _entity = null!;

    public void Initialize(Questionnaire entity)
    {
        _entity = entity;

        Title = string.Empty;
        Description = string.Empty;
        QuestionnaireTitle = entity.Title;
    }
}