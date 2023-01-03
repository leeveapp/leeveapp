using Leeve.Client.Common;
using Leeve.Core;
using Leeve.Core.Common;
using Leeve.Core.Users;

namespace Leeve.Client.Users;

public interface ITeacherNotificationService
{
    Task Subscribe();
}

public sealed class TeacherNotificationService : NotificationService<TeacherNotificationResponse>, ITeacherNotificationService
{
    private readonly ChannelManager _manager;
    private readonly IMessenger _messenger;
    private readonly IThreadWrapper _threadWrapper;

    public TeacherNotificationService(ChannelManager manager, IMessenger messenger, IThreadWrapper threadWrapper)
    {
        _manager = manager;
        _messenger = messenger;
        _threadWrapper = threadWrapper;
    }

    protected override AsyncServerStreamingCall<TeacherNotificationResponse> GetServerCall(CancellationToken token)
    {
        var service = new Core.TeacherNotificationService.TeacherNotificationServiceClient(_manager.Channel);
        return service.Subscribe(new TeacherNotificationRequest(), cancellationToken: token);
    }

    protected override void Respond(TeacherNotificationResponse response)
    {
        var teacher = new Teacher
        {
            Id = response.Teacher.Id,
            FirstName = response.Teacher.FirstName,
            LastName = response.Teacher.LastName,
            Department = response.Teacher.Department,
            Image = response.Teacher.Image.ToByteArray()
        };

        if (response.Action == Actions.Add || response.Action == Actions.Update)
            TeacherServiceHub.AddOrUpdate(teacher.Id, teacher);
        else if (response.Action == Actions.Delete) TeacherServiceHub.Remove(teacher.Id);

        _threadWrapper.Invoke(() =>
        {
            if (response.Teacher.Id == TeacherAssist.Id) TeacherAssist.Teacher = teacher;
            _messenger.Send(new TeacherModifiedMessage { Teacher = teacher, Action = response.Action });
        });
    }
}