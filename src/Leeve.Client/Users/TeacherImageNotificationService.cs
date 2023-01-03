using Leeve.Client.Common;
using Leeve.Core;
using Leeve.Core.Common;

namespace Leeve.Client.Users;

public interface ITeacherImageNotificationService
{
    Task Subscribe();
}

public sealed class TeacherImageNotificationService : NotificationService<TeacherImageNotificationResponse>,
    ITeacherImageNotificationService
{
    private readonly ChannelManager _manager;
    private readonly IThreadWrapper _threadWrapper;

    public TeacherImageNotificationService(ChannelManager manager, IThreadWrapper threadWrapper)
    {
        _manager = manager;
        _threadWrapper = threadWrapper;
    }

    protected override AsyncServerStreamingCall<TeacherImageNotificationResponse> GetServerCall(CancellationToken token)
    {
        var service = new Core.TeacherImageNotificationService.TeacherImageNotificationServiceClient(_manager.Channel);
        return service.Subscribe(new TeacherImageNotificationRequest(), cancellationToken: token);
    }

    protected override void Respond(TeacherImageNotificationResponse response)
    {
        var image = response.TeacherImage.Image.ToByteArray();

        // update image in user hub
        var exist = TeacherServiceHub.TryGet(response.TeacherImage.Id, out var existing);
        if (exist && existing != null)
        {
            existing.Image = image;
            TeacherServiceHub.AddOrUpdate(existing.Id, existing);
        }

        _threadWrapper.Invoke(() =>
        {
            if (response.TeacherImage.Id == TeacherAssist.Id)
                TeacherAssist.UpdateImage(image);
        });
    }
}