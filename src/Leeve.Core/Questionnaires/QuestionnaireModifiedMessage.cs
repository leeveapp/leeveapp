namespace Leeve.Core.Questionnaires;

public sealed class QuestionnaireModifiedMessage
{
    public Questionnaire? Questionnaire { get; set; }
    public string Action { get; set; } = string.Empty;
}