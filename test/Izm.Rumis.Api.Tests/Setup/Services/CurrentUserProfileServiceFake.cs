using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal class CurrentUserProfileServiceFake : ICurrentUserProfileService
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public UserProfileType Type { get; set; }

        public string Role { get; set; }

        public int? EducationalInstitutionId { get; set; }

        public int? SupervisorId { get; set; }

        public IEnumerable<string> Permissions { get; set; } = new string[] { };

        public bool IsInitialized { get; set; } = true;

        public bool HasPermission(string permission)
        {
            return Permissions.Contains(permission);
        }
    }
}
