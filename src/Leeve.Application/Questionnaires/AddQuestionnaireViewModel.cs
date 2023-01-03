using Leeve.Application.Common;
using Leeve.Application.Messages;
using Leeve.Client.Questionnaires;

namespace Leeve.Application.Questionnaires;

public interface IAddQuestionnaireViewModel : IInitializable
{
}

public sealed class AddQuestionnaireViewModel : QuestionnaireViewModel, IAddQuestionnaireViewModel
{
    private readonly IMessenger _messenger;
    private readonly IQuestionnaireClientService _clientService;

    public AddQuestionnaireViewModel(IMessenger messenger,
        IQuestionnaireClientService clientService) : base(messenger)
    {
        _messenger = messenger;
        _clientService = clientService;
    }

    protected override async Task SaveAsync()
    {
        var entity = this.ToEntity();
        var result = await _clientService.AddAsync(entity);

        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
        }
        else
        {
            var message = $"Questionnaire [{entity.Title}] has been added successfully";
            _messenger.Send(new NotificationMessage(message));

            _messenger.Send(new RemoveLastPageMessage());
        }
    }

    public Task<bool> InitializeAsync()
    {
        Title = string.Empty;
        Description = string.Empty;
        Methodologies.Clear();

        var methodology = new MethodologyViewModel(AddCommand, DeleteCommand);
        methodology.Initialize();
        Methodologies.Add(methodology);

        return Task.FromResult(true);
    }
}