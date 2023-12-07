using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;

namespace Izm.Rumis.Application.Mappers
{
    internal static class ApplicationResourceMapper
    {
        public static ApplicationResource Map(ApplicationResourceCreateDto dto, ApplicationResource entity)
        {
            entity.ApplicationId = dto.ApplicationId;
            entity.AssignedResourceId = dto.AssignedResourceId;
            entity.Notes = dto.Notes;

            return entity;
        }

        public static ApplicationResource Map(ApplicationResourceReturnEditDto dto, ApplicationResource entity)
        {
            entity.AssignedResource.ResourceStatusId = dto.ResourceStatusId;
            entity.ReturnResourceStateId = dto.ReturnResourceStateId;
            entity.ReturnResourceDate = dto.ReturnResourceDate;
            entity.AssignedResource.Notes = dto.Notes;
            entity.Notes = dto.Notes;

            return entity;
        }
    }
}
