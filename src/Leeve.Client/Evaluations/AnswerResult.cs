namespace Leeve.Client.Evaluations;

public sealed class AnswerResult
{
    public required string Title { get; set; }
    public int Count { get; set; }
    public float WeightedScore { get; set; }
}