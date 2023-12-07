using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class DocumentTemplate : Entity<int>, IAuthorizedResource
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(250)]
        public string Title { get; set; }

        public DateOnly? ValidFrom { get; set; }

        public DateOnly? ValidTo { get; set; }

        public Guid FileId { get; set; }
        public string FileName { get; set; }

        public UserProfileType PermissionType { get; set; }

        public int? EducationalInstitutionId { get; set; }
        public virtual EducationalInstitution EducationalInstitution { get; set; }

        public int? SupervisorId { get; set; }
        public virtual Supervisor Supervisor { get; set; }

        [MaxLength(2000)]
        public string Hyperlink { get; set; }

        [ClassifierType(ClassifierTypes.ResourceType)]
        public Guid ResourceTypeId { get; set; }
        public virtual Classifier ResourceType { get; set; }

    }
}
