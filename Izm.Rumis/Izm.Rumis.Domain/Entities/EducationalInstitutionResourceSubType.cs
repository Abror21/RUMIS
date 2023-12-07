using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using System;

namespace Izm.Rumis.Domain.Entities
{
    public class EducationalInstitutionResourceSubType : Entity<Guid>
    {
        public int EducationalInstitutionId { get; set; }
        public virtual EducationalInstitution EducationalInstitution { get; set; }

        [ClassifierType(ClassifierTypes.ResourceSubType)]
        public Guid ResourceSubTypeId { get; set; }
        public virtual Classifier ResourceSubType { get; set; }

        [ClassifierType(ClassifierTypes.TargetGroup)]
        public Guid TargetPersonGroupTypeId { get; set; }
        public virtual Classifier TargetPersonGroupType { get; set; }
    }
}
