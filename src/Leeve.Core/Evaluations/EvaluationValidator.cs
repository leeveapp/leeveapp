using Leeve.Core.Common;
using Leeve.Core.Questionnaires;

namespace Leeve.Core.Evaluations;

public static class EvaluationValidator
{
    public static Result Validate(this EvaluationDto dto)
    {
        if (string.IsNullOrEmpty(dto.Title))
            return new Result(new ArgumentException("Title is required"));
        if (string.IsNullOrEmpty(dto.TeacherId))
            return new Result(new ArgumentException("No teacher provided"));

        return dto.Questionnaire.Validate();
    }
}