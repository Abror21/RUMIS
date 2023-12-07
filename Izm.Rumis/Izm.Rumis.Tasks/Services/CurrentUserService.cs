using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Infrastructure.Common;
using System;
using System.Collections.Generic;

namespace Izm.Rumis.Tasks.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public Guid Id => UserIds.Tasks;
        public Guid? PersonId => null;
        public string UserName => UserNames.Tasks;
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
        public string[] Roles { get; }
        public string[] Permissions { get; }
        public string Language { get; set; }
        public string IpAddress { get; set; }
        public string RequestUrl { get; set; }
        public IEnumerable<PersonData> Persons { get; set; } = Array.Empty<PersonData>();

        public bool HasRole(string role)
        {
            return true;
        }

        public bool HasPermission(string permission)
        {
            return true;
        }
    }
}
