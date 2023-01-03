using Leeve.Client.Common;
using Leeve.Client.Users;
using Leeve.Core;
using Leeve.Core.Common;
using Leeve.Core.Evaluations;

namespace Leeve.Client.Evaluations;

public interface IEvaluationNotificationService
{
    Task Subscribe();
}

public sealed class EvaluationNotificationService : NotificationService<EvaluationNotificationResponse>, IEvaluationNotificationService
{
    private readonly ChannelManager _manager;
    private readonly IMessenger _messenger;
    private readonly IThreadWrapper _threadWrapper;

    public EvaluationNotificationService(ChannelManager manager, IMessenger messenger, IThreadWrapper threadWrapper)
    {
        _manager = manager;
        _messenger = messenger;
        _threadWrapper = threadWrapper;
    }

    protected override AsyncServerStreamingCall<EvaluationNotificationResponse> GetServerCall(CancellationToken token)
    {
        var service = new Core.EvaluationNotificationService.EvaluationNotificationServiceClient(_manager.Channel);
        return service.Subscribe(new EvaluationNotificationRequest(), cancellationToken: token);
    }

    protected override void Respond(EvaluationNotificationResponse response)
    {
        if (response.Evaluation.TeacherId != TeacherAssist.Id) return;

        var entity = response.Evaluation.ToEntity();

        if (response.Action == Actions.Add || response.Action == Actions.Update)
            EvaluationServiceHub.AddOrUpdate(entity.Id, entity);
        else if (response.Action == Actions.Delete) EvaluationServiceHub.Remove(entity.Id);

        _threadWrapper.Invoke(() => _messenger.Send(new EvaluationModifiedMessage
        {
            Evaluation = entity,
            Action = response.Action
        }));
    }
}