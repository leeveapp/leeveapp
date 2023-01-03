using Leeve.Core;
using Leeve.Server.Common;

namespace Leeve.Server.Users;

public sealed class TeacherNotifier : TeacherNotificationService.TeacherNotificationServiceBase
{
    private readonly ILogger _log;
    private readonly TeacherNotification _notifier;

    public TeacherNotifier(ILogger log)
    {
        _log = log;
        _notifier = new TeacherNotification();
    }

    public void Notify(TeacherMessage message)
    {
        _notifier.Change(message);
    }

    public override async Task Subscribe(TeacherNotificationRequest request,
        IServerStreamWriter<TeacherNotificationResponse> responseStream,
        ServerCallContext context)
    {
        try
        {
            await _notifier.GetAsObservable()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async x => await responseStream.WriteAsync(
                    new TeacherNotificationResponse
                    {
                        Action = x.Action,
                        CallerId = x.CallerId,
                        Teacher = x.Teacher
                    }), context.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _log.Error(e, "{Message}", e.Message);
        }
    }

    private class TeacherNotification : NotifierBase<TeacherMessage>
    {
    }
}