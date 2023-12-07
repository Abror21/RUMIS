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
    internal class ClassifierServiceFake : IClassifierService
    {
        public IQueryable<Classifier> Data { get; set; } = new TestAsyncEnumerable<Classifier>(new List<Classifier>());

        public Guid CreateResult { get; set; } = Guid.NewGuid();

        public Task<Guid> CreateAsync(ClassifierCreateDto item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResult);
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public SetQuery<Classifier> Get()
        {
            return new SetQuery<Classifier>(Data);
        }

        public SetQuery<Classifier> GetInEServices()
        {
            return new SetQuery<Classifier>(Data);
        }

        public Task UpdateAsync(Guid id, ClassifierUpdateDto item, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
