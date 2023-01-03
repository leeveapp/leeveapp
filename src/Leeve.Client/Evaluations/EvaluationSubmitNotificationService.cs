using Leeve.Client.Common;
using Leeve.Core;
using Leeve.Core.Common;
using Leeve.Core.Evaluations;

namespace Leeve.Client.Evaluations;

public interface IEvaluationSubmitNotificationService
{
    Task Subscribe();
}

public sealed class EvaluationSubmitNotificationService : NotificationService<EvaluationSubmitNotificationResponse>,
    IEvaluationSubmitNotificationService
{
    private readonly ChannelManager _manager;
    private readonly IMessenger _messenger;
    private readonly IThreadWrapper _threadWrapper;

    public EvaluationSubmitNotificationService(ChannelManager manager, IMessenger messenger, IThreadWrapper threadWrapper)
    {
        _manager = manager;
        _messenger = messenger;
        _threadWrapper = threadWrapper;
    }

    protected override AsyncServerStreamingCall<EvaluationSubmitNotificationResponse> GetServerCall(CancellationToken token)
    {
        var service = new Core.EvaluationSubmitNotificationService.EvaluationSubmitNotificationServiceClient(_manager.Channel);
        return service.Subscribe(new EvaluationSubmitNotificationRequest(), cancellationToken: token);
    }

    protected override void Respond(EvaluationSubmitNotificationResponse response)
    {
        var entity = EvaluationServiceHub.Search(x => x.Value.Id == response.EvaluationId).FirstOrDefault();
        if (entity == null) return;

        entity.Responses = response.Count;
        EvaluationServiceHub.AddOrUpdate(entity.Id, entity);

        _threadWrapper.Invoke(() => _messenger.Send(new EvaluationSubmittedMessage { Evaluation = entity }));
    }
}