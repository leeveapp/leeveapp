using Leeve.Core;

namespace Leeve.Server.Questionnaires;

public static class QuestionnaireExtensions
{
    public static QuestionnaireDto ToDto(this Questionnaire entity)
    {
        var result = new QuestionnaireDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            TeacherId = entity.TeacherId,
            Methodologies = { entity.Methodologies.Select(x => x.ToDto()) }
        };

        return result;
    }

    public static Questionnaire ToEntity(this QuestionnaireDto dto)
    {
        var result = new Questionnaire
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Methodologies = dto.Methodologies.Select(x => x.ToEntity()),
            TeacherId = dto.TeacherId
        };

        return result;
    }

    private static MethodologyDto ToDto(this Methodology entity)
    {
        var result = new MethodologyDto
        {
            Title = entity.Title,
            Description = entity.Description,
            Questions = { entity.Questions.Select(x => x.ToDto()) }
        };

        return result;
    }

    public static Methodology ToEntity(this MethodologyDto dto)
    {
        var result = new Methodology
        {
            Title = dto.Title,
            Description = dto.Description,
            Questions = dto.Questions.Select(x => x.ToEntity())
        };

        return result;
    }

    private static QuestionDto ToDto(this Question entity)
    {
        var result = new QuestionDto
        {
            Title = entity.Title,
            Answers = { entity.Answers.Select(x => x.ToDto()) }
        };

        return result;
    }

    private static Question ToEntity(this QuestionDto dto)
    {
        var result = new Question
        {
            Title = dto.Title,
            Answers = dto.Answers.Select(x => x.ToEntity())
        };

        return result;
    }

    private static AnswerDto ToDto(this Answer entity)
    {
        var result = new AnswerDto
        {
            Title = entity.Title,
            Selected = entity.Selected,
            Weight = entity.Weight
        };

        return result;
    }

    private static Answer ToEntity(this AnswerDto dto)
    {
        var result = new Answer
        {
            Title = dto.Title,
            Selected = dto.Selected,
            Weight = dto.Weight
        };

        return result;
    }
}