using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class PersonContact : Entity<Guid>
    {
        [Required]
        [MaxLength(200)]
        public string ContactValue { get; set; }
        public bool IsActive { get; set; }

        [ClassifierType(ClassifierTypes.ContactType)]
        public Guid ContactTypeId { get; set; }
        public virtual Classifier ContactType { get; set; }

        public Guid PersonTechnicalId { get; set; }
        public virtual PersonTechnical PersonTechnical { get; set; }
    }
}
