using Izm.Rumis.Application.Models;
using System;
using System.Collections.Generic;

namespace Izm.Rumis.Application.Contracts
{
    public interface ICurrentUserService
    {
        Guid Id { get; }
        Guid? PersonId { get; }
        string UserName { get; }
        string Email { get; }
        string IpAddress { get; }
        string RequestUrl { get; }
        string Language { get; }
        IEnumerable<PersonData> Persons { get; }
    }
}
