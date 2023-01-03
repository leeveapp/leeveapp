using Google.Protobuf;
using Leeve.Core;
using Leeve.Core.Common;

namespace Leeve.Server.Users;

public sealed class TeacherRepository
{
    private readonly IDbContext _context;
    private readonly ILogger _log;

    public TeacherRepository(IDbContext context, ILogger log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Result<TeacherDto>> AddAsync(TeacherDto dto, string adminId, string defaultPassword = "user",
        CancellationToken token = default)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.FirstName))
                return new Result<TeacherDto>(new Exception("First name is required"));
            if (string.IsNullOrEmpty(dto.LastName))
                return new Result<TeacherDto>(new Exception("Last name is required"));

            var checkResult = await CheckDuplicateAsync(_context, dto);
            if (checkResult.IsFaulted) return checkResult;

            var salt = PasswordExt.SaltByte();
            var password = defaultPassword.ComputeHash(salt);

            var entity = new Teacher
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Department = dto.Department,
                UserName = $"{dto.FirstName} {dto.LastName}",
                Key = Convert.ToBase64String(salt),
                Password = Convert.ToBase64String(password),
                CreatedBy = adminId
            };

            await _context.SaveAsync(entity, token);

            return new Result<TeacherDto>(entity.ToDto());
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to save teacher with error: {Message}", e.Message);
            return new Result<TeacherDto>(new Exception("Failed to save teacher"));
        }
    }

    public async Task<Result<TeacherDto>> GetAsync(string id, CancellationToken token = default)
    {
        try
        {
            var dto = await _context.Find<Teacher, TeacherDto>(x => x.MatchId(id))
                .Project(x => x.ToDto()).ExecuteFirstOrDefaultAsync(token);

            if (dto == null) return new Result<TeacherDto>(new Exception("Teacher not found"));

            return new Result<TeacherDto>(dto);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to save teacher with error: {Message}", e.Message);
            return new Result<TeacherDto>(new Exception("Failed to fetch teacher"));
        }
    }

    public async Task<Result<TeacherDto>> GetByCredentialsAsync(string userName, string password, CancellationToken token = default)
    {
        try
        {
            var admin = await _context.Find<Teacher>(x => x.Match(t => t.UserName == userName)).ExecuteFirstOrDefaultAsync(token);
            if (admin == null) return new Result<TeacherDto>(new Exception("User not registered"));

            var valid = admin.ValidatePassword(password);
            return valid ? new Result<TeacherDto>(admin.ToDto()) : new Result<TeacherDto>(new Exception("Incorrect password"));
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to get teacher with error: {Message}", e.Message);
            return new Result<TeacherDto>(new Exception("Failed to get teacher"));
        }
    }

    public async Task<Result<IEnumerable<TeacherDto>>> GetAllAsync(CancellationToken token = default)
    {
        try
        {
            var teachers = await _context.Find<Teacher, TeacherDto>()
                .Project(x => x.ToDto())
                .Sort(x => x.By(i => i.FirstName))
                .Sort(x => x.By(i => i.LastName))
                .ExecuteAsync(token);
            return new Result<IEnumerable<TeacherDto>>(teachers);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to save teacher with error: {Message}", e.Message);
            return new Result<IEnumerable<TeacherDto>>(new Exception("Failed to fetch teachers"));
        }
    }

    private static async Task<Result<TeacherDto>> CheckDuplicateAsync(IDbContext db, TeacherDto dto, bool forUpdate = false)
    {
        var filter = Builders<Teacher>.Filter.Match(t => t.FirstName.ToLower() == dto.FirstName.ToLower() &&
                                                         t.LastName.ToLower() == dto.LastName.ToLower());

        if (forUpdate) filter = filter.Match(t => t.Id != dto.Id);

        var exist = await db.Find(filter).AnyAsync();
        return exist
            ? new Result<TeacherDto>(new ArgumentException("Teacher already exist"))
            : new Result<TeacherDto>(dto);
    }

    public async Task<Result<TeacherDto>> UpdateAsync(TeacherDto dto, string password, CancellationToken token = default)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.FirstName))
                return new Result<TeacherDto>(new Exception("First name is required"));
            if (string.IsNullOrEmpty(dto.LastName))
                return new Result<TeacherDto>(new Exception("Last name is required"));
            if (string.IsNullOrEmpty(dto.Department))
                return new Result<TeacherDto>(new Exception("Department is required"));

            var oldEntity = await _context.Find<Teacher>(x => x.MatchId(dto.Id)).ExecuteFirstOrDefaultAsync(token);
            if (oldEntity == null) return new Result<TeacherDto>(new Exception("Teacher not found"));

            var validPassword = oldEntity.ValidatePassword(password);
            if (!validPassword) return new Result<TeacherDto>(new Exception("Invalid password"));

            var checkResult = await CheckDuplicateAsync(_context, dto, true);
            if (checkResult.IsFaulted) return checkResult;

            using var trans = _context.Transaction();

            await _context.LogAsync(oldEntity, token);

            ModifiedBy modifiedBy = dto.Id;
            var result = await _context.Update<Teacher>(x => x.MatchId(dto.Id))
                .Modify(x => x
                    .Set(i => i.FirstName, dto.FirstName)
                    .Set(i => i.LastName, dto.LastName)
                    .Set(i => i.Department, dto.Department)
                    .Set(i => i.ModifiedBy, modifiedBy))
                .ExecuteAndGetAsync(token);

            await trans.CommitAsync(token);

            return new Result<TeacherDto>(result.ToDto());
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to update teacher with error: {Message}", e.Message);
            return new Result<TeacherDto>(new Exception("Failed to update teacher"));
        }
    }

    public async Task<Result<TeacherDto>> UpdateByAdminAsync(TeacherDto dto, string adminId, CancellationToken token = default)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.FirstName))
                return new Result<TeacherDto>(new Exception("First name is required"));
            if (string.IsNullOrEmpty(dto.LastName))
                return new Result<TeacherDto>(new Exception("Last name is required"));

            var checkResult = await CheckDuplicateAsync(_context, dto, true);
            if (checkResult.IsFaulted) return checkResult;

            using var trans = _context.Transaction();

            var oldEntity = await _context.Find<Teacher>(x => x.MatchId(dto.Id)).ExecuteSingleAsync(token);
            await _context.LogAsync(oldEntity, token);

            ModifiedBy modifiedBy = adminId;
            var result = await _context.Update<Teacher>(x => x.MatchId(dto.Id))
                .Modify(x => x
                    .Set(i => i.FirstName, dto.FirstName)
                    .Set(i => i.LastName, dto.LastName)
                    .Set(i => i.Department, dto.Department)
                    .Set(i => i.ModifiedBy, modifiedBy))
                .ExecuteAndGetAsync(token);

            await trans.CommitAsync(token);

            return new Result<TeacherDto>(result.ToDto());
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to update teacher with error: {Message}", e.Message);
            return new Result<TeacherDto>(new Exception("Failed to update teacher"));
        }
    }

    public async Task<Result<TeacherDto>> UpdateCredentialsAsync(CredentialDto dto, string teacherId,
        CancellationToken token = default)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.UserName))
                return new Result<TeacherDto>(new Exception("User name is required"));
            if (string.IsNullOrEmpty(dto.Password))
                return new Result<TeacherDto>(new Exception("Password is required"));
            if (string.IsNullOrEmpty(dto.OldPassword))
                return new Result<TeacherDto>(new Exception("Old password is required"));

            var oldEntity = await _context.Find<Teacher>(x => x.MatchId(teacherId)).ExecuteFirstOrDefaultAsync(token);
            if (oldEntity == null) return new Result<TeacherDto>(new Exception("Teacher not found"));

            var validPassword = oldEntity.ValidatePassword(dto.OldPassword);
            if (!validPassword) return new Result<TeacherDto>(new Exception("Invalid password"));

            var exist = await _context.Find<Teacher>(x =>
                x.Match(i => i.UserName == dto.UserName && i.Id != teacherId)).AnyAsync(token);
            if (exist) return new Result<TeacherDto>(new Exception("User name already in use"));

            await _context.LogAsync(oldEntity, token);
            using var trans = _context.Transaction();

            var salt = PasswordExt.SaltByte();
            var password = dto.Password.ComputeHash(salt);

            ModifiedBy modifiedBy = teacherId;
            var result = await _context.Update<Teacher>(x => x.MatchId(teacherId))
                .Modify(x => x
                    .Set(i => i.UserName, dto.UserName)
                    .Set(i => i.Password, Convert.ToBase64String(password))
                    .Set(i => i.Key, Convert.ToBase64String(salt))
                    .Set(i => i.ModifiedBy, modifiedBy))
                .ExecuteAndGetAsync(token);

            await trans.CommitAsync(token);

            return new Result<TeacherDto>(result.ToDto());
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to update teacher with error: {Message}", e.Message);
            return new Result<TeacherDto>(new Exception("Failed to update teacher"));
        }
    }

    public async Task<Result<TeacherImageDto>> UpdateImageAsync(TeacherImageDto dto, string userId,
        CancellationToken token = default)
    {
        try
        {
            ModifiedBy modifiedBy = userId;
            var entity = await _context.Update<Teacher>(x => x.MatchId(dto.Id))
                .Modify(x => x
                    .Set(i => i.Image, dto.Image.ToByteArray())
                    .Set(i => i.ModifiedBy, modifiedBy))
                .ExecuteAndGetAsync(token);

            var image = ByteString.CopyFrom(entity.Image);

            return new Result<TeacherImageDto>(new TeacherImageDto { Id = entity.Id, Image = image });
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to update teacher with error: {Message}", e.Message);
            return new Result<TeacherImageDto>(new Exception("Failed to update teacher"));
        }
    }

    public async Task<Result<TeacherDto>> DeleteAsync(string teacherId, string adminId, CancellationToken token = default)
    {
        try
        {
            var oldEntity = await _context.Find<Teacher>(x => x.MatchId(teacherId)).ExecuteFirstOrDefaultAsync(token);
            if (oldEntity == null) return new Result<TeacherDto>(new Exception("Teacher not found"));

            using var trans = _context.Transaction();

            await _context.LogAsync(oldEntity, token);
            var result = await _context.SoftDelete<Teacher>(x => x.MatchId(teacherId)).ExecuteAndGetAsync(adminId, token: token);

            await trans.CommitAsync(token);

            return new Result<TeacherDto>(result.ToDto());
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to update teacher with error: {Message}", e.Message);
            return new Result<TeacherDto>(new Exception("Failed to update teacher"));
        }
    }
}