namespace Leeve.Core.Evaluations;

public sealed class EvaluationRequestedMessage
{
    public required Evaluation Evaluation { get; init; }
}