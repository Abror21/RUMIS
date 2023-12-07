using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class EducationalInstitutionContactPerson : Entity<Guid>
    {
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string PhoneNumber { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [ClassifierType(ClassifierTypes.EducationalInstitutionJobPosition)]
        public Guid JobPositionId { get; set; }
        public virtual Classifier JobPosition { get; set; }

        public int EducationalInstitutionId { get; set; }
        public virtual EducationalInstitution EducationalInstitution { get; set; }

        public virtual ICollection<ContactPersonResourceSubType> ContactPersonResourceSubTypes { get; set; } = new List<ContactPersonResourceSubType>();
    }
}
