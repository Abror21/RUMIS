using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class PersonDataReport : Entity<Guid>
    {
        [MaxLength(11)]
        public string DataHandlerPrivatePersonalIdentifier { get; set; }

        [MaxLength(11)]
        public string DataOwnerPrivatePersonalIdentifier { get; set; }

        [Required]
        [MaxLength(200)]
        public string Notes { get; set; }

        [ClassifierType(ClassifierTypes.PersonalDataSpecialistReasonForRequest)]
        public Guid ReasonId { get; set; }
        public virtual Classifier Reason { get; set; }

        public Guid UserProfileId { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}
