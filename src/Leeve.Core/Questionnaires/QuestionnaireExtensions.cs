using System.Collections.ObjectModel;

namespace Leeve.Core.Questionnaires;

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
            Methodologies = { entity.Methodologies.Select(e => e.ToDto()) }
        };

        return result;
    }

    public static Questionnaire ToEntity(this QuestionnaireDto dto)
    {
        var methodologies = dto.Methodologies.Select(e => e.ToEntity());
        var result = new Questionnaire
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            TeacherId = dto.TeacherId,
            Methodologies = new ObservableCollection<Methodology>(methodologies)
        };

        return result;
    }

    private static MethodologyDto ToDto(this Methodology entity)
    {
        var result = new MethodologyDto
        {
            Title = entity.Title,
            Description = entity.Description,
            Questions = { entity.Questions.Select(e => e.ToDto()) }
        };

        return result;
    }

    public static Methodology ToEntity(this MethodologyDto dto)
    {
        var questions = dto.Questions.Select(e => e.ToEntity());
        var result = new Methodology
        {
            Title = dto.Title,
            Description = dto.Description,
            Questions = new ObservableCollection<Question>(questions)
        };

        return result;
    }

    private static QuestionDto ToDto(this Question entity)
    {
        var result = new QuestionDto
        {
            Title = entity.Title,
            Answers = { entity.Answers.Select(e => e.ToDto()) }
        };

        return result;
    }

    private static Question ToEntity(this QuestionDto dto)
    {
        var answers = dto.Answers.Select(e => e.ToEntity());
        var result = new Question
        {
            Title = dto.Title,
            Answers = new ObservableCollection<Answer>(answers)
        };

        return result;
    }

    private static AnswerDto ToDto(this Answer entity)
    {
        var result = new AnswerDto
        {
            Title = entity.Title,
            Weight = entity.Weight,
            Selected = entity.Selected
        };

        return result;
    }

    private static Answer ToEntity(this AnswerDto dto)
    {
        var result = new Answer
        {
            Title = dto.Title,
            Weight = dto.Weight,
            Selected = dto.Selected
        };

        return result;
    }
}