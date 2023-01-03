using Leeve.Core.Questionnaires;

namespace Leeve.Core.Evaluations;

public sealed class Evaluation
{
    public string Id { get; init; } = string.Empty;
    public string TeacherId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedOn { get; init; }
    public Questionnaire Questionnaire { get; init; } = null!;
    public bool IsActive { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Responses { get; set; }
}