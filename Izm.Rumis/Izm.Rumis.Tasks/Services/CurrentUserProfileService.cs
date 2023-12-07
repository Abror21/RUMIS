using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Izm.Rumis.Tasks.Services
{
    public sealed class CurrentUserProfileService : ICurrentUserProfileService
    {
        public Guid Id => throw new NotImplementedException();

        public Guid UserId => throw new NotImplementedException();

        public UserProfileType Type => throw new NotImplementedException();

        public string Role => throw new NotImplementedException();

        public int? EducationalInstitutionId => throw new NotImplementedException();

        public int? SupervisorId => throw new NotImplementedException();

        public IEnumerable<string> Permissions => throw new NotImplementedException();

        public bool IsInitialized => throw new NotImplementedException();

        public bool HasPermission(string permission)
        {
            throw new NotImplementedException();
        }
    }
}
