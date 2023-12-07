using System;
using System.Collections.Generic;

namespace Izm.Rumis.Infrastructure.EServices.Dtos
{
    public class EServiceApplicationCreateDto
    {
        public string ApplicationStatusHistory { get; set; }
        public int EducationalInstitutionId { get; set; }
        public string Notes { get; set; }

        public Guid ResourceSubTypeId { get; set; }
        public int? ResourceTargetPersonClassGrade { get; set; }
        public string ResourceTargetPersonClassParallel { get; set; }
        public string ResourceTargetPersonEducationalProgram { get; set; }
        public Guid? ResourceTargetPersonEducationalStatusId { get; set; }
        public Guid? ResourceTargetPersonEducationalSubStatusId { get; set; }
        public string ResourceTargetPersonGroup { get; set; }
        public PersonData ResourceTargetPerson { get; set; }
        public Guid ResourceTargetPersonTypeId { get; set; }
        public Guid? ResourceTargetPersonWorkStatusId { get; set; }

        public Guid SubmitterTypeId { get; set; }
        public IEnumerable<PersonData.ContactData> SubmitterContactData { get; set; } = Array.Empty<PersonData.ContactData>();

        public bool? SocialStatus { get; set; }
        public bool? SocialStatusApproved { get; set; }
        public IEnumerable<Guid> ApplicationSocialStatuses { get; set; } = Array.Empty<Guid>();

        public class PersonData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PrivatePersonalIdentifier { get; set; }
            public IEnumerable<ContactData> ContactInformation { get; set; } = Array.Empty<ContactData>();

            public class ContactData
            {
                public Guid TypeId { get; set; }
                public string Value { get; set; }
            }
        }
    }
}
