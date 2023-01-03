namespace Leeve.Core.Evaluations;

public sealed class EvaluationProcessStartedMessage
{
    public required Evaluation Evaluation { get; init; }
}