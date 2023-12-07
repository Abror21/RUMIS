using Izm.Rumis.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Tests.Common
{
    internal sealed class ApplicationDuplicateServiceFake : IApplicationDuplicateService
    {
        public IEnumerable<Guid> CheckApplicationsDuplicatesAsyncCalledWith { get; set; } = null;

        public Task CheckApplicationsDuplicatesAsync(IEnumerable<Guid> applicationIds, CancellationToken cancellationToken = default)
        {
            CheckApplicationsDuplicatesAsyncCalledWith = applicationIds;

            return Task.CompletedTask;
        }
    }
}
