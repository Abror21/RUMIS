using Izm.Rumis.Infrastructure.Vraa;
using Izm.Rumis.Infrastructure.Vraa.Models;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal sealed class VraaClientFake : IVraaClient
    {
        public IntrospectResult IntrospectResult { get; set; } = new IntrospectResult();

        public Task<IntrospectResult> IntrospectAsync(string token)
        {
            return Task.FromResult(IntrospectResult);
        }
    }
}
