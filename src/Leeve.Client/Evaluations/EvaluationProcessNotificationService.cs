using Leeve.Client.Common;
using Leeve.Core;
using Leeve.Core.Common;
using Leeve.Core.Evaluations;

namespace Leeve.Client.Evaluations;

public interface IEvaluationProcessNotificationService
{
    Task Subscribe();
}

public sealed class EvaluationProcessNotificationService : NotificationService<EvaluationProcessNotificationResponse>,
    IEvaluationProcessNotificationService
{
    private readonly ChannelManager _manager;
    private readonly IMessenger _messenger;
    private readonly IThreadWrapper _threadWrapper;

    public EvaluationProcessNotificationService(ChannelManager manager, IMessenger messenger, IThreadWrapper threadWrapper)
    {
        _manager = manager;
        _messenger = messenger;
        _threadWrapper = threadWrapper;
    }

    protected override AsyncServerStreamingCall<EvaluationProcessNotificationResponse> GetServerCall(CancellationToken token)
    {
        var service = new Core.EvaluationProcessNotificationService.EvaluationProcessNotificationServiceClient(_manager.Channel);
        return service.Subscribe(new EvaluationProcessNotificationRequest(), cancellationToken: token);
    }

    protected override void Respond(EvaluationProcessNotificationResponse response)
    {
        var entity = EvaluationServiceHub.Search(x => x.Value.Id == response.EvaluationId).FirstOrDefault();
        if (entity == null) return;

        if (response.Action == Actions.Add)
        {
            entity.IsActive = true;
            entity.Code = response.EvaluationCode;
            EvaluationServiceHub.AddOrUpdate(entity.Id, entity);
        }
        else if (response.Action == Actions.Delete)
        {
            entity.IsActive = false;
            entity.Code = string.Empty;
            EvaluationServiceHub.AddOrUpdate(entity.Id, entity);
        }

        _threadWrapper.Invoke(() =>
        {
            if (response.Action == Actions.Add)
                _messenger.Send(new EvaluationProcessStartedMessage { Evaluation = entity });
            else if (response.Action == Actions.Delete)
                _messenger.Send(new EvaluationProcessStoppedMessage { Evaluation = entity });
        });
    }
}