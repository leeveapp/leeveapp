using Leeve.Core.Common;
using Leeve.Core.Questionnaires;

namespace Leeve.Core.Evaluations;

public static class EvaluationExtensions
{
    public static Evaluation ToEntity(this EvaluationDto dto) =>
        new()
        {
            Id = dto.Id,
            TeacherId = dto.TeacherId,
            Title = dto.Title,
            Description = dto.Description,
            Code = dto.Code,
            CreatedOn = dto.CreatedOn.ToDateTime().ToLocalDateTime(),
            Responses = dto.Responses,
            IsActive = dto.IsActive,
            Questionnaire = dto.Questionnaire.ToEntity()
        };

    public static EvaluationDto ToDto(this Evaluation entity) =>
        new()
        {
            Id = entity.Id,
            TeacherId = entity.TeacherId,
            Title = entity.Title,
            Description = entity.Description,
            Code = entity.Code,
            Responses = entity.Responses,
            IsActive = entity.IsActive,
            Questionnaire = entity.Questionnaire.ToDto()
        };
}