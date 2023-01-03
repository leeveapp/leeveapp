using System.Collections.ObjectModel;
using Leeve.Client.Common;
using Leeve.Client.Users;
using Leeve.Core;
using Leeve.Core.Common;
using Leeve.Core.Evaluations;

namespace Leeve.Client.Evaluations;

public interface IEvaluationClientService
{
    Task<Result> GetAllAsync(ObservableCollection<Evaluation> evaluations, CancellationToken token = default);
    Task<Result> GetAllActiveAsync(ObservableCollection<Evaluation> collection, CancellationToken token = default);
    Result Search(ObservableCollection<Evaluation> collection, string searchString);
    Task<Result> AddAsync(Evaluation evaluation);
    Task<Result> DeleteAsync(string id);
    Task<Result> StartEvaluationAsync(string id);
    Task<Result> StopEvaluationAsync(string id);
    Task<Result<Evaluation>> RequestEvaluationAsync(string code);
    Task<Result> SubmitEvaluationAsync(Evaluation entity);
    Task<Result> GenerateReportAsync(string id, string path, CancellationToken token = default);
}

public sealed class EvaluationClientService : IEvaluationClientService
{
    private readonly ChannelManager _manager;
    private readonly IThreadWrapper _threadWrapper;

    public EvaluationClientService(ChannelManager manager, IThreadWrapper threadWrapper)
    {
        _manager = manager;
        _threadWrapper = threadWrapper;
    }

    public async Task<Result> GetAllAsync(ObservableCollection<Evaluation> collection, CancellationToken token = default)
    {
        if (collection is null) throw new ArgumentNullException(nameof(collection));
        if (EvaluationServiceHub.Count == 0)
            return await GetAllFromDbAsync(collection, token);

        return GetAllFromMemory(collection, token);
    }

    private async Task<Result> GetAllFromDbAsync(ICollection<Evaluation> collection, CancellationToken token)
    {
        try
        {
            _threadWrapper.Invoke(collection.Clear);

            var service = new EvaluationService.EvaluationServiceClient(_manager.Channel);
            var request = new GetAllEvaluationsByTeacherIdRequest { TeacherId = TeacherAssist.Id };
            using var call = service.GetAllByTeacherId(request, deadline: ChannelManager.DeadLine, cancellationToken: token);
            while (await call.ResponseStream.MoveNext(token).ConfigureAwait(false))
            {
                var item = call.ResponseStream.Current.Evaluation;
                var entity = item.ToEntity();
                _threadWrapper.Invoke(() => collection.Add(entity));
                EvaluationServiceHub.AddOrUpdate(item.Id, entity);
            }
            return new Result();
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            EvaluationServiceHub.Clear(); //to ensure that the next call will be from the database
            _threadWrapper.Invoke(collection.Clear);
            return new Result();
        }
        catch (RpcException)
        {
            EvaluationServiceHub.Clear(); //to ensure that the next call will be from the database
            _threadWrapper.Invoke(collection.Clear);
            return new Result(new Exception("Unable to fetch data from server"));
        }
    }

    private Result GetAllFromMemory(ICollection<Evaluation> collection, CancellationToken token)
    {
        try
        {
            collection.Clear();

            var items = EvaluationServiceHub.GetAll().OrderByDescending(t => t.CreatedOn);
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

    public async Task<Result> GetAllActiveAsync(ObservableCollection<Evaluation> collection, CancellationToken token = default)
    {
        if (collection is null) throw new ArgumentNullException(nameof(collection));
        if (EvaluationServiceHub.Count == 0)
            await GetAllFromDbAsync(collection, token);

        return GetAllActiveFromMemory(collection);
    }

    private static Result GetAllActiveFromMemory(ICollection<Evaluation> collection)
    {
        try
        {
            collection.Clear();

            var items = EvaluationServiceHub.GetAll().Where(t => t.IsActive).OrderByDescending(t => t.CreatedOn);
            foreach (var item in items)
                collection.Add(item);

            return new Result();
        }
        catch (Exception)
        {
            collection.Clear();
            return new Result(new Exception("Unable to fetch data from server"));
        }
    }

    public Result Search(ObservableCollection<Evaluation> collection, string searchString)
    {
        if (collection is null) throw new ArgumentNullException(nameof(collection));

        try
        {
            collection.Clear();

            var items = EvaluationServiceHub.Search(kvp => kvp.Value.Title.ToLower().Contains(searchString.ToLower()))
                .OrderByDescending(c => c.CreatedOn);
            foreach (var item in items) collection.Add(item);

            return new Result();
        }
        catch (Exception)
        {
            collection.Clear();
            return new Result(new Exception("Searching failed"));
        }
    }

    public async Task<Result> AddAsync(Evaluation evaluation)
    {
        try
        {
            var dto = evaluation.ToDto();
            dto.TeacherId = TeacherAssist.Id;
            dto.Questionnaire.TeacherId = TeacherAssist.Id;

            var request = new AddEvaluationRequest
            {
                CallerId = TeacherAssist.CallerId,
                Evaluation = dto
            };

            var service = new EvaluationService.EvaluationServiceClient(_manager.Channel);
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

    public async Task<Result> StartEvaluationAsync(string id)
    {
        try
        {
            var request = new StartEvaluationRequest
            {
                CallerId = TeacherAssist.CallerId,
                EvaluationId = id
            };

            var service = new EvaluationService.EvaluationServiceClient(_manager.Channel);
            await service.StartAsync(request, deadline: ChannelManager.DeadLine);

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

    public async Task<Result> StopEvaluationAsync(string id)
    {
        try
        {
            var request = new StopEvaluationRequest
            {
                CallerId = TeacherAssist.CallerId,
                EvaluationId = id
            };

            var service = new EvaluationService.EvaluationServiceClient(_manager.Channel);
            await service.StopAsync(request, deadline: ChannelManager.DeadLine);

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
            var request = new DeleteEvaluationRequest
            {
                CallerId = TeacherAssist.CallerId,
                TeacherId = id,
                Id = id
            };

            var service = new EvaluationService.EvaluationServiceClient(_manager.Channel);
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

    public async Task<Result<Evaluation>> RequestEvaluationAsync(string code)
    {
        try
        {
            var request = new EvaluateRequest { Code = code };
            var service = new EvaluationService.EvaluationServiceClient(_manager.Channel);
            var evaluation = await service.EvaluateAsync(request, deadline: ChannelManager.DeadLine);

            var entity = evaluation.Evaluation.ToEntity();
            return new Result<Evaluation>(entity);
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Unavailable)
        {
            return new Result<Evaluation>(new Exception("Server unavailable"));
        }
        catch (RpcException e)
        {
            return new Result<Evaluation>(new Exception(e.Status.ExtractMessage()));
        }
    }

    public async Task<Result> SubmitEvaluationAsync(Evaluation entity)
    {
        try
        {
            var evaluation = entity.ToDto();
            var validationResult = evaluation.Validate();
            if (validationResult.IsFaulted) return validationResult;

            var request = new SubmitEvaluationRequest { Evaluation = evaluation };
            var service = new EvaluationService.EvaluationServiceClient(_manager.Channel);
            await service.SubmitAsync(request, deadline: ChannelManager.DeadLine);
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

    public async Task<Result> GenerateReportAsync(string id, string path, CancellationToken token = default)
    {
        try
        {
            var request = new GetEvaluationResultsRequest { EvaluationId = id };
            var service = new EvaluationService.EvaluationServiceClient(_manager.Channel);
            using var call = service.GetResults(request, deadline: ChannelManager.DeadLine, cancellationToken: token);

            var entities = new List<Evaluation>();
            while (await call.ResponseStream.MoveNext(token).ConfigureAwait(false))
            {
                var item = call.ResponseStream.Current.Evaluation;
                entities.Add(item.ToEntity());
            }

            var exporter = new EvaluationExportService();
            return await exporter.ExportAsync(entities, path);
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