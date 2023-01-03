using Leeve.Application.Common;
using Leeve.Application.Messages;
using Leeve.Client.Questionnaires;
using Leeve.Core.Questionnaires;

namespace Leeve.Application.Questionnaires;

public interface IEditQuestionnaireViewModel : IInitializableForEdit
{
}

public sealed class EditQuestionnaireViewModel : QuestionnaireViewModel, IEditQuestionnaireViewModel
{
    private readonly IMessenger _messenger;
    private readonly IQuestionnaireClientService _clientService;

    public EditQuestionnaireViewModel(IMessenger messenger,
        IQuestionnaireClientService clientService) : base(messenger)
    {
        _messenger = messenger;
        _clientService = clientService;
    }

    protected override async Task SaveAsync()
    {
        var entity = this.ToEntity();
        entity.Id = _entity.Id;
        var result = await _clientService.UpdateAsync(entity);

        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
        }
        else
        {
            var message = $"Questionnaire [{entity.Title}] has been updated successfully";
            _messenger.Send(new NotificationMessage(message));

            _messenger.Send(new RemoveLastPageMessage());
        }
    }

    private Questionnaire _entity = null!;

    public Task<bool> InitializeAsync(object entity)
    {
        if (entity is not Questionnaire questionnaire) return Task.FromResult(false);
        _entity = questionnaire;

        Title = questionnaire.Title;
        Description = questionnaire.Description;

        Methodologies.Clear();
        var methodologies = questionnaire.Methodologies.Select(x => x.ToViewModel(AddCommand, DeleteCommand));
        foreach (var methodology in methodologies) Methodologies.Add(methodology);

        return Task.FromResult(true);
    }
}