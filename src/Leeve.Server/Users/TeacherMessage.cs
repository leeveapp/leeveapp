using Leeve.Core;

namespace Leeve.Server.Users;

public sealed class TeacherMessage
{
    public required string Action { get; init; }
    public required string CallerId { get; init; }
    public required TeacherDto Teacher { get; init; }
}