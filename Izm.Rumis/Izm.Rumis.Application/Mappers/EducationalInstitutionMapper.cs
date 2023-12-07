using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;

namespace Izm.Rumis.Application.Mappers
{
    internal static class EducationalInstitutionMapper
    {
        public static EducationalInstitution Map(EducationalInstitutionCreateDto dto, EducationalInstitution entity)
        {
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.Address = dto.Address;
            entity.City = dto.City;
            entity.District = dto.District;
            entity.Email = dto.Email;
            entity.PhoneNumber = dto.PhoneNumber;
            entity.Municipality = dto.Municipality;
            entity.Village = dto.Village;
            entity.SupervisorId = dto.SupervisorId;

            return entity;
        }

        public static EducationalInstitution Map(EducationalInstitutionUpdateDto dto, EducationalInstitution entity)
        {
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.Address = dto.Address;
            entity.City = dto.City;
            entity.District = dto.District;
            entity.Email = dto.Email;
            entity.PhoneNumber = dto.PhoneNumber;
            entity.Municipality = dto.Municipality;
            entity.Village = dto.Village;
            entity.SupervisorId = dto.SupervisorId;
            entity.StatusId = dto.StatusId;

            return entity;
        }
    }
}
