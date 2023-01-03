using System.Collections.ObjectModel;
using Leeve.Client.Common;
using Leeve.Core;
using Leeve.Core.Common;
using Leeve.Core.Users;

namespace Leeve.Client.Users;

public interface ITeacherClientService
{
    public Task<Result<Teacher>> LoginAsync(string userName, string password);
    Task<Result<TeacherImage>> UpdateTeacherImageAsync(string imagePath, string userId);
    Task<Result> GetAllAsync(ObservableCollection<Teacher> collection, CancellationToken token = default);
    Result Search(ObservableCollection<Teacher> teachers, string searchString);
    Task<Result> AddAsync(Teacher entity);
    Task<Result> UpdateByAdminAsync(Teacher entity);
    Task<Result> DeleteAsync(string id);
    Task<Result> UpdateAsync(Teacher entity, string password);
    Task<Result> UpdateCredentialsAsync(string username, string password, string currentPassword);
}

public sealed class TeacherClientService : ITeacherClientService
{
    private readonly ChannelManager _manager;
    private readonly IThreadWrapper _threadWrapper;

    public TeacherClientService(ChannelManager manager, IThreadWrapper threadWrapper)
    {
        _manager = manager;
        _threadWrapper = threadWrapper;
    }

    public async Task<Result<Teacher>> LoginAsync(string userName, string password)
    {
        try
        {
            var request = new GetTeacherByCredentialsRequest
            {
                UserName = userName,
                Password = password
            };

            var service = new TeacherService.TeacherServiceClient(_manager.Channel);
            var response = await service.GetByCredentialsAsync(request, deadline: ChannelManager.DeadLine);

            var dto = response.Teacher;
            var model = new Teacher
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Department = dto.Department,
                Image = dto.Image?.ToByteArray()
            };

            return new Result<Teacher>(model);
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Unavailable)
        {
            return new Result<Teacher>(new Exception("Server unavailable"));
        }
        catch (RpcException e)
        {
            return new Result<Teacher>(new Exception(e.Status.ExtractMessage()));
        }
    }

    public async Task<Result<TeacherImage>> UpdateTeacherImageAsync(string imagePath, string userId)
    {
        try
        {
            var imageBytes = await imagePath.ToImageByteArrayAsync(200, 200).ConfigureAwait(false);
            var request = new UpdateTeacherImageRequest
            {
                CallerId = TeacherAssist.CallerId,
                UserId = userId,
                TeacherImage = new TeacherImageDto
                {
                    Id = TeacherAssist.Id,
                    Image = ByteString.CopyFrom(imageBytes)
                }
            };

            var service = new TeacherImageService.TeacherImageServiceClient(_manager.Channel);
            var response = await service.UpdateTeacherImageAsync(request, deadline: ChannelManager.DeadLine);

            var image = new TeacherImage
            {
                Id = response.TeacherImage.Id,
                Image = response.TeacherImage.Image.ToByteArray()
            };

            return new Result<TeacherImage>(image);
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Unavailable)
        {
            return new Result<TeacherImage>(new Exception("Server unavailable"));
        }
        catch (RpcException e)
        {
            return new Result<TeacherImage>(new Exception(e.Status.ExtractMessage()));
        }
    }

    public async Task<Result> GetAllAsync(ObservableCollection<Teacher> collection,
        CancellationToken token = default)
    {
        if (collection is null) throw new ArgumentNullException(nameof(collection));
        if (TeacherServiceHub.Count == 0)
            return await GetAllFromDbAsync(collection, token);

        return GetAllFromMemory(collection, token);
    }

    public Result Search(ObservableCollection<Teacher> collection, string searchString)
    {
        if (collection is null) throw new ArgumentNullException(nameof(collection));

        try
        {
            collection.Clear();

            var items = TeacherServiceHub.Search(kvp => kvp.Value.FullName.ToLower().Contains(searchString.ToLower()))
                .OrderBy(c => c.FullName);
            foreach (var item in items) collection.Add(item);

            return new Result();
        }
        catch (Exception)
        {
            collection.Clear();
            return new Result(new Exception("Searching failed"));
        }
    }

    private async Task<Result> GetAllFromDbAsync(ICollection<Teacher> collection, CancellationToken token)
    {
        try
        {
            _threadWrapper.Invoke(collection.Clear);

            var service = new TeacherService.TeacherServiceClient(_manager.Channel);
            using var call = service.GetAll(new GetAllTeachersRequest(), deadline: ChannelManager.DeadLine,
                cancellationToken: token);
            while (await call.ResponseStream.MoveNext(token).ConfigureAwait(false))
            {
                var item = call.ResponseStream.Current.Teacher;
                var entity = item.ToEntity();
                _threadWrapper.Invoke(() => collection.Add(entity));
                TeacherServiceHub.AddOrUpdate(item.Id, entity);
            }
            return new Result();
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            TeacherServiceHub.Clear(); //to ensure that the next call will be from the database
            _threadWrapper.Invoke(collection.Clear);
            return new Result();
        }
        catch (RpcException)
        {
            TeacherServiceHub.Clear(); //to ensure that the next call will be from the database
            _threadWrapper.Invoke(collection.Clear);
            return new Result(new Exception("Unable to fetch data from server"));
        }
    }

    private Result GetAllFromMemory(ObservableCollection<Teacher> collection, CancellationToken token)
    {
        try
        {
            collection.Clear();

            var items = TeacherServiceHub.GetAll().OrderBy(c => c.FullName);
            foreach (var item in items)
            {
                token.ThrowIfCancellationRequested();
                collection.Add(item);
            }
            return new Result();
        }
        catch (OperationCanceledException)
        {
            collection.Clear();
            return new Result();
        }
        catch (Exception)
        {
            collection.Clear();
            return new Result(new Exception("Unable to fetch data from server"));
        }
    }

    public async Task<Result> AddAsync(Teacher entity)
    {
        try
        {
            var request = new AddTeacherRequest
            {
                CallerId = AdminAssist.CallerId,
                Teacher = entity.ToDto()
            };

            var service = new TeacherService.TeacherServiceClient(_manager.Channel);
            await service.AddAsync(request, deadline: ChannelManager.DeadLine);

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

    public async Task<Result> UpdateByAdminAsync(Teacher entity)
    {
        try
        {
            var request = new UpdateTeacherByAdminRequest
            {
                CallerId = AdminAssist.CallerId,
                Teacher = entity.ToDto(),
                AdminId = AdminAssist.AdminId
            };

            var service = new TeacherService.TeacherServiceClient(_manager.Channel);
            await service.UpdateByAdminAsync(request, deadline: ChannelManager.DeadLine);

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

    public async Task<Result> UpdateAsync(Teacher entity, string password)
    {
        try
        {
            var request = new UpdateTeacherRequest
            {
                CallerId = AdminAssist.CallerId,
                Teacher = entity.ToDto(),
                Password = password
            };

            var service = new TeacherService.TeacherServiceClient(_manager.Channel);
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

    public async Task<Result> UpdateCredentialsAsync(string username, string password, string currentPassword)
    {
        try
        {
            var request = new UpdateTeacherCredentialsRequest
            {
                Credential = new CredentialDto
                {
                    UserName = username,
                    Password = password,
                    OldPassword = currentPassword
                },
                TeacherId = TeacherAssist.Id
            };

            var service = new TeacherService.TeacherServiceClient(_manager.Channel);
            await service.UpdateCredentialsAsync(request, deadline: ChannelManager.DeadLine);

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

    public async Task<Result> DeleteAsync(string id)
    {
        try
        {
            var request = new DeleteTeacherRequest
            {
                CallerId = AdminAssist.CallerId,
                TeacherId = id,
                AdminId = AdminAssist.AdminId
            };

            var service = new TeacherService.TeacherServiceClient(_manager.Channel);
            await service.DeleteAsync(request, deadline: ChannelManager.DeadLine);

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