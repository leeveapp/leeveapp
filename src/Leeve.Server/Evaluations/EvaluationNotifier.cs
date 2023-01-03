using Leeve.Core;
using Leeve.Server.Common;

namespace Leeve.Server.Evaluations;

public sealed class EvaluationNotifier : EvaluationNotificationService.EvaluationNotificationServiceBase
{
    private readonly ILogger _log;
    private readonly EvaluationNotification _notifier;

    public EvaluationNotifier(ILogger log)
    {
        _log = log;
        _notifier = new EvaluationNotification();
    }

    public void Notify(EvaluationMessage message)
    {
        _notifier.Change(message);
    }

    public override async Task Subscribe(EvaluationNotificationRequest request,
        IServerStreamWriter<EvaluationNotificationResponse> responseStream,
        ServerCallContext context)
    {
        try
        {
            await _notifier.GetAsObservable()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async x => await responseStream.WriteAsync(
                    new EvaluationNotificationResponse
                    {
                        Action = x.Action,
                        CallerId = x.CallerId,
                        Evaluation = x.Evaluation
                    }), context.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _log.Error(e, "{Message}", e.Message);
        }
    }

    private class EvaluationNotification : NotifierBase<EvaluationMessage>
    {
    }
}