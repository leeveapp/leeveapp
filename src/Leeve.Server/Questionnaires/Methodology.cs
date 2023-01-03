namespace Leeve.Server.Questionnaires;

public sealed class Methodology
{
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public required IEnumerable<Question> Questions { get; set; }
}