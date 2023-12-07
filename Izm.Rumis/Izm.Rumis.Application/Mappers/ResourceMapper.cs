using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System.Linq;

namespace Izm.Rumis.Application.Mappers
{
    internal static class ResourceMapper
    {
        public static Resource Map(ResourceUpdateDto dto, Resource entity)
        {
            return Map((ResourceEditDto)dto, entity);
        }

        public static Resource Map(ResourceCreateDto dto, Resource entity)
        {
            entity = Map((ResourceEditDto)dto, entity);
            entity.EducationalInstitutionId = dto.EducationalInstitutionId;
            entity.ResourceSubTypeId = dto.ResourceSubTypeId;
            entity.ResourceParameters = dto.ResourceParameters.Select(t => new ResourceParameter
            {
                Value = t.Value,
                ParameterId = t.ParameterId
            }).ToArray();

            return entity;
        }

        private static Resource Map(ResourceEditDto dto, Resource entity)
        {
            entity.ResourceName = dto.ResourceName;
            entity.ModelIdentifier = dto.ModelIdentifier;
            entity.AcquisitionsValue = dto.AcquisitionsValue;
            entity.ManufactureYear = dto.ManufactureYear;
            entity.InventoryNumber = dto.InventoryNumber;
            entity.SerialNumber = dto.SerialNumber;
            entity.Notes = dto.Notes;
            entity.ResourceGroupId = dto.ResourceGroupId;
            entity.ResourceStatusId = dto.ResourceStatusId;
            entity.ResourceLocationId = dto.ResourceLocationId;
            entity.TargetGroupId = dto.TargetGroupId;
            entity.UsagePurposeTypeId = dto.UsagePurposeTypeId;
            entity.AcquisitionTypeId = dto.AcquisitionTypeId;
            entity.ManufacturerId = dto.ManufacturerId;
            entity.ModelNameId = dto.ModelNameId;
            entity.SocialSupportResource = dto.SocialSupportResource;

            return entity;
        }
    }
}
