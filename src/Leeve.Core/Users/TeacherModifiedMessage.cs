namespace Leeve.Core.Users;

public sealed class TeacherModifiedMessage
{
    public Teacher? Teacher { get; init; }
    public string Action { get; set; } = string.Empty;
}