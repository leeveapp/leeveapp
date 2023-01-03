namespace Leeve.Server.Questionnaires;

public sealed class Question
{
    public required string Title { get; set; }
    public required IEnumerable<Answer> Answers { get; set; }
}