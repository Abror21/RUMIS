using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;

namespace Izm.Rumis.Application.Mappers
{
    internal static class RoleMapper
    {
        public static Role Map(RoleEditDto item, Role entity)
        {
            entity.Code = item.Code;
            entity.Name = item.Name;

            return entity;
        }
    }
}
