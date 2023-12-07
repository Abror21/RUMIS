using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Izm.Rumis.Application.Contracts
{
    public interface ICurrentUserProfileService
    {
        public Guid Id { get; }
        public bool IsInitialized { get; }
        public Guid UserId { get; }
        public UserProfileType Type { get; }
        public string Role { get; }
        public int? EducationalInstitutionId { get; }
        public int? SupervisorId { get; }
        public IEnumerable<string> Permissions { get; }

        bool HasPermission(string permission);
    }
}
