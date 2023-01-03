using Leeve.Server.Common;

namespace Leeve.Server.Users;

[Collection("teachers")]
public sealed class Teacher : BaseDocument, IUser
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Department { get; init; }
    public required string UserName { get; init; }
    public required string Key { get; init; }
    public required string Password { get; init; }
    public byte[]? Image { get; init; }
}