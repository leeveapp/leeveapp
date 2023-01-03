using Leeve.Core;
using Leeve.Server.Common;

namespace Leeve.Server.Questionnaires;

public sealed class QuestionnaireNotifier : QuestionnaireNotificationService.QuestionnaireNotificationServiceBase
{
    private readonly ILogger _log;
    private readonly QuestionnaireNotification _notifier;

    public QuestionnaireNotifier(ILogger log)
    {
        _log = log;
        _notifier = new QuestionnaireNotification();
    }

    public void Notify(QuestionnaireMessage message)
    {
        _notifier.Change(message);
    }

    public override async Task Subscribe(QuestionnaireNotificationRequest request,
        IServerStreamWriter<QuestionnaireNotificationResponse> responseStream,
        ServerCallContext context)
    {
        try
        {
            await _notifier.GetAsObservable()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async x => await responseStream.WriteAsync(
                    new QuestionnaireNotificationResponse
                    {
                        Action = x.Action,
                        CallerId = x.CallerId,
                        Questionnaire = x.Questionnaire
                    }), context.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _log.Error(e, "{Message}", e.Message);
        }
    }

    private class QuestionnaireNotification : NotifierBase<QuestionnaireMessage>
    {
    }
}