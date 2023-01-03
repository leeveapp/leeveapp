namespace Leeve.Server.Questionnaires;

public sealed class Answer
{
    public required string Title { get; set; }
    public float Weight { get; set; }
    public bool Selected { get; set; }
}