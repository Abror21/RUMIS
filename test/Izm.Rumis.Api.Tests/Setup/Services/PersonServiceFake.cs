using Azure;
using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal sealed class PersonServiceFake : IPersonService
    {
        public PersonCreateDto CreateCalledWith { get; set; } = null;
        public Guid? EnsureUserCalledWith { get; set; } = null;
        public IQueryable<Person> Persons { get; set; } = new TestAsyncEnumerable<Person>(new List<Person>());


        public Task<PersonCreateResponseDto> CreateAsync(PersonCreateDto item, CancellationToken cancellationToken = default)
        {
            CreateCalledWith = item;

            return Task.FromResult(new PersonCreateResponseDto { Id = Guid.NewGuid(), UserId = Guid.NewGuid() });
        }

        public Task<Guid?> EnsureUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            EnsureUserCalledWith = id;

            Guid? response = Guid.NewGuid();

            return Task.FromResult(response);
        }

        public SetQuery<Person> Get()
        {
            return new SetQuery<Person>(Persons);
        }

        public Task UpdateAsync(Guid id, PersonUpdateDto item, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
