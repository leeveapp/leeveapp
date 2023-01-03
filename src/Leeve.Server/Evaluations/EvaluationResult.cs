using Leeve.Server.Common;

namespace Leeve.Server.Evaluations;

[Collection("evaluationResults")]
public sealed class EvaluationResult : BaseDocument
{
    [ObjectId]
    public required string EvaluationId { get; set; }

    [ObjectId]
    public required string TeacherId { get; set; }

    public required Evaluation Evaluation { get; set; }
}