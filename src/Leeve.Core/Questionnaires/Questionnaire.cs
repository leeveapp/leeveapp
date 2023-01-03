namespace Leeve.Core.Questionnaires;

public sealed class Questionnaire
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IEnumerable<Methodology> Methodologies { get; set; } = null!;
    public string TeacherId { get; init; } = string.Empty;
}

public sealed class Methodology
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IEnumerable<Question> Questions { get; init; } = null!;
}

public sealed class Question
{
    public string Title { get; init; } = string.Empty;
    public IEnumerable<Answer> Answers { get; init; } = null!;
}

public sealed class Answer
{
    public string Title { get; init; } = string.Empty;
    public float Weight { get; init; }
    public bool Selected { get; init; }
}