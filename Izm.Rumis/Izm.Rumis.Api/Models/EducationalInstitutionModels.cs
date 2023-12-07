using Izm.Rumis.Api.Common;
using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Models
{
    public class EducationalInstitutionCreateRequest
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
        public int SupervisorId { get; set; }
        public Guid StatusId { get; set; }
    }

    public class EducationalInstitutionListItemFilterRequest : Filter<EducationalInstitution>
    {
        public IEnumerable<int> EducationalInstitutionIds { get; set; }
        public IEnumerable<int> SupervisorIds { get; set; }
        public IEnumerable<Guid> EducationalInstitutionStatusIds { get; set; }

        protected override Expression<Func<EducationalInstitution, bool>>[] GetFilters()
        {
            var result = new List<Expression<Func<EducationalInstitution, bool>>>();

            if (EducationalInstitutionIds != null)
                result.Add(t => EducationalInstitutionIds.Contains(t.Id));

            if (SupervisorIds != null)
                result.Add(t => SupervisorIds.Contains(t.SupervisorId));

            if (EducationalInstitutionStatusIds != null)
                result.Add(t => EducationalInstitutionStatusIds.Contains(t.StatusId));

            return result.ToArray();
        }
    }

    public class EducationalInstitutionListItemResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public ClassifierData Status { get; set; }
        public SupervisorData Supervisor { get; set; }

        public class SupervisorData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }
        }
    }

    public class EducationalInstitutionResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Municipality { get; set; }
        public string Village { get; set; }
        public ClassifierData Status { get; set; }
        public SupervisorData Supervisor { get; set; }
        public IEnumerable<EducationalInstitutionContactPersonData> EducationalInstitutionContactPersons { get; set; }
        public IEnumerable<EducationalInstitutionResourceSubTypeData> EducationalInstitutionResourceSubTypes { get; set; }

        public class EducationalInstitutionContactPersonData
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public ClassifierData JobPosition { get; set; }
            public IEnumerable<ContactPersonResourceSubTypeData> ContactPersonResourceSubTypes { get; set; }

            public class ContactPersonResourceSubTypeData
            {
                public Guid Id { get; set; }
                public ClassifierData ResourceSubType { get; set; }
            }
        }

        public class EducationalInstitutionResourceSubTypeData
        {
            public Guid Id { get; set; }
            public ClassifierData ResourceSubType { get; set; }
            public ClassifierData TargetPersonGroupType { get; set; }
        }

        public class SupervisorData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }
        }
    }

    public class EducationalInstitutionUpdateRequest
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
        public int SupervisorId { get; set; }
        public Guid StatusId { get; set; }
        public IEnumerable<EducationalInstitutionContactPersonData> EducationalInstitutionContactPersons { get; set; } = Array.Empty<EducationalInstitutionContactPersonData>();
        public IEnumerable<EducationalInstitutionResourceSubTypeData> EducationalInstitutionResourceSubTypes { get; set; } = Array.Empty<EducationalInstitutionResourceSubTypeData>();


        public class EducationalInstitutionContactPersonData
        {
            public Guid? Id { get; set; }

            [MaxLength(200)]
            public string Name { get; set; }

            [MaxLength(100)]
            public string Email { get; set; }

            [MaxLength(50)]
            public string PhoneNumber { get; set; }

            [MaxLength(500)]
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
