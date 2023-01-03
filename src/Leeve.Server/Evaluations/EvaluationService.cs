using Leeve.Core;
using Leeve.Core.Common;

namespace Leeve.Server.Evaluations;

public sealed class EvaluationService : Core.EvaluationService.EvaluationServiceBase
{
    private readonly ILogger _log;
    private readonly EvaluationNotifier _notifier;
    private readonly EvaluationProcessNotifier _processNotifier;
    private readonly EvaluationSubmitNotifier _submitNotifier;

    public EvaluationService(ILogger log,
        EvaluationNotifier notifier,
        EvaluationProcessNotifier processNotifier,
        EvaluationSubmitNotifier submitNotifier)
    {
        _log = log;
        _notifier = notifier;
        _processNotifier = processNotifier;
        _submitNotifier = submitNotifier;
    }

    public override async Task<AddEvaluationResponse> Add(AddEvaluationRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new EvaluationRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.AddAsync(request.Evaluation, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            _notifier.Notify(new EvaluationMessage
            {
                Action = Actions.Add,
                CallerId = request.CallerId,
                Evaluation = result.Value
            });

            return new AddEvaluationResponse { Evaluation = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new AddEvaluationResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<GetEvaluationResponse> Get(GetEvaluationRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new EvaluationRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.GetAsync(request.Id, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            return new GetEvaluationResponse { Evaluation = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new GetEvaluationResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task GetAllByTeacherId(GetAllEvaluationsByTeacherIdRequest request,
        IServerStreamWriter<GetAllEvaluationsByTeacherIdResponse> responseStream, ServerCallContext context)
    {
        try
        {
            var token = context.CancellationToken;
            var dbContext = DbFactory.Get();
            var repo = new EvaluationRepository(dbContext, _log);

            var result = await repo.GetAllByTeacherIdAsync(request.TeacherId, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            if (token.IsCancellationRequested) return;

            var evaluations = result.Value;
            foreach (var dto in evaluations)
            {
                if (token.IsCancellationRequested) return;

                await SetEvaluationResult(repo, dto, result, token);

                SetEvaluationStatus(dto);

                await responseStream.WriteAsync(new GetAllEvaluationsByTeacherIdResponse { Evaluation = dto }, token);
            }
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    private void SetEvaluationStatus(EvaluationDto dto)
    {
        if (!EvaluationCache.IsActive(dto.Id, out var code)) return;

        dto.Code = code;
        dto.IsActive = true;
    }

    private static async Task SetEvaluationResult(EvaluationRepository repo, EvaluationDto dto,
        Result<List<EvaluationDto>> result, CancellationToken token = default)
    {
        var responses = await repo.ResponsesCountAsync(dto.Id, token);
        if (responses.IsFaulted)
            throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

        dto.Responses = responses;
    }

    public override async Task<UpdateEvaluationResponse> Update(UpdateEvaluationRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new EvaluationRepository(dbContext, _log);
            var token = context.CancellationToken;

            var dto = new EvaluationDto
            {
                Id = request.EvaluationId,
                Title = request.Title,
                Description = request.Description
            };

            var result = await repo.UpdateAsync(dto, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            _notifier.Notify(new EvaluationMessage
            {
                Action = Actions.Update,
                CallerId = request.CallerId,
                Evaluation = result.Value
            });

            return new UpdateEvaluationResponse { Evaluation = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new UpdateEvaluationResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<DeleteEvaluationResponse> Delete(DeleteEvaluationRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new EvaluationRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.DeleteAsync(request.Id, request.TeacherId, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            _notifier.Notify(new EvaluationMessage
            {
                Action = Actions.Delete,
                CallerId = request.CallerId,
                Evaluation = result.Value
            });

            return new DeleteEvaluationResponse { Evaluation = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new DeleteEvaluationResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override Task<StartEvaluationResponse> Start(StartEvaluationRequest request, ServerCallContext context)
    {
        try
        {
            var id = request.EvaluationId;

            var active = EvaluationCache.IsActive(id, out var code);
            if (!active) code = EvaluationCache.Add(id);

            _processNotifier.Notify(new EvaluationProcessMessage
            {
                Action = Actions.Add,
                CallerId = request.EvaluationId,
                EvaluationId = id,
                EvaluationCode = code
            });

            return Task.FromResult(new StartEvaluationResponse { EvaluationId = id, Code = code });
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override Task<StopEvaluationResponse> Stop(StopEvaluationRequest request, ServerCallContext context)
    {
        try
        {
            EvaluationCache.Remove(request.EvaluationId);
            _processNotifier.Notify(new EvaluationProcessMessage
            {
                Action = Actions.Delete,
                CallerId = request.EvaluationId,
                EvaluationId = request.EvaluationId,
                EvaluationCode = string.Empty
            });
            return Task.FromResult(new StopEvaluationResponse { EvaluationId = request.EvaluationId });
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<EvaluateResponse> Evaluate(EvaluateRequest request, ServerCallContext context)
    {
        try
        {
            var result = EvaluationCache.Get(request.Code);
            if (result.IsFaulted) throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            var repository = new EvaluationRepository(DbFactory.Get(), _log);
            var evaluation = await repository.GetAsync(result, context.CancellationToken);
            if (evaluation.IsFaulted) throw new RpcException(new Status(StatusCode.Unknown, evaluation.ToString()));

            return new EvaluateResponse { Evaluation = evaluation };
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<SubmitEvaluationResponse> Submit(SubmitEvaluationRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new EvaluationRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.SubmitResultAsync(request.Evaluation, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            _submitNotifier.Notify(new EvaluationSubmitMessage
            {
                Count = result,
                CallerId = request.CallerId,
                EvaluationId = request.Evaluation.Id
            });

            return new SubmitEvaluationResponse();
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            throw new RpcException(new Status(StatusCode.Cancelled, e.Message));
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task GetResults(GetEvaluationResultsRequest request,
        IServerStreamWriter<GetEvaluationResultsResponse> responseStream, ServerCallContext context)
    {
        try
        {
            var token = context.CancellationToken;
            var dbContext = DbFactory.Get();
            var repo = new EvaluationRepository(dbContext, _log);

            var result = await repo.GetResultsAsync(request.EvaluationId, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            if (token.IsCancellationRequested) return;

            var evaluations = result.Value;
            foreach (var dto in evaluations)
            {
                if (token.IsCancellationRequested) return;

                await responseStream.WriteAsync(new GetEvaluationResultsResponse { Evaluation = dto }, token);
            }
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }
}