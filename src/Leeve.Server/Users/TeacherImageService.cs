using Leeve.Core;

namespace Leeve.Server.Users;

public sealed class TeacherImageService : Core.TeacherImageService.TeacherImageServiceBase
{
    private readonly ILogger _log;
    private readonly TeacherImageNotifier _notifier;

    public TeacherImageService(ILogger log, TeacherImageNotifier notifier)
    {
        _log = log;
        _notifier = notifier;
    }

    public override async Task<UpdateTeacherImageResponse> UpdateTeacherImage(UpdateTeacherImageRequest request,
        ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new TeacherRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.UpdateImageAsync(request.TeacherImage, request.UserId, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            _notifier.Notify(new TeacherImageMessage
            {
                CallerId = request.CallerId,
                TeacherImage = result
            });

            return new UpdateTeacherImageResponse { TeacherImage = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new UpdateTeacherImageResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }
}