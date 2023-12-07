using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using System;

namespace Izm.Rumis.Domain.Entities
{
    public class ContactPersonResourceSubType : Entity<Guid>
    {
        public Guid EducationalInstitutionContactPersonId { get; set; }
        public virtual EducationalInstitutionContactPerson EducationalInstitutionContactPerson { get; set; }

        [ClassifierType(ClassifierTypes.ResourceSubType)]
        public Guid ResourceSubTypeId { get; set; }
        public virtual Classifier ResourceSubType { get; set; }
    }
}
