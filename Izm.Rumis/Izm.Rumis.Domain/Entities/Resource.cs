using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Izm.Rumis.Domain.Entities
{
    public class Resource : Entity<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string ResourceNumber { get; set; }

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

        [Column(TypeName = "longtext")]
        public string ResourceStatusHistory { get; set; }

        [Column(TypeName = "longtext")]
        public string Notes { get; set; }

        public int EducationalInstitutionId { get; set; }
        public virtual EducationalInstitution EducationalInstitution { get; set; }

        [ClassifierType(ClassifierTypes.ResourceSubType)]
        public Guid ResourceSubTypeId { get; set; }
        public virtual Classifier ResourceSubType { get; set; }

        [ClassifierType(ClassifierTypes.ResourceGroup)]
        public Guid? ResourceGroupId { get; set; }
        public virtual Classifier ResourceGroup { get; set; }

        [ClassifierType(ClassifierTypes.ResourceStatus)]
        public Guid ResourceStatusId { get; set; }
        public virtual Classifier ResourceStatus { get; set; }

        [ClassifierType(ClassifierTypes.ResourceLocation)]
        public Guid ResourceLocationId { get; set; }
        public virtual Classifier ResourceLocation { get; set; }

        [ClassifierType(ClassifierTypes.TargetGroup)]
        public Guid TargetGroupId { get; set; }
        public virtual Classifier TargetGroup { get; set; }

        [ClassifierType(ClassifierTypes.ResourceUsingPurpose)]
        public Guid UsagePurposeTypeId { get; set; }
        public virtual Classifier UsagePurposeType { get; set; }

        [ClassifierType(ClassifierTypes.ResourceAcquisitionType)]
        public Guid AcquisitionTypeId { get; set; }
        public virtual Classifier AcquisitionType { get; set; }

        [ClassifierType(ClassifierTypes.ResourceManufacturer)]
        public Guid ManufacturerId { get; set; }
        public virtual Classifier Manufacturer { get; set; }

        [ClassifierType(ClassifierTypes.ResourceModelName)]
        public Guid ModelNameId { get; set; }
        public virtual Classifier ModelName { get; set; }

        public virtual ICollection<ResourceParameter> ResourceParameters { get; set; } = new List<ResourceParameter>();

        public override string ToString()
        {
            if (ModelName == null)
                return $"{Manufacturer.Value}";

            return $"{Manufacturer.Value} {ModelName.Value}";
        }
    }
}
