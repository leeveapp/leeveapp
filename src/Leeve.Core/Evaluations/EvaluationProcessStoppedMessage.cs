namespace Leeve.Core.Evaluations;

public sealed class EvaluationProcessStoppedMessage
{
    public required Evaluation Evaluation { get; init; }
}