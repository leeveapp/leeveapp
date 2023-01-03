using Leeve.Application.Main;
using Leeve.Application.Messages;
using Leeve.Application.Questionnaires;
using Leeve.Client.Evaluations;
using Leeve.Core.Evaluations;

namespace Leeve.Application.Evaluations;

public interface IEvaluationViewModel
{
    void Initialize(Evaluation entity);
}

public sealed partial class EvaluationViewModel : ObservableObject, IEvaluationViewModel
{
    private readonly IMessenger _messenger;
    private readonly IEvaluationClientService _clientService;

    public EvaluationViewModel(IMessenger messenger, IEvaluationClientService clientService)
    {
        _messenger = messenger;
        _clientService = clientService;
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _questionnaireTitle = string.Empty;

    [ObservableProperty]
    private string _questionnaireDescription = string.Empty;

    public IEnumerable<MethodologyViewModel> Methodologies { get; private set; } = null!;

    [RelayCommand]
    private async Task SubmitAsync()
    {
        var questionnaire = _entity.Questionnaire;
        questionnaire.Methodologies = Methodologies.Select(m => m.ToEntity()).ToList();

        var entity = new Evaluation
        {
            Id = _entity.Id,
            Title = _entity.Title,
            Description = _entity.Description,
            TeacherId = _entity.TeacherId,
            Questionnaire = questionnaire
        };

        var result = await _clientService.SubmitEvaluationAsync(entity);
        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
            return;
        }

        _messenger.Send(new NotificationMessage("Evaluation submitted successfully"));
        _messenger.Send(new UserSelectionMessage());
    }

    [RelayCommand]
    private void Cancel()
    {
        _messenger.Send(new UserSelectionMessage());
    }

    private Evaluation _entity = null!;

    public void Initialize(Evaluation entity)
    {
        _entity = entity;

        Title = entity.Title;
        Description = entity.Description;
        QuestionnaireTitle = entity.Questionnaire.Title;
        QuestionnaireDescription = entity.Questionnaire.Description;
        Methodologies = entity.Questionnaire.Methodologies.Select(x => x.ToViewModel()).ToList();
    }
}