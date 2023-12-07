using Izm.Rumis.Application.Contracts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Tests.Common
{
    internal class ApplicationSocialStatusCheckService : IApplicationSocialStatusCheckService
    {
        Task<Dictionary<string, bool>> IApplicationSocialStatusCheckService.CheckSocialStatusesAsync(string privatePersonalIdentifier, IEnumerable<string> statusTypes, CancellationToken cancellationToken = default)
        {
            var socialStatusTestValue = new Dictionary<string, bool>
            {
                { "I", true },
                { "M", false },
                { "P", false },
                { "T", false }
            };

            return Task.FromResult(socialStatusTestValue);
        }
    }
}
