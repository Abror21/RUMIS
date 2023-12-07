using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;

namespace Izm.Rumis.Application.Mappers
{
    internal static class SupervisorMapper
    {
        public static Supervisor Map(SupervisorCreateDto dto, Supervisor entity)
        {
            entity.Code = dto.Code;
            entity.Name = dto.Name;

            return entity;
        }

        public static Supervisor Map(SupervisorUpdateDto dto, Supervisor entity)
        {
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.IsActive = dto.IsActive;

            return entity;
        }
    }
}
