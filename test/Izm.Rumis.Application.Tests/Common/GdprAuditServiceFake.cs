using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Tests.Common
{
    internal sealed class GdprAuditServiceFake : IGdprAuditService
    {
        public GdprAuditTraceDto TraceAsyncCalledWith { get; set; } = null;
        public IEnumerable<GdprAuditTraceDto> TraceRangeAsyncCalledWith { get; set; } = null;

        public Task TraceAsync(GdprAuditTraceDto item, CancellationToken cancellationToken = default)
        {
            TraceAsyncCalledWith = item;

            return Task.CompletedTask;
        }

        public Task TraceRangeAsync(IEnumerable<GdprAuditTraceDto> items, CancellationToken cancellationToken = default)
        {
            TraceRangeAsyncCalledWith = items;

            return Task.CompletedTask;
        }
    }
}
