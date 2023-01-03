using Leeve.Server.Common;

namespace Leeve.Server.Users;

[Collection("admins")]
public sealed class Admin : BaseDocument, IUser
{
    public required string UserName { get; init; }
    public string Password { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
}