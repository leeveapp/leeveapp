using Leeve.Core;
using Leeve.Core.Common;

namespace Leeve.Server.Users;

public class AdminRepository
{
    private readonly IDbContext _context;
    private readonly ILogger _log;

    public AdminRepository(IDbContext context, ILogger log)
    {
        _context = context;
        _log = log;
    }

    internal async Task<Result> SeedAsync()
    {
        try
        {
            var count = await _context.CountEstimatedAsync<Admin>();
            if (count > 0) return new Result();

            var salt = PasswordExt.SaltByte();
            var password = "admin".ComputeHash(salt);

            var user = new Admin
            {
                UserName = "admin",
                Key = Convert.ToBase64String(salt),
                Password = Convert.ToBase64String(password)
            };

            await _context.SaveAsync(user);
            return new Result();
        }
        catch (Exception e)
        {
            _log.Error(e, "Seeding default user failed with error: {Message}", e.Message);
            return new Result(new Exception("Seeding default user failed"));
        }
    }

    internal async Task<Result<AdminDto>> GetByCredentialsAsync(string userName, string password, CancellationToken token)
    {
        try
        {
            var admin = await _context.Find<Admin>(x => x.Match(t => t.UserName == userName)).ExecuteFirstOrDefaultAsync(token);
            if (admin == null) return new Result<AdminDto>(new Exception("Invalid credentials"));

            var valid = admin.ValidatePassword(password);
            return valid ? new Result<AdminDto>(admin.ToDto()) : new Result<AdminDto>(new Exception("Incorrect password"));
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to get admin with error: {Message}", e.Message);
            return new Result<AdminDto>(new Exception("Failed to get admin"));
        }
    }

    internal async Task<Result<AdminDto>> UpdateAsync(AdminDto dto, string oldPassword, CancellationToken token = default)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.UserName))
                return new Result<AdminDto>(new Exception("Username is required"));
            if (string.IsNullOrEmpty(dto.Password))
                return new Result<AdminDto>(new Exception("Password is required"));
            if (string.IsNullOrEmpty(oldPassword))
                return new Result<AdminDto>(new Exception("Old password is required"));

            using var trans = _context.Transaction();

            var oldEntity = await _context.Find<Admin>(x => x.MatchId(dto.Id)).ExecuteSingleAsync(token);

            var validPassword = oldEntity.ValidatePassword(oldPassword);
            if (!validPassword) return new Result<AdminDto>(new Exception("Incorrect password"));

            await _context.LogAsync(oldEntity, token);

            var definitions = GenerateUpdateDefinitions(dto);
            var updated = await _context.Update<Admin>(x => x.MatchId(dto.Id)).Modify(definitions).ExecuteAndGetAsync(token);
            await trans.CommitAsync(token);

            return new Result<AdminDto>(updated.ToDto());
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to update admin with error: {Message}", e.Message);
            return new Result<AdminDto>(new Exception("Failed to update admin"));
        }
    }

    private static IEnumerable<UpdateDefinition<Admin>> GenerateUpdateDefinitions(AdminDto dto)
    {
        var salt = PasswordExt.SaltByte();
        var password = dto.Password.ComputeHash(salt);

        var definitions = new List<UpdateDefinition<Admin>>
        {
            Builders<Admin>.Update
                .Set(i => i.UserName, dto.UserName)
                .Set(i => i.Key, Convert.ToBase64String(salt))
                .Set(i => i.Password, Convert.ToBase64String(password))
        };

        return definitions;
    }
}