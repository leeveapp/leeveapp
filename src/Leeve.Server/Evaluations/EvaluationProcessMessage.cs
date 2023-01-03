namespace Leeve.Server.Evaluations;

public sealed class EvaluationProcessMessage
{
    public required string Action { get; init; }
    public required string CallerId { get; init; }
    public required string EvaluationId { get; init; }
    public required string EvaluationCode { get; init; }
}