namespace Leeve.Server.Users;

public interface IUser
{
    string UserName { get; init; }
    string Key { get; init; }
    string Password { get; init; }
}