using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;

namespace Izm.Rumis.Application.Mappers
{
    internal static class UserProfileMapper
    {
        public static UserProfile Map(UserProfileEditDto dto, UserProfile entity)
        {
            entity.ConfigurationInfo = dto.ConfigurationInfo;
            entity.Email = dto.Email;
            entity.InstitutionId = dto.InstitutionId;
            entity.Job = dto.Job;
            entity.Notes = dto.Notes;
            entity.PhoneNumber = dto.PhoneNumber;
            entity.ProfileCreationDocumentDate = dto.ProfileCreationDocumentDate;
            entity.ProfileCreationDocumentNumber = dto.ProfileCreationDocumentNumber;
            entity.UserId = dto.UserId;

            return entity;
        }
    }
}
