using Izm.Rumis.Api.Common;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Models
{
    public class UserProfileEditRequest
    {
        public Guid UserId { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string Job { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        public int? EducationalInstitutionId { get; set; }
        public DateTime Expires { get; set; }
        public bool IsDisabled { get; set; }
        public int? SupervisorId { get; set; }
        public UserProfileType Type { get; set; }
        public IEnumerable<int> RoleIds { get; set; } = Array.Empty<int>();

        [MaxLength(100)]
        public string ProfileCreationDocumentNumber { get; set; }
        public DateTime? ProfileCreationDocumentDate { get; set; }
        public string Notes { get; set; }
        public string ConfigurationInfo { get; set; }
        public Guid? InstitutionId { get; set; }
    }

    public class UserProfileFilterRequest : Filter<UserProfile>
    {
        protected override Expression<Func<UserProfile, bool>>[] GetFilters()
        {
            var filters = new List<Expression<Func<UserProfile, bool>>>();

            return filters.ToArray();
        }
    }

    public class UserProfileListItemIntermediateResponse
    {
        public Guid Id { get; set; }
        public Guid? PersonTechnicalId { get; set; }
        public bool Disabled { get; set; }
        public EducationalInstitutionData EducationalInstitution { get; set; }
        public string Email { get; set; }
        public DateTime Expires { get; set; }
        public SupervisorData Supervisor { get; set; }
        public string PhoneNumber { get; set; }
        public UserProfileType Type { get; set; }
        public bool IsLoggedIn { get; set; }
        public Guid UserId { get; set; }
        public IEnumerable<RoleData> Roles { get; set; } = Array.Empty<RoleData>();
        public IEnumerable<PersonData> Persons { get; set; } = Array.Empty<PersonData>();

        public class EducationalInstitutionData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class PersonData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime Created { get; set; }

        }

        public class RoleData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class SupervisorData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }

    public class UserProfileListItemResponse
    {
        public Guid Id { get; set; }
        public bool Disabled { get; set; }
        public EducationalInstitutionData EducationalInstitution { get; set; }
        public string Email { get; set; }
        public DateTime Expires { get; set; }
        public SupervisorData Supervisor { get; set; }
        public string PhoneNumber { get; set; }
        public UserProfileType Type { get; set; }
        public bool IsLoggedIn { get; set; }
        public Guid UserId { get; set; }
        public IEnumerable<RoleData> Roles { get; set; } = Array.Empty<RoleData>();
        public IEnumerable<PersonData> Persons { get; set; } = Array.Empty<PersonData>();

        public class EducationalInstitutionData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class PersonData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime Created { get; set; }

        }

        public class RoleData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class SupervisorData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }

    public class UserProfileIntermediateResponse
    {
        public Guid Id { get; set; }
        public Guid? PersonTechnicalId { get; set; }
        public bool Disabled { get; set; }
        public EducationalInstitutionData EducationalInstitution { get; set; }
        public string Email { get; set; }
        public DateTime Expires { get; set; }
        public SupervisorData Supervisor { get; set; }
        public string PhoneNumber { get; set; }
        public UserProfileType Type { get; set; }
        public bool IsLoggedIn { get; set; }
        public Guid UserId { get; set; }
        public IEnumerable<RoleData> Roles { get; set; } = Array.Empty<RoleData>();
        public string ProfileCreationDocumentNumber { get; set; }
        public DateTime? ProfileCreationDocumentDate { get; set; }
        public string Notes { get; set; }
        public string ConfigurationInfo { get; set; }
        public ClassifierData InstitutionId { get; set; }
        public string Job { get; set; }

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }
        }

        public class EducationalInstitutionData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class RoleData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class SupervisorData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }

    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public bool Disabled { get; set; }
        public EducationalInstitutionData EducationalInstitution { get; set; }
        public string Email { get; set; }
        public DateTime Expires { get; set; }
        public SupervisorData Supervisor { get; set; }
        public string PhoneNumber { get; set; }
        public UserProfileType Type { get; set; }
        public bool IsLoggedIn { get; set; }
        public Guid UserId { get; set; }
        public IEnumerable<RoleData> Roles { get; set; } = Array.Empty<RoleData>();
        public string ProfileCreationDocumentNumber { get; set; }
        public DateTime? ProfileCreationDocumentDate { get; set; }
        public string Notes { get; set; }
        public string ConfigurationInfo { get; set; }
        public ClassifierData InstitutionId { get; set; }
        public string Job { get; set; }

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }
        }

        public class EducationalInstitutionData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class RoleData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class SupervisorData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }
}
