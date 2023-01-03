using Leeve.Core.Common;

namespace Leeve.Core.Questionnaires;

public static class QuestionnaireValidator
{
    public static Result Validate(this QuestionnaireDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return new Result(new ArgumentException("Questionnaire's title is required"));

        if (dto.Methodologies.Count == 0)
            return new Result(new ArgumentException("Methodologies are required"));

        foreach (var methodology in dto.Methodologies)
        {
            var validation = methodology.Validate();
            if (validation.IsFaulted) return validation;
        }

        return new Result();
    }

    private static Result Validate(this MethodologyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title)) return new Result(new ArgumentException("Methodology name is required"));

        if (dto.Questions.Count == 0) return new Result(new ArgumentException("Questions are required"));

        foreach (var question in dto.Questions)
        {
            var validation = question.Validate();
            if (validation.IsFaulted) return validation;
        }

        return new Result();
    }

    private static Result Validate(this QuestionDto dto)
    {
        if (string.IsNullOrEmpty(dto.Title)) return new Result(new ArgumentException("Question text is required"));

        if (dto.Answers.Count == 0) return new Result(new ArgumentException("Answers are required"));

        foreach (var answer in dto.Answers)
        {
            var validation = answer.Validate();
            if (validation.IsFaulted) return validation;
        }

        return new Result();
    }

    private static Result Validate(this AnswerDto dto)
    {
        if (string.IsNullOrEmpty(dto.Title))
            return new Result(new ArgumentException("Answer is required"));

        if (dto.Weight == 0)
            return new Result(new ArgumentException("Answer's weighted score must be a number greater than zero"));

        return new Result();
    }

    public static Result ValidateAnswers(this QuestionnaireDto dto)
    {
        if (dto.Methodologies.Count == 0)
            return new Result(new ArgumentException("Invalid evaluation: no available methodologies"));

        foreach (var methodology in dto.Methodologies)
        {
            var validation = methodology.ValidateAnswers();
            if (validation.IsFaulted) return validation;
        }

        return new Result();
    }

    private static Result ValidateAnswers(this MethodologyDto dto)
    {
        if (dto.Questions.Count == 0)
            return new Result(new ArgumentException("Invalid evaluation: no available questions"));

        foreach (var question in dto.Questions)
        {
            var validation = question.ValidateAnswers();
            if (validation.IsFaulted) return validation;
        }

        return new Result();
    }

    private static Result ValidateAnswers(this QuestionDto dto)
    {
        if (dto.Answers.Count == 0)
            return new Result(new ArgumentException("Invalid evaluation: no answers"));

        var selection = dto.Answers.Count(a => a.Selected);
        if (selection == 0)
            return new Result(new ArgumentException("There are unanswered questions"));

        return new Result();
    }
}