using Leeve.Core;

namespace Leeve.Server.Users;

public static class AdminExtensions
{
    public static bool ValidatePassword(this IUser user, string password)
    {
        var salt = Convert.FromBase64String(user.Key);
        var currentPassword = Convert.ToBase64String(password.ComputeHash(salt));

        return user.Password == currentPassword;
    }

    public static AdminDto ToDto(this Admin admin)
    {
        var result = new AdminDto
        {
            Id = admin.Id,
            UserName = admin.UserName
        };

        return result;
    }
}