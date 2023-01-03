namespace Leeve.Core.Evaluations;

public sealed class EvaluationModifiedMessage
{
    public Evaluation? Evaluation { get; set; }
    public string Action { get; set; } = string.Empty;
}