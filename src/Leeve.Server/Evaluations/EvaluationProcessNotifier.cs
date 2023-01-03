using Leeve.Core;
using Leeve.Server.Common;

namespace Leeve.Server.Evaluations;

public sealed class EvaluationProcessNotifier : EvaluationProcessNotificationService.EvaluationProcessNotificationServiceBase
{
    private readonly ILogger _log;
    private readonly EvaluationProcessNotification _notifier;

    public EvaluationProcessNotifier(ILogger log)
    {
        _log = log;
        _notifier = new EvaluationProcessNotification();
    }

    public void Notify(EvaluationProcessMessage message)
    {
        _notifier.Change(message);
    }

    public override async Task Subscribe(EvaluationProcessNotificationRequest request,
        IServerStreamWriter<EvaluationProcessNotificationResponse> responseStream,
        ServerCallContext context)
    {
        try
        {
            await _notifier.GetAsObservable()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async x => await responseStream.WriteAsync(
                    new EvaluationProcessNotificationResponse
                    {
                        Action = x.Action,
                        CallerId = x.CallerId,
                        EvaluationId = x.EvaluationId,
                        EvaluationCode = x.EvaluationCode
                    }), context.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _log.Error(e, "{Message}", e.Message);
        }
    }

    private class EvaluationProcessNotification : NotifierBase<EvaluationProcessMessage>
    {
    }
}

public sealed class EvaluationSubmitNotifier : EvaluationSubmitNotificationService.EvaluationSubmitNotificationServiceBase
{
    private readonly ILogger _log;
    private readonly EvaluationSubmitNotification _notifier;

    public EvaluationSubmitNotifier(ILogger log)
    {
        _log = log;
        _notifier = new EvaluationSubmitNotification();
    }

    public void Notify(EvaluationSubmitMessage message)
    {
        _notifier.Change(message);
    }

    public override async Task Subscribe(EvaluationSubmitNotificationRequest request,
        IServerStreamWriter<EvaluationSubmitNotificationResponse> responseStream,
        ServerCallContext context)
    {
        try
        {
            await _notifier.GetAsObservable()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async x => await responseStream.WriteAsync(
                    new EvaluationSubmitNotificationResponse
                    {
                        Count = x.Count,
                        CallerId = x.CallerId,
                        EvaluationId = x.EvaluationId
                    }), context.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _log.Error(e, "{Message}", e.Message);
        }
    }

    private class EvaluationSubmitNotification : NotifierBase<EvaluationSubmitMessage>
    {
    }
}