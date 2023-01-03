using Leeve.Core;
using Leeve.Server.Common;

namespace Leeve.Server.Users;

public sealed class TeacherImageNotifier : TeacherImageNotificationService.TeacherImageNotificationServiceBase
{
    private readonly ILogger _log;
    private readonly TeacherImageNotification _notifier;

    public TeacherImageNotifier(ILogger log)
    {
        _log = log;
        _notifier = new TeacherImageNotification();
    }

    public void Notify(TeacherImageMessage message)
    {
        _notifier.Change(message);
    }

    public override async Task Subscribe(TeacherImageNotificationRequest request,
        IServerStreamWriter<TeacherImageNotificationResponse> responseStream,
        ServerCallContext context)
    {
        try
        {
            await _notifier.GetAsObservable()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async x => await responseStream.WriteAsync(
                    new TeacherImageNotificationResponse
                    {
                        TeacherImage = x.TeacherImage,
                        CallerId = x.CallerId
                    }), context.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _log.Error(e, "{Message}", e.Message);
        }
    }

    private class TeacherImageNotification : NotifierBase<TeacherImageMessage>
    {
    }
}