using Leeve.Core;
using Leeve.Core.Common;

namespace Leeve.Server.Users;

public sealed class TeacherService : Core.TeacherService.TeacherServiceBase
{
    private readonly ILogger _log;
    private readonly TeacherNotifier _notifier;

    public TeacherService(ILogger log, TeacherNotifier notifier)
    {
        _log = log;
        _notifier = notifier;
    }

    public override async Task<AddTeacherResponse> Add(AddTeacherRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new TeacherRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.AddAsync(request.Teacher, request.AdminId, token: token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            _notifier.Notify(new TeacherMessage
            {
                Action = Actions.Add,
                CallerId = request.CallerId,
                Teacher = result.Value
            });

            return new AddTeacherResponse { Teacher = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new AddTeacherResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<GetTeacherResponse> Get(GetTeacherRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new TeacherRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.GetAsync(request.Id, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            return new GetTeacherResponse { Teacher = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new GetTeacherResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<GetTeacherByCredentialsResponse> GetByCredentials(GetTeacherByCredentialsRequest request,
        ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new TeacherRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.GetByCredentialsAsync(request.UserName, request.Password, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.PermissionDenied, result.ToString()));

            return new GetTeacherByCredentialsResponse { Teacher = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new GetTeacherByCredentialsResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task GetAll(GetAllTeachersRequest request, IServerStreamWriter<GetAllTeachersResponse> responseStream,
        ServerCallContext context)
    {
        try
        {
            var token = context.CancellationToken;
            var dbContext = DbFactory.Get();
            var repo = new TeacherRepository(dbContext, _log);

            var result = await repo.GetAllAsync(token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            if (token.IsCancellationRequested) return;

            var departments = result.Value;
            foreach (var dto in departments)
            {
                if (token.IsCancellationRequested) return;
                await responseStream.WriteAsync(new GetAllTeachersResponse { Teacher = dto }, token);
            }
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<UpdateTeacherResponse> Update(UpdateTeacherRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new TeacherRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.UpdateAsync(request.Teacher, request.Password, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            _notifier.Notify(new TeacherMessage
            {
                Action = Actions.Update,
                CallerId = request.CallerId,
                Teacher = result.Value
            });

            return new UpdateTeacherResponse { Teacher = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new UpdateTeacherResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<UpdateTeacherByAdminResponse> UpdateByAdmin(UpdateTeacherByAdminRequest request,
        ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new TeacherRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.UpdateByAdminAsync(request.Teacher, request.AdminId, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            _notifier.Notify(new TeacherMessage
            {
                Action = Actions.Update,
                CallerId = request.CallerId,
                Teacher = result.Value
            });

            return new UpdateTeacherByAdminResponse { Teacher = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new UpdateTeacherByAdminResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<UpdateTeacherCredentialsResponse> UpdateCredentials(UpdateTeacherCredentialsRequest request,
        ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new TeacherRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.UpdateCredentialsAsync(request.Credential, request.TeacherId, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            return new UpdateTeacherCredentialsResponse { Teacher = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new UpdateTeacherCredentialsResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<DeleteTeacherResponse> Delete(DeleteTeacherRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new TeacherRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.DeleteAsync(request.TeacherId, request.AdminId, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            _notifier.Notify(new TeacherMessage
            {
                Action = Actions.Delete,
                CallerId = request.CallerId,
                Teacher = result.Value
            });

            return new DeleteTeacherResponse { Teacher = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new DeleteTeacherResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }
}