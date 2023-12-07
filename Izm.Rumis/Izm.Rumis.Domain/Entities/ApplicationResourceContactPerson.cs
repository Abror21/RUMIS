using System;

namespace Izm.Rumis.Domain.Entities
{
    public class ApplicationResourceContactPerson : Entity<Guid>
    {
        public Guid ApplicationResourceId { get; set; }
        public virtual ApplicationResource ApplicationResource { get; set; }

        public Guid EducationalInstitutionContactPersonId { get; set; }
        public virtual EducationalInstitutionContactPerson EducationalInstitutionContactPerson { get; set; }
    }
}
