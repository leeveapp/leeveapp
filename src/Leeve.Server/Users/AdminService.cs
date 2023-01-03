using Leeve.Core;

namespace Leeve.Server.Users;

public sealed class AdminService : Core.AdminService.AdminServiceBase
{
    private readonly ILogger _log;

    public AdminService(ILogger log)
    {
        _log = log;
    }

    public override async Task<GetByCredentialsResponse> GetByCredentials(GetByCredentialsRequest request,
        ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new AdminRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.GetByCredentialsAsync(request.UserName, request.Password, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            return new GetByCredentialsResponse { Admin = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new GetByCredentialsResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<UpdateAdminResponse> Update(UpdateAdminRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new AdminRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.UpdateAsync(request.Admin, request.OldPassword, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            return new UpdateAdminResponse { Admin = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new UpdateAdminResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }
}