using Izm.Rumis.Infrastructure.EAddress.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.EAddress
{
    public interface IEAddressClient
    {
        Task<string> SendMessageAsync(EAddressSendMessageRequest request, CancellationToken cancellationToken = default);
        Task<EAddressValidateNaturalPersonResponse> ValidateNaturalPersonAsync(string privatePersonalIdentifier, CancellationToken cancellationToken = default);
    }
}