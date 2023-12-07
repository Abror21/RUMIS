using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Izm.Rumis.Application.Dto
{
    public abstract class ResourceEditDto
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
        public IEnumerable<ResourceParameterDto> ResourceParameters { get; set; } = Array.Empty<ResourceParameterDto>();
    }

    public class ResourceCreateDto : ResourceEditDto 
    {
        public int EducationalInstitutionId { get; set; }
        public Guid ResourceSubTypeId { get; set; }
    }

    public class ResourceUpdateDto : ResourceEditDto { }
}
