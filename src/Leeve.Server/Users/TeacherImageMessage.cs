using Leeve.Core;

namespace Leeve.Server.Users;

public sealed class TeacherImageMessage
{
    public required string CallerId { get; init; }
    public required TeacherImageDto TeacherImage { get; init; }
}