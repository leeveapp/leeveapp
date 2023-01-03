using Leeve.Core;
using Leeve.Core.Common;
using Leeve.Core.Evaluations;
using Leeve.Core.Questionnaires;
using MongoSharpen.Builders;

namespace Leeve.Server.Evaluations;

public sealed class EvaluationRepository
{
    private readonly IDbContext _context;
    private readonly ILogger _log;

    public EvaluationRepository(IDbContext context, ILogger log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Result<EvaluationDto>> AddAsync(EvaluationDto dto, CancellationToken token)
    {
        try
        {
            var validation = dto.Validate();
            if (validation.IsFaulted) return new Result<EvaluationDto>(new ArgumentException(validation.ToString()));

            var checkResult = await CheckDuplicateAsync(_context, dto);
            if (checkResult.IsFaulted) return checkResult;

            var entity = dto.ToEntity();
            entity.CreatedBy = dto.TeacherId;

            await _context.SaveAsync(entity, token);

            return new Result<EvaluationDto>(entity.ToDto());
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to save evaluation with error: {Message}", e.Message);
            return new Result<EvaluationDto>(new Exception("Failed to save evaluation"));
        }
    }

    private static async Task<Result<EvaluationDto>> CheckDuplicateAsync(IDbContext db, EvaluationDto dto,
        bool forUpdate = false)
    {
        var filter = Builders<Evaluation>.Filter.Match(t => t.Title.ToLower() == dto.Title.ToLower());

        if (forUpdate) filter = filter.Match(t => t.Id != dto.Id);

        var exist = await db.Find(filter).AnyAsync();
        return exist
            ? new Result<EvaluationDto>(new ArgumentException($"Evaluation with title [{dto.Title}] already exist"))
            : new Result<EvaluationDto>(dto);
    }

    public async Task<Result<EvaluationDto>> GetAsync(string id, CancellationToken token = default)
    {
        try
        {
            var entity = await _context.Find<Evaluation>(x => x.MatchId(id)).ExecuteFirstOrDefaultAsync(token);

            if (entity == null) return new Result<EvaluationDto>(new ArgumentException($"Evaluation with id [{id}] not found"));

            var responses = await _context.CountAsync<EvaluationResult>(x => x.Match(i => i.EvaluationId == id), token: token);
            var dto = entity.ToDto();
            dto.Responses = (int) responses;

            return new Result<EvaluationDto>(dto);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to get evaluation with error: {Message}", e.Message);
            return new Result<EvaluationDto>(new Exception("Failed to get evaluation"));
        }
    }

    public async Task<Result<List<EvaluationDto>>> GetAllByTeacherIdAsync(string teacherId, CancellationToken token = default)
    {
        try
        {
            var entities = await _context.Find<Evaluation, EvaluationDto>(x => x.Match(t => t.TeacherId == teacherId))
                .Project(x => x.ToDto())
                .Sort(x => x.By(i => i.CreatedOn!, Order.Descending))
                .ExecuteAsync(token);
            return new Result<List<EvaluationDto>>(entities);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to get evaluation with error: {Message}", e.Message);
            return new Result<List<EvaluationDto>>(new Exception("Failed to get evaluation"));
        }
    }

    public async Task<Result<int>> ResponsesCountAsync(string id, CancellationToken token = default)
    {
        try
        {
            var responses = await _context.CountAsync<EvaluationResult>(x => x.Match(i => i.EvaluationId == id), token: token);
            return new Result<int>((int) responses);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to get evaluation results with error: {Message}", e.Message);
            return new Result<int>(new Exception("Failed to get evaluation results"));
        }
    }

    public async Task<Result<EvaluationDto>> UpdateAsync(EvaluationDto dto, CancellationToken token)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Title))
                return new Result<EvaluationDto>(new Exception("Evaluation title is required"));

            var checkResult = await CheckDuplicateAsync(_context, dto, true);
            if (checkResult.IsFaulted) return checkResult;

            var oldEntity = await _context.Find<Evaluation>(x => x.MatchId(dto.Id)).ExecuteFirstOrDefaultAsync(token);
            if (oldEntity == null) return new Result<EvaluationDto>(new Exception("Evaluation not found"));

            using var trans = _context.Transaction();

            await _context.LogAsync(oldEntity, token);

            ModifiedBy modifiedBy = dto.TeacherId;
            var result = await _context.Update<Evaluation, EvaluationDto>(x => x.MatchId(dto.Id))
                .Modify(x => x
                    .Set(i => i.Title, dto.Title)
                    .Set(i => i.Description, dto.Description)
                    .Set(i => i.ModifiedBy, modifiedBy))
                .Project(x => x.ToDto())
                .ExecuteAndGetAsync(token);

            await trans.CommitAsync(token);

            return new Result<EvaluationDto>(result);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to save evaluation with error: {Message}", e.Message);
            return new Result<EvaluationDto>(new Exception("Failed to save evaluation"));
        }
    }

    public async Task<Result<EvaluationDto>> DeleteAsync(string id, string teacherId, CancellationToken token = default)
    {
        try
        {
            var oldEntity = await _context.Find<Evaluation>(x => x.MatchId(id)).ExecuteFirstOrDefaultAsync(token);
            if (oldEntity == null) return new Result<EvaluationDto>(new Exception("Evaluation not found"));

            using var trans = _context.Transaction();

            await _context.LogAsync(oldEntity, token);
            var result = await _context.SoftDelete<Evaluation>(x => x.MatchId(id)).ExecuteAndGetAsync(teacherId, token: token);

            await trans.CommitAsync(token);

            return new Result<EvaluationDto>(result.ToDto());
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to update evaluation with error: {Message}", e.Message);
            return new Result<EvaluationDto>(new Exception("Failed to update teacher"));
        }
    }

    public async Task<Result<int>> SubmitResultAsync(EvaluationDto dto, CancellationToken token)
    {
        try
        {
            var validation = dto.Questionnaire.ValidateAnswers();
            if (validation.IsFaulted) return new Result<int>(new ArgumentException(validation.ToString()));

            var entity = new EvaluationResult
            {
                EvaluationId = dto.Id,
                Evaluation = dto.ToEntity(),
                TeacherId = dto.TeacherId
            };

            await _context.SaveAsync(entity, token);

            var count = await _context.CountAsync<EvaluationResult>(x => x.Match(i => i.EvaluationId == dto.Id), token: token);

            return new Result<int>((int) count);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to submit evaluation with error: {Message}", e.Message);
            return new Result<int>(new Exception("Failed to submit evaluation"));
        }
    }

    public async Task<Result<List<EvaluationDto>>> GetResultsAsync(string id, CancellationToken token = default)
    {
        try
        {
            var entities = await _context.Find<EvaluationResult, EvaluationDto>(x => x.Match(i => i.EvaluationId == id))
                .Project(x => x.ToDto())
                .Sort(x => x.By(i => i.CreatedOn!))
                .ExecuteAsync(token);
            return new Result<List<EvaluationDto>>(entities);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to get evaluation results with error: {Message}", e.Message);
            return new Result<List<EvaluationDto>>(new Exception("Failed to get evaluation results"));
        }
    }
}