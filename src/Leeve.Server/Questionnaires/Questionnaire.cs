using Leeve.Server.Common;

namespace Leeve.Server.Questionnaires;

[Collection("questionnaires")]
public sealed class Questionnaire : BaseDocument
{
    [ObjectId]
    public required string TeacherId { get; set; }

    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public required IEnumerable<Methodology> Methodologies { get; set; }
}