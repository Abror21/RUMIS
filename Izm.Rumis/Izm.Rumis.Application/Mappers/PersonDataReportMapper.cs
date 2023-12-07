using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;

namespace Izm.Rumis.Application.Mappers
{
    internal static class PersonDataReportMapper
    {
        public static PersonDataReport Map(PersonDataReportGenerateDto item, PersonDataReport entity)
        {
            entity.Notes = item.Notes;
            entity.DataHandlerPrivatePersonalIdentifier = item.DataHandlerPrivatePersonalIdentifier;
            entity.DataOwnerPrivatePersonalIdentifier = item.DataOwnerPrivatePersonalIdentifier;
            entity.ReasonId = item.ReasonId;

            return entity;
        }
    }
}
