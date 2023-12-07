using Izm.Rumis.Api.Common;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Models
{
    public class ResourceResponse
    {
        public Guid Id { get; set; }
        public string ResourceNumber { get; set; }
        public string ResourceName { get; set; }
        public string ModelIdentifier { get; set; }
        public decimal AcquisitionsValue { get; set; }
        public int ManufactureYear { get; set; }
        public string InventoryNumber { get; set; }
        public string SerialNumber { get; set; }
        public bool? SocialSupportResource { get; set; }
        public string ResourceStatusHistory { get; set; }
        public string Notes { get; set; }
        public EducationalInstitutionData EducationalInstitution { get; set; }
        public ClassifierData ResourceSubType { get; set; }
        public ClassifierData ResourceType { get; set; }
        public ClassifierData ResourceGroup { get; set; }
        public ClassifierData ResourceStatus { get; set; }
        public ClassifierData ResourceLocation { get; set; }
        public ClassifierData TargetGroup { get; set; }
        public ClassifierData UsagePurposeType { get; set; }
        public ClassifierData AcquisitionType { get; set; }
        public ClassifierData Manufacturer { get; set; }
        public ClassifierData ModelName { get; set; }
        public IEnumerable<ResourceParameterData> ResourceParameters { get; set; }
        public SupervisorData Supervisor { get; set; }

        public class ResourceParameterData
        {
            public Guid Id { get; set; }
            public string Value { get; set; }
            public ClassifierData Parameter { get; set; }
        }

        public class EducationalInstitutionData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }

            public static Expression<Func<Classifier, ClassifierData>> Project()
            {
                return t => new ClassifierData
                {
                    Id = t.Id,
                    Code = t.Code,
                    Value = t.Value
                };
            }
        }
        public class SupervisorData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }

    public class ResourceIntermediateResponse
    {
        public Guid Id { get; set; }
        public string ResourceNumber { get; set; }
        public string ResourceName { get; set; }
        public string ModelIdentifier { get; set; }
        public decimal AcquisitionsValue { get; set; }
        public int ManufactureYear { get; set; }
        public string InventoryNumber { get; set; }
        public string SerialNumber { get; set; }
        public bool? SocialSupportResource { get; set; }
        public string ResourceStatusHistory { get; set; }
        public string Notes { get; set; }
        public EducationalInstitution EducationalInstitution { get; set; }
        public Classifier ResourceSubType { get; set; }
        public Classifier ResourceType { get; set; }
        public Classifier ResourceGroup { get; set; }
        public Classifier ResourceStatus { get; set; }
        public Classifier ResourceLocation { get; set; }
        public Classifier TargetGroup { get; set; }
        public Classifier UsagePurposeType { get; set; }
        public Classifier AcquisitionType { get; set; }
        public Classifier Manufacturer { get; set; }
        public Classifier ModelName { get; set; }
        public IEnumerable<ResourceParameter> ResourceParameters { get; set; }
        public Supervisor Supervisor { get; set; }
    }


    public class ResourceFilterRequest : Filter<Resource>
    {
        public string ResourceNumber { get; set; }
        public string ResourceName { get; set; }
        public string ModelIdentifier { get; set; }
        public decimal? AcquisitionsValue { get; set; }
        public int? ManufactureYear { get; set; }
        public string InventoryNumber { get; set; }
        public string SerialNumber { get; set; }
        public string ResourceStatusHistory { get; set; }
        public string Notes { get; set; }
        public IEnumerable<Guid> ResourceSubTypeIds { get; set; }
        public IEnumerable<Guid> ResourceGroupIds { get; set; }
        public IEnumerable<Guid> ResourceStatusIds { get; set; }
        public IEnumerable<Guid> ResourceLocationIds { get; set; }
        public IEnumerable<Guid> TargetGroupIds { get; set; }
        public IEnumerable<Guid> UsagePurposeTypeIds { get; set; }
        public IEnumerable<Guid> AcquisitionTypeIds { get; set; }
        public IEnumerable<Guid> ManufacturerIds { get; set; }
        public IEnumerable<Guid> ModelNameIds { get; set; }
        public IEnumerable<int> EducationalInstitutionIds { get; set; }
        public IEnumerable<int> SupervisorIds { get; set; }
        public bool? SocialSupportResource { get; set; }

        protected override Expression<Func<Resource, bool>>[] GetFilters()
        {
            var filters = new List<Expression<Func<Resource, bool>>>();

            if (!string.IsNullOrEmpty(ResourceNumber))
                filters.Add(t => t.ResourceNumber.Contains(ResourceNumber));

            if (!string.IsNullOrEmpty(ResourceName))
                filters.Add(t => t.ResourceName.Contains(ResourceName));

            if (!string.IsNullOrEmpty(ModelIdentifier))
                filters.Add(t => t.ModelIdentifier.Contains(ModelIdentifier));

            if (AcquisitionsValue.HasValue)
                filters.Add(t => t.AcquisitionsValue == AcquisitionsValue);

            if (ManufactureYear.HasValue)
                filters.Add(t => t.ManufactureYear == ManufactureYear);

            if (!string.IsNullOrEmpty(InventoryNumber))
                filters.Add(t => t.InventoryNumber.Contains(InventoryNumber));

            if (!string.IsNullOrEmpty(SerialNumber))
                filters.Add(t => t.SerialNumber.Contains(SerialNumber));

            if (!string.IsNullOrEmpty(ResourceStatusHistory))
                filters.Add(t => t.ResourceStatusHistory.Contains(ResourceStatusHistory));

            if (!string.IsNullOrEmpty(Notes))
                filters.Add(t => t.Notes.Contains(Notes));

            if (ResourceSubTypeIds != null && ResourceSubTypeIds.Any())
                filters.Add(t => ResourceSubTypeIds.Contains(t.ResourceSubTypeId));

            if (ResourceGroupIds != null && ResourceGroupIds.Any())
                filters.Add(t => t.ResourceGroupId.HasValue && ResourceGroupIds.Contains(t.ResourceGroupId.Value));

            if (ResourceStatusIds != null && ResourceStatusIds.Any())
                filters.Add(t => ResourceStatusIds.Contains(t.ResourceStatusId));

            if (ResourceLocationIds != null && ResourceLocationIds.Any())
                filters.Add(t => ResourceLocationIds.Contains(t.ResourceLocationId));

            if (TargetGroupIds != null && TargetGroupIds.Any())
                filters.Add(t => TargetGroupIds.Contains(t.TargetGroupId));

            if (UsagePurposeTypeIds != null && UsagePurposeTypeIds.Any())
                filters.Add(t => UsagePurposeTypeIds.Contains(t.UsagePurposeTypeId));

            if (AcquisitionTypeIds != null && AcquisitionTypeIds.Any())
                filters.Add(t => AcquisitionTypeIds.Contains(t.AcquisitionTypeId));

            if (ManufacturerIds != null && ManufacturerIds.Any())
                filters.Add(t => ManufacturerIds.Contains(t.ManufacturerId));

            if (ModelNameIds != null && ModelNameIds.Any())
                filters.Add(t => ModelNameIds.Contains(t.ModelNameId));

            if (EducationalInstitutionIds != null && EducationalInstitutionIds.Any())
                filters.Add(t => EducationalInstitutionIds.Contains(t.EducationalInstitutionId));

            if (SupervisorIds != null && SupervisorIds.Any())
                filters.Add(t => SupervisorIds.Contains(t.EducationalInstitution.SupervisorId));

            if (SocialSupportResource.HasValue)
                filters.Add(t => t.SocialSupportResource == SocialSupportResource);

            return filters.ToArray();
        }
    }

    public abstract class ResourceEditRequest
    {
        [MaxLength(500)]
        public string ResourceName { get; set; }

        [MaxLength(100)]
        public string ModelIdentifier { get; set; }

        [Column(TypeName = "decimal(8, 2)")]
        public decimal AcquisitionsValue { get; set; }
        public int ManufactureYear { get; set; }

        [Required]
        [MaxLength(200)]
        public string InventoryNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string SerialNumber { get; set; }

        public bool? SocialSupportResource { get; set; }
        public string Notes { get; set; }
        public Guid? ResourceGroupId { get; set; }
        public Guid ResourceStatusId { get; set; }
        public Guid ResourceLocationId { get; set; }
        public Guid TargetGroupId { get; set; }
        public Guid UsagePurposeTypeId { get; set; }
        public Guid AcquisitionTypeId { get; set; }
        public Guid ManufacturerId { get; set; }
        public Guid ModelNameId { get; set; }
        public IEnumerable<ResourceParameterEdit> ResourceParameters { get; set; } = Array.Empty<ResourceParameterEdit>();

        public class ResourceParameterEdit
        {
            public Guid? Id { get; set; }
            public string Value { get; set; }
            public Guid? ParameterId { get; set; }
        }
    }

    public class ResourceCreateRequest : ResourceEditRequest 
    {
        public int EducationalInstitutionId { get; set; }
        public Guid ResourceSubTypeId { get; set; }
    }

    public class ResourceUpdateRequest : ResourceEditRequest { }

    public class ResourceImportDataRequest
    {
        public int EducationalInstitutionId { get; set; }
        public IFormFile File { get; set; }
    }

    public class ResourceImportDataResponse
    {
        public int Imported { get; set; }
        public IEnumerable<Error> Errors { get; set; }

        public class Error
        {
            public string Message { get; set; }
            public int? Row { get; set; }
            public string Column { get; set; }
        }
    }
}
