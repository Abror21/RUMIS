using Izm.Rumis.Api.Common;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Models
{
    public class UserPersonListItemResponse
    {
        public Guid Id { get; set; }
        public Guid PersonTechnicalId { get; set; }
        public IEnumerable<UserProfileData> UserProfiles { get; set; } = Array.Empty<UserProfileData>();
        public IEnumerable<PersonData> Persons { get; set; } = Array.Empty<PersonData>();

        public class UserProfileData
        {
            public Guid Id { get; set; }
            public UserProfileType Type { get; set; }
            public EducationalInstitutionData EducationalInstitution { get; set; }
            public SupervisorData Supervisor { get; set; }
            public bool IsDisabled { get; set; }
            public bool IsLoggedIn { get; set; }
            public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();

            public class EducationalInstitutionData
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

        public class PersonData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PrivatePersonalIdentifier { get; set; }
        }
    }

    public class UserListItemFilterRequest : Filter<User>
    {
        public IEnumerable<int?> EducationalInstitutionIds { get; set; }
        public IEnumerable<int?> SupervisorIds { get; set; }
        public string Person { get; set; }
        public IEnumerable<int> RoleIds { get; set; }
        public IEnumerable<UserProfileType> Types { get; set; }

        protected override Expression<Func<User, bool>>[] GetFilters()
        {
            var result = new List<Expression<Func<User, bool>>>();

            if (EducationalInstitutionIds != null)
                result.Add(t => t.Profiles.Any(n => EducationalInstitutionIds.Contains(n.EducationalInstitutionId)));

            if (SupervisorIds != null)
                result.Add(t => t.Profiles.Any(n => SupervisorIds.Contains(n.SupervisorId)));

            if (!string.IsNullOrEmpty(Person))
                result.Add(t => t.PersonTechnical.Persons.Any(t =>
                                                                t.FirstName.Contains(Person)
                                                                || t.LastName.Contains(Person)
                                                                || t.PrivatePersonalIdentifier.Contains(Person)));

            if (RoleIds != null)
                result.Add(t => t.Profiles.Any(n => n.Roles.Any(m => RoleIds.Contains(m.Id))));

            if (Types != null)
                result.Add(t => t.Profiles.Any(n => Types.Contains(n.PermissionType)));

            return result.ToArray();
        }
    }
}
