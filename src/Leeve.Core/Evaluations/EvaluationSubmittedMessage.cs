namespace Leeve.Core.Evaluations;

public sealed class EvaluationSubmittedMessage
{
    public required Evaluation Evaluation { get; init; }
}