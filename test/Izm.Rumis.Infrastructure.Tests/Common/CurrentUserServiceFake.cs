using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal class CurrentUserServiceFake : ICurrentUserService
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? PersonId { get; set; } = null;
        public string UserName { get; set; } = "userName";
        public string Email { get; set; } = "test@test.com";
        public string[] Roles { get; set; } = new string[] { };
        public string[] Permissions { get; set; } = new string[] { };
        public string IpAddress { get; set; }
        public string RequestUrl { get; set; }
        public string Language { get; set; }
        public IEnumerable<PersonData> Persons { get; set; } = Array.Empty<PersonData>();

        public bool HasPermission(string permission)
        {
            return Permissions.Contains(permission);
        }

        public bool HasRole(string role)
        {
            return Roles.Contains(role);
        }
    }
}
