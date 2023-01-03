using System.Collections.ObjectModel;
using Leeve.Client.Common;
using Leeve.Client.Users;
using Leeve.Core;
using Leeve.Core.Common;
using Leeve.Core.Questionnaires;

namespace Leeve.Client.Questionnaires;

public interface IQuestionnaireClientService
{
    Task<Result> AddAsync(Questionnaire questionnaire);
    Task<Result> GetAllAsync(ObservableCollection<Questionnaire> collection, CancellationToken token = default);
    Result Search(ObservableCollection<Questionnaire> collection, string searchString);
    Task<Result> UpdateAsync(Questionnaire questionnaire);
    Task<Result> DeleteAsync(string id);
}

public sealed class QuestionnaireClientService : IQuestionnaireClientService
{
    private readonly ChannelManager _manager;
    private readonly IThreadWrapper _threadWrapper;

    public QuestionnaireClientService(ChannelManager manager, IThreadWrapper threadWrapper)
    {
        _manager = manager;
        _threadWrapper = threadWrapper;
    }

    public async Task<Result> AddAsync(Questionnaire questionnaire)
    {
        try
        {
            var dto = questionnaire.ToDto();
            dto.TeacherId = TeacherAssist.Id;

            var request = new AddQuestionnaireRequest
            {
                CallerId = TeacherAssist.CallerId,
                Questionnaire = dto
            };
            var service = new QuestionnaireService.QuestionnaireServiceClient(_manager.Channel);
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

    public async Task<Result> GetAllAsync(ObservableCollection<Questionnaire> collection,
        CancellationToken token = default)
    {
        if (collection is null) throw new ArgumentNullException(nameof(collection));
        if (QuestionnaireServiceHub.Count == 0)
            return await GetAllFromDbAsync(collection, token);

        return GetAllFromMemory(collection, token);
    }

    private async Task<Result> GetAllFromDbAsync(ICollection<Questionnaire> collection, CancellationToken token)
    {
        try
        {
            _threadWrapper.Invoke(collection.Clear);

            var teacherId = TeacherAssist.Id;
            var request = new GetAllQuestionnairesByTeacherIdRequest { TeacherId = teacherId };
            var service = new QuestionnaireService.QuestionnaireServiceClient(_manager.Channel);

            using var call = service.GetAllByTeacherId(request, deadline: ChannelManager.DeadLine,
                cancellationToken: token);
            while (await call.ResponseStream.MoveNext(token).ConfigureAwait(false))
            {
                var item = call.ResponseStream.Current.Questionnaire;
                var entity = item.ToEntity();
                _threadWrapper.Invoke(() => collection.Add(entity));
                QuestionnaireServiceHub.AddOrUpdate(item.Id, entity);
            }
            return new Result();
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            QuestionnaireServiceHub.Clear(); //to ensure that the next call will be from the database
            _threadWrapper.Invoke(collection.Clear);
            return new Result();
        }
        catch (RpcException)
        {
            QuestionnaireServiceHub.Clear(); //to ensure that the next call will be from the database
            _threadWrapper.Invoke(collection.Clear);
            return new Result(new Exception("Unable to fetch data from server"));
        }
    }

    private Result GetAllFromMemory(ObservableCollection<Questionnaire> collection, CancellationToken token)
    {
        try
        {
            collection.Clear();

            var items = QuestionnaireServiceHub.GetAll().OrderBy(c => c.Title);
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

    public Result Search(ObservableCollection<Questionnaire> collection, string searchString)
    {
        if (collection is null) throw new ArgumentNullException(nameof(collection));

        try
        {
            collection.Clear();

            var items = QuestionnaireServiceHub.Search(kvp => kvp.Value.Title.ToLower().Contains(searchString.ToLower()))
                .OrderBy(c => c.Title);
            foreach (var item in items) collection.Add(item);

            return new Result();
        }
        catch (Exception)
        {
            collection.Clear();
            return new Result(new Exception("Searching failed"));
        }
    }

    public async Task<Result> UpdateAsync(Questionnaire questionnaire)
    {
        try
        {
            var dto = questionnaire.ToDto();
            dto.TeacherId = TeacherAssist.Id;

            var request = new UpdateQuestionnaireRequest
            {
                CallerId = TeacherAssist.CallerId,
                Questionnaire = dto
            };

            var service = new QuestionnaireService.QuestionnaireServiceClient(_manager.Channel);
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

    public async Task<Result> DeleteAsync(string id)
    {
        try
        {
            var request = new DeleteQuestionnaireRequest
            {
                CallerId = TeacherAssist.CallerId,
                Id = id,
                TeacherId = TeacherAssist.Id
            };

            var service = new QuestionnaireService.QuestionnaireServiceClient(_manager.Channel);
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