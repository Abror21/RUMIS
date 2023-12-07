using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    public static class RoleMapper
    {
        public static Expression<Func<Role, RoleResponse>> Project()
        {
            return t => new RoleResponse
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Permissions = t.Permissions.Select(n => n.Value)
            };
        }

        public static RoleEditDto Map(RoleEditRequest model, RoleEditDto dto)
        {
            dto.Code = model.Code;
            dto.Name = model.Name;
            dto.Permissions = model.Permissions;

            return dto;
        }
    }
}
