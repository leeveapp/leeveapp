using Leeve.Core;
using Leeve.Core.Common;
using Leeve.Core.Questionnaires;

namespace Leeve.Server.Questionnaires;

public sealed class QuestionnaireRepository
{
    private readonly IDbContext _context;
    private readonly ILogger _log;

    public QuestionnaireRepository(IDbContext context, ILogger log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Result<QuestionnaireDto>> AddAsync(QuestionnaireDto dto, CancellationToken token)
    {
        try
        {
            var validation = dto.Validate();
            if (validation.IsFaulted) return new Result<QuestionnaireDto>(new ArgumentException(validation.ToString()));

            var checkResult = await CheckDuplicateAsync(_context, dto);
            if (checkResult.IsFaulted) return checkResult;

            var entity = dto.ToEntity();
            entity.CreatedBy = dto.TeacherId;

            await _context.SaveAsync(entity, token);

            return new Result<QuestionnaireDto>(entity.ToDto());
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to save questionnaire with error: {Message}", e.Message);
            return new Result<QuestionnaireDto>(new Exception("Failed to save questionnaire"));
        }
    }

    private static async Task<Result<QuestionnaireDto>> CheckDuplicateAsync(IDbContext db, QuestionnaireDto dto,
        bool forUpdate = false)
    {
        var filter = Builders<Questionnaire>.Filter.Match(t => t.Title.ToLower() == dto.Title.ToLower());

        if (forUpdate) filter = filter.Match(t => t.Id != dto.Id);

        var exist = await db.Find(filter).AnyAsync();
        return exist
            ? new Result<QuestionnaireDto>(new ArgumentException($"Questionnaire with title [{dto.Title}] already exist"))
            : new Result<QuestionnaireDto>(dto);
    }

    public async Task<Result<QuestionnaireDto>> GetAsync(string id, CancellationToken token = default)
    {
        try
        {
            var entity = await _context.Find<Questionnaire>(x => x.MatchId(id)).ExecuteFirstOrDefaultAsync(token);
            return entity == null
                ? new Result<QuestionnaireDto>(new ArgumentException($"Questionnaire with id [{id}] not found"))
                : new Result<QuestionnaireDto>(entity.ToDto());
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to get questionnaire with error: {Message}", e.Message);
            return new Result<QuestionnaireDto>(new Exception("Failed to get questionnaire"));
        }
    }

    public async Task<Result<List<QuestionnaireDto>>> GetAllByIdAsync(string teacherId, CancellationToken token = default)
    {
        try
        {
            var entities = await _context.Find<Questionnaire, QuestionnaireDto>(x => x.Match(t => t.TeacherId == teacherId))
                .Project(x => x.ToDto())
                .Sort(x => x.By(i => i.Title))
                .ExecuteAsync(token);
            return new Result<List<QuestionnaireDto>>(entities.ToList());
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to get questionnaire with error: {Message}", e.Message);
            return new Result<List<QuestionnaireDto>>(new Exception("Failed to get questionnaire"));
        }
    }

    public async Task<Result<QuestionnaireDto>> UpdateAsync(QuestionnaireDto dto, CancellationToken token)
    {
        try
        {
            var validation = dto.Validate();
            if (validation.IsFaulted) return new Result<QuestionnaireDto>(new ArgumentException(validation.ToString()));

            var checkResult = await CheckDuplicateAsync(_context, dto, true);
            if (checkResult.IsFaulted) return checkResult;

            var oldEntity = await _context.Find<Questionnaire>(x => x.MatchId(dto.Id)).ExecuteFirstOrDefaultAsync(token);
            if (oldEntity == null) return new Result<QuestionnaireDto>(new Exception("Questionnaire not found"));

            using var trans = _context.Transaction();

            await _context.LogAsync(oldEntity, token);

            ModifiedBy modifiedBy = dto.TeacherId;
            var result = await _context.Update<Questionnaire, QuestionnaireDto>(x => x.MatchId(dto.Id))
                .Modify(x => x
                    .Set(i => i.Title, dto.Title)
                    .Set(i => i.Description, dto.Description)
                    .Set(i => i.Methodologies, dto.Methodologies.Select(m => m.ToEntity()))
                    .Set(i => i.ModifiedBy, modifiedBy))
                .Project(x => x.ToDto())
                .ExecuteAndGetAsync(token);

            await trans.CommitAsync(token);

            return new Result<QuestionnaireDto>(result);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to save questionnaire with error: {Message}", e.Message);
            return new Result<QuestionnaireDto>(new Exception("Failed to save questionnaire"));
        }
    }

    public async Task<Result<QuestionnaireDto>> DeleteAsync(string id, string teacherId, CancellationToken token = default)
    {
        try
        {
            var oldEntity = await _context.Find<Questionnaire>(x => x.MatchId(id)).ExecuteFirstOrDefaultAsync(token);
            if (oldEntity == null) return new Result<QuestionnaireDto>(new Exception("Questionnaire not found"));

            using var trans = _context.Transaction();

            await _context.LogAsync(oldEntity, token);
            var result = await _context.SoftDelete<Questionnaire>(x => x.MatchId(id)).ExecuteAndGetAsync(teacherId, token: token);

            await trans.CommitAsync(token);

            return new Result<QuestionnaireDto>(result.ToDto());
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to update questionnaire with error: {Message}", e.Message);
            return new Result<QuestionnaireDto>(new Exception("Failed to update teacher"));
        }
    }
}