using Google.Protobuf;

namespace Leeve.Core.Users;

public static class TeacherExtensions
{
    public static Teacher ToEntity(this TeacherDto dto) =>
        new()
        {
            Id = dto.Id,
            Department = dto.Department,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Image = dto.Image.ToByteArray()
        };

    public static TeacherDto ToDto(this Teacher entity)
    {
        var dto = new TeacherDto
        {
            Id = entity.Id,
            Department = entity.Department,
            FirstName = entity.FirstName,
            LastName = entity.LastName
        };

        if (entity.Image != null)
            dto.Image = ByteString.CopyFrom(entity.Image);

        return dto;
    }
}