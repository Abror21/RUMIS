using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IApplicationSocialStatusCheckService
    {
        Task<Dictionary<string, bool>> CheckSocialStatusesAsync(string privatePersonalIdentifier, IEnumerable<string> statusTypes, CancellationToken cancellationToken = default);
    }
}
