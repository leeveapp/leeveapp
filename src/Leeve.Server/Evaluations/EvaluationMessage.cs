using Leeve.Core;

namespace Leeve.Server.Evaluations;

public sealed class EvaluationMessage
{
    public required string Action { get; init; }
    public required string CallerId { get; init; }
    public required EvaluationDto Evaluation { get; init; }
}