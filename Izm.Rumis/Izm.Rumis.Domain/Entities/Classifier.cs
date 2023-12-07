using Izm.Rumis.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class Classifier : Entity<Guid>, IAuthorizedResource
    {
        [Required]
        [MaxLength(50)]
        public string Type { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(500)]
        public string Value { get; set; }

        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public string Payload { get; set; }
        public int? SortOrder { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsRequired { get; set; }

        public UserProfileType PermissionType { get; set; }

        public int? EducationalInstitutionId { get; set; }
        public virtual EducationalInstitution EducationalInstitution { get; set; }

        public int? SupervisorId { get; set; }
        public virtual Supervisor Supervisor { get; set; }
    }
}
