using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal class ParameterServiceFake : IParameterService
    {
        public IQueryable<Parameter> Parameters { get; set; } = new TestAsyncEnumerable<Parameter>(new List<Parameter>());

        public string FindValue(string code, IEnumerable<Parameter> parameters)
        {
            throw new NotImplementedException();
        }

        public SetQuery<Parameter> Get()
        {
            return new SetQuery<Parameter>(Parameters);
        }

        public string GetValue(string code)
        {
            return code;
        }

        public Task UpdateAsync(int id, string value, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
