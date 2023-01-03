using Leeve.Client.Common;
using Leeve.Client.Users;
using Leeve.Core;
using Leeve.Core.Common;
using Leeve.Core.Questionnaires;

namespace Leeve.Client.Questionnaires;

public interface IQuestionnaireNotificationService
{
    Task Subscribe();
}

public sealed class QuestionnaireNotificationService : NotificationService<QuestionnaireNotificationResponse>,
    IQuestionnaireNotificationService
{
    private readonly ChannelManager _manager;
    private readonly IMessenger _messenger;
    private readonly IThreadWrapper _threadWrapper;

    public QuestionnaireNotificationService(ChannelManager manager, IMessenger messenger, IThreadWrapper threadWrapper)
    {
        _manager = manager;
        _messenger = messenger;
        _threadWrapper = threadWrapper;
    }

    protected override AsyncServerStreamingCall<QuestionnaireNotificationResponse> GetServerCall(CancellationToken token)
    {
        var service = new Core.QuestionnaireNotificationService.QuestionnaireNotificationServiceClient(_manager.Channel);
        return service.Subscribe(new QuestionnaireNotificationRequest(), cancellationToken: token);
    }

    protected override void Respond(QuestionnaireNotificationResponse response)
    {
        if (response.Questionnaire.TeacherId != TeacherAssist.Id) return;

        var questionnaire = new Questionnaire
        {
            Id = response.Questionnaire.Id,
            Title = response.Questionnaire.Title,
            Description = response.Questionnaire.Description,
            Methodologies = response.Questionnaire.Methodologies.Select(x => x.ToEntity()),
            TeacherId = response.Questionnaire.TeacherId
        };

        if (response.Action == Actions.Add || response.Action == Actions.Update)
            QuestionnaireServiceHub.AddOrUpdate(questionnaire.Id, questionnaire);
        else if (response.Action == Actions.Delete) QuestionnaireServiceHub.Remove(questionnaire.Id);

        _threadWrapper.Invoke(() => _messenger.Send(new QuestionnaireModifiedMessage
        {
            Questionnaire = questionnaire,
            Action = response.Action
        }));
    }
}