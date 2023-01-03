using Leeve.Server.Common;
using Leeve.Server.Questionnaires;

namespace Leeve.Server.Evaluations;

[Collection("evaluations")]
public sealed class Evaluation : BaseDocument
{
    [ObjectId]
    public required string TeacherId { get; set; }

    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public required Questionnaire Questionnaire { get; set; }
}