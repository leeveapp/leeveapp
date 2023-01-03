using Leeve.Core;
using Leeve.Core.Common;

namespace Leeve.Server.Questionnaires;

public sealed class QuestionnaireService : Core.QuestionnaireService.QuestionnaireServiceBase
{
    private readonly ILogger _log;
    private readonly QuestionnaireNotifier _notifier;

    public QuestionnaireService(ILogger log, QuestionnaireNotifier notifier)
    {
        _log = log;
        _notifier = notifier;
    }

    public override async Task<AddQuestionnaireResponse> Add(AddQuestionnaireRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new QuestionnaireRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.AddAsync(request.Questionnaire, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            _notifier.Notify(new QuestionnaireMessage
            {
                Action = Actions.Add,
                CallerId = request.CallerId,
                Questionnaire = result.Value
            });

            return new AddQuestionnaireResponse { Questionnaire = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new AddQuestionnaireResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<GetQuestionnaireResponse> Get(GetQuestionnaireRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new QuestionnaireRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.GetAsync(request.Id, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            return new GetQuestionnaireResponse { Questionnaire = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new GetQuestionnaireResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task GetAllByTeacherId(GetAllQuestionnairesByTeacherIdRequest request,
        IServerStreamWriter<GetAllQuestionnairesByTeacherIdResponse> responseStream, ServerCallContext context)
    {
        try
        {
            var token = context.CancellationToken;
            var dbContext = DbFactory.Get();
            var repo = new QuestionnaireRepository(dbContext, _log);

            var result = await repo.GetAllByIdAsync(request.TeacherId, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            if (token.IsCancellationRequested) return;

            var questionnaires = result.Value;
            foreach (var dto in questionnaires)
            {
                if (token.IsCancellationRequested) return;
                await responseStream.WriteAsync(new GetAllQuestionnairesByTeacherIdResponse { Questionnaire = dto }, token);
            }
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<UpdateQuestionnaireResponse> Update(UpdateQuestionnaireRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new QuestionnaireRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.UpdateAsync(request.Questionnaire, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            _notifier.Notify(new QuestionnaireMessage
            {
                Action = Actions.Update,
                CallerId = request.CallerId,
                Questionnaire = result.Value
            });

            return new UpdateQuestionnaireResponse { Questionnaire = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new UpdateQuestionnaireResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }

    public override async Task<DeleteQuestionnaireResponse> Delete(DeleteQuestionnaireRequest request, ServerCallContext context)
    {
        try
        {
            var dbContext = DbFactory.Get();
            var repo = new QuestionnaireRepository(dbContext, _log);
            var token = context.CancellationToken;

            var result = await repo.DeleteAsync(request.Id, request.TeacherId, token);
            if (result.IsFaulted)
                throw new RpcException(new Status(StatusCode.Unknown, result.ToString()));

            _notifier.Notify(new QuestionnaireMessage
            {
                Action = Actions.Delete,
                CallerId = request.CallerId,
                Questionnaire = result.Value
            });

            return new DeleteQuestionnaireResponse { Questionnaire = result };
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            return new DeleteQuestionnaireResponse();
        }
        catch (Exception e)
        {
            var metadata = new Metadata { { "Message", "Operation failed to execute. Please try again!" } };
            throw new RpcException(new Status(StatusCode.Unknown, e.Message), metadata);
        }
    }
}