using Google.Protobuf;
using Leeve.Core;

namespace Leeve.Server.Users;

public static class TeacherExtensions
{
    public static TeacherDto ToDto(this Teacher entity)
    {
        var result = new TeacherDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Department = entity.Department,
            FullName = $"{entity.FirstName} {entity.LastName}",
            Image = entity.Image == null ? ByteString.Empty : ByteString.CopyFrom(entity.Image)
        };

        return result;
    }
}