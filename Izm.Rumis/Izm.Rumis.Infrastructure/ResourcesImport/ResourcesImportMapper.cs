using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.ResourceImport.Models;

namespace Izm.Rumis.Infrastructure.ResourceImport
{
    internal static class ResourcesImportMapper
    {
        public static Resource Map(ResourcesImportData data, Resource entity)
        {
            entity.ResourceName = data.ResourceName;
            entity.ModelIdentifier = data.ModelIdentifier;
            entity.AcquisitionsValue = data.AcquisitionsValue;
            entity.ManufactureYear = data.ManufactureYear;
            entity.InventoryNumber = data.InventoryNumber;
            entity.SerialNumber = data.SerialNumber;
            entity.ResourceSubTypeId = data.ResourceSubTypeId;
            entity.TargetGroupId = data.TargetGroupId;
            entity.UsagePurposeTypeId = data.UsagePurposeTypeId;
            entity.AcquisitionTypeId = data.AcquisitionTypeId;
            entity.ManufacturerId = data.ManufacturerId;
            entity.ModelNameId = data.ModelNameId;
            entity.SocialSupportResource = data.SocialSupportResource;
            entity.ResourceLocationId = data.ResourceLocationId;

            return entity;
        }
    }
}
