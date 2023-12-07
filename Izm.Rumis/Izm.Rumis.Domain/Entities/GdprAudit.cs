using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class GdprAudit
    {
        public Guid Id { get; set; }
        public Guid UnitOfWorkId { get; set; }
        public DateTime Created { get; set; }

        public Guid? UserId { get; set; }
        public virtual User User { get; set; }

        public Guid? UserProfileId { get; set; }
        public virtual UserProfile UserProfile { get; set; }

        [MaxLength(11)]
        public string DataHandlerPrivatePersonalIdentifier { get; set; }

        public Guid? DataHandlerId { get; set; }
        public virtual PersonTechnical DataHandler { get; set; }

        public int? EducationalInstitutionId { get; set; }
        public virtual EducationalInstitution EducationalInstitution { get; set; }

        public int? SupervisorId { get; set; }
        public virtual Supervisor Supervisor { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; }

        [MaxLength(500)]
        public string ActionData { get; set; }

        [MaxLength(11)]
        public string DataOwnerPrivatePersonalIdentifier { get; set; }

        public Guid? DataOwnerId { get; set; }
        public virtual PersonTechnical DataOwner { get; set; }

        public virtual ICollection<GdprAuditData> Data { get; set; } = new List<GdprAuditData>();
    }
}
