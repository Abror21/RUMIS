using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    public static class ClassifierMapper
    {
        public static Expression<Func<Classifier, ClassifierModel>> Project()
        {
            return t => new ClassifierModel
            {
                Code = t.Code,
                Id = t.Id,
                Payload = t.Payload,
                SortOrder = t.SortOrder,
                Type = t.Type,
                Value = t.Value,
                IsDisabled = t.IsDisabled,
                IsRequired = t.IsRequired,
                ActiveFrom = t.ActiveFrom,
                ActiveTo = t.ActiveTo,
                PermissionType = t.PermissionType,
                EducationalInstitutionId = t.EducationalInstitutionId,
                SupervisorId = t.SupervisorId
            };
        }

        public static ClassifierCreateDto Map(ClassifierCreateModel model, ClassifierCreateDto dto)
        {
            dto.Type = model.Type;
            dto.PermissionType = model.PermissionType;

            Map(model, (ClassifierEditDto)dto);

            return dto;
        }

        public static TDto Map<TModel, TDto>(TModel model, TDto dto)
            where TModel : ClassifierEditModel
            where TDto : ClassifierEditDto
        {
            dto.Code = model.Code;
            dto.Payload = model.Payload;
            dto.Value = model.Value;
            dto.SortOrder = model.SortOrder;
            dto.IsDisabled = model.IsDisabled;
            dto.ActiveFrom = model.ActiveFrom;
            dto.ActiveTo = model.ActiveTo;
            dto.EducationalInstitutionId = model.EducationalInstitutionId;
            dto.SupervisorId = model.SupervisorId;

            return dto;
        }
    }
}
