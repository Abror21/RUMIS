using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Izm.Rumis.Application.Mappers
{
    internal static class DocumentTemplateMapper
    {
        public static DocumentTemplate Map(DocumentTemplateEditDto dto, DocumentTemplate entity)
        {
            entity.Code = Utility.SanitizeCode(dto.Code);
            entity.Title = dto.Title;
            entity.ValidFrom = dto.ValidFrom;
            entity.ValidTo = dto.ValidTo;
            entity.Hyperlink = dto.Hyperlink;
            entity.ResourceTypeId = dto.ResourceTypeId;
            entity.SupervisorId = dto.SupervisorId;

            return entity;
        }
    }
}
