using System;
using System.Collections.Generic;

namespace Izm.Rumis.Application.Dto
{
    public class EducationalInstitutionCreateDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Municipality { get; set; }
        public string Village { get; set; }
        public int SupervisorId { get; set; }
        public Guid StatusId { get; set; }
    }

    public class EducationalInstitutionUpdateDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Municipality { get; set; }
        public string Village { get; set; }
        public int SupervisorId { get; set; }
        public Guid StatusId { get; set; }
        public IEnumerable<EducationalInstitutionContactPersonData> EducationalInstitutionContactPersons { get; set; } = Array.Empty<EducationalInstitutionContactPersonData>();
        public IEnumerable<EducationalInstitutionResourceSubTypeData> EducationalInstitutionResourceSubTypes { get; set; } = Array.Empty<EducationalInstitutionResourceSubTypeData>();

        public class EducationalInstitutionContactPersonData
        {
            public Guid? Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public Guid JobPositionId { get; set; }
            public IEnumerable<ContactPersonResourceSubTypeData> ContactPersonResourceSubTypes { get; set; } = Array.Empty<ContactPersonResourceSubTypeData>();

            public class ContactPersonResourceSubTypeData
            {
                public Guid? Id { get; set; }
                public Guid ResourceSubTypeId { get; set; }
            }
        }

        public class EducationalInstitutionResourceSubTypeData
        {
            public Guid? Id { get; set; }
            public Guid ResourceSubTypeId { get; set; }
            public Guid TargetPersonGroupTypeId { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
