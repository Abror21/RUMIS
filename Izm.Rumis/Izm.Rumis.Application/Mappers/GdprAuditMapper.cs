using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;

namespace Izm.Rumis.Application.Mappers
{
    internal static class GdprAuditMapper
    {
        public static GdprAudit Map(GdprAuditTraceDto item, GdprAudit entity)
        {
            entity.Action = item.Action;
            entity.ActionData = item.ActionData;
            entity.DataOwnerId = item.DataOwnerId;
            entity.DataOwnerPrivatePersonalIdentifier = item.DataOwnerPrivatePersonalIdentifier;

            return entity;
        }
    }
}
