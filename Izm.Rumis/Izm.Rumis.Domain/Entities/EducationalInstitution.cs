using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class EducationalInstitution : Entity<int>
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(200)]
        public string City { get; set; }

        [MaxLength(200)]
        public string District { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string PhoneNumber { get; set; }

        [MaxLength(200)]
        public string Municipality { get; set; }

        [MaxLength(200)]
        public string Village { get; set; }

        [ClassifierType(ClassifierTypes.EducationalInstitutionStatus)]
        public Guid StatusId { get; set; }
        public virtual Classifier Status { get; set; }

        public int SupervisorId { get; set; }
        public virtual Supervisor Supervisor { get; set; }

        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
        public virtual ICollection<EducationalInstitutionContactPerson> EducationalInstitutionContactPersons { get; set; } = new List<EducationalInstitutionContactPerson>();
        public virtual ICollection<EducationalInstitutionResourceSubType> EducationalInstitutionResourceSubTypes { get; set; } = new List<EducationalInstitutionResourceSubType>();
        public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
    }
}
