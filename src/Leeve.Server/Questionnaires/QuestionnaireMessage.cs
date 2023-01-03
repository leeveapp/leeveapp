using Leeve.Core;

namespace Leeve.Server.Questionnaires;

public sealed class QuestionnaireMessage
{
    public required string Action { get; init; }
    public required string CallerId { get; init; }
    public required QuestionnaireDto Questionnaire { get; init; }
}