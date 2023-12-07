using Izm.Rumis.Api.Models;
using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    internal static class UserMapper
    {
        public static Expression<Func<User, UserPersonListItemResponse>> ProjectPersonListItem()
        {
            return user => new UserPersonListItemResponse
            {
                Id = user.Id,
                Persons = new List<UserPersonListItemResponse.PersonData>()
                {
                    new UserPersonListItemResponse.PersonData
                    {
                        FirstName = user.PersonTechnical.Persons
                            .OrderByDescending(t => t.ActiveFrom)
                            .FirstOrDefault()
                            .FirstName,
                        LastName = user.PersonTechnical.Persons
                            .OrderByDescending(t => t.ActiveFrom)
                            .FirstOrDefault()
                            .LastName,
                        PrivatePersonalIdentifier = user.PersonTechnical.Persons
                            .OrderByDescending(t => t.ActiveFrom)
                            .FirstOrDefault()
                            .PrivatePersonalIdentifier
                    }
                },
                PersonTechnicalId = user.PersonTechnical.Id,
                UserProfiles = user.Profiles.Select(profile => new UserPersonListItemResponse.UserProfileData
                {
                    Id = profile.Id,
                    EducationalInstitution = profile.EducationalInstitution == null ? null : new UserPersonListItemResponse.UserProfileData.EducationalInstitutionData
                    {
                        Id = profile.EducationalInstitution.Id,
                        Code = profile.EducationalInstitution.Code,
                        Name = profile.EducationalInstitution.Name
                    },
                    Supervisor = profile.Supervisor == null ? null : new UserPersonListItemResponse.UserProfileData.SupervisorData
                    {
                        Id = profile.Supervisor.Id,
                        Code = profile.Supervisor.Code,
                        Name = profile.Supervisor.Name
                    },
                    IsLoggedIn = profile.IsLoggedIn,
                    IsDisabled = profile.Disabled,
                    Roles = profile.Roles.Select(role => role.Name),
                    Type = profile.PermissionType
                })
            };
        }
    }
}
