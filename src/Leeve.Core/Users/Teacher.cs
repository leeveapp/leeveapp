namespace Leeve.Core.Users;

public class Teacher
{
    public string Id { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public byte[]? Image { get; set; }
}