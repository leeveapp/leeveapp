namespace Leeve.Server.Evaluations;

public sealed class EvaluationSubmitMessage
{
    public required int Count { get; init; }
    public required string CallerId { get; init; }
    public required string EvaluationId { get; init; }
}