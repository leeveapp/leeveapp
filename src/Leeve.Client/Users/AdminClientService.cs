using Leeve.Client.Common;
using Leeve.Core;
using Leeve.Core.Common;

namespace Leeve.Client.Users;

public interface IAdminClientService
{
    Task<Result> LoginAsync(string userName, string password);
    Task<Result> UpdateCredentialsAsync(string username, string password, string oldPassword);
}

public sealed class AdminClientService : IAdminClientService
{
    private readonly ChannelManager _manager;

    public AdminClientService(ChannelManager manager)
    {
        _manager = manager;
    }

    public async Task<Result> LoginAsync(string userName, string password)
    {
        try
        {
            var request = new GetByCredentialsRequest
            {
                UserName = userName,
                Password = password
            };

            var service = new AdminService.AdminServiceClient(_manager.Channel);
            var response = await service.GetByCredentialsAsync(request, deadline: ChannelManager.DeadLine);

            AdminAssist.AdminId = response.Admin.Id;

            return new Result();
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Unavailable)
        {
            return new Result(new Exception("Server unavailable"));
        }
        catch (RpcException e)
        {
            return new Result(new Exception(e.Status.ExtractMessage()));
        }
    }

    public async Task<Result> UpdateCredentialsAsync(string username, string password, string oldPassword)
    {
        try
        {
            var request = new UpdateAdminRequest
            {
                Admin = new AdminDto { Id = AdminAssist.AdminId, UserName = username, Password = password },
                OldPassword = oldPassword
            };

            var service = new AdminService.AdminServiceClient(_manager.Channel);
            await service.UpdateAsync(request, deadline: ChannelManager.DeadLine);

            return new Result();
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Unavailable)
        {
            return new Result(new Exception("Server unavailable"));
        }
        catch (RpcException e)
        {
            return new Result(new Exception(e.Status.ExtractMessage()));
        }
    }
}