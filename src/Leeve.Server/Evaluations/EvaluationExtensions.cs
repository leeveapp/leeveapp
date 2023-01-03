using Google.Protobuf.WellKnownTypes;
using Leeve.Core;
using Leeve.Server.Questionnaires;

namespace Leeve.Server.Evaluations;

public static class EvaluationExtensions
{
    public static EvaluationDto ToDto(this Evaluation entity)
    {
        var result = new EvaluationDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Questionnaire = entity.Questionnaire.ToDto(),
            TeacherId = entity.TeacherId
        };

        if (entity.CreatedOn != null)
            result.CreatedOn = Timestamp.FromDateTime(entity.CreatedOn.Value);

        return result;
    }

    public static EvaluationDto ToDto(this EvaluationResult entity)
    {
        var result = new EvaluationDto
        {
            Id = entity.Evaluation.Id,
            Title = entity.Evaluation.Title,
            Description = entity.Evaluation.Description,
            Questionnaire = entity.Evaluation.Questionnaire.ToDto(),
            TeacherId = entity.TeacherId
        };

        if (entity.CreatedOn != null)
            result.CreatedOn = Timestamp.FromDateTime(entity.CreatedOn.Value);

        return result;
    }

    public static Evaluation ToEntity(this EvaluationDto dto)
    {
        var result = new Evaluation
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Questionnaire = dto.Questionnaire.ToEntity(),
            TeacherId = dto.TeacherId
        };

        return result;
    }
}