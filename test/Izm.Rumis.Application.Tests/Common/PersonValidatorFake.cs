using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Validators;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Tests.Common
{
    internal sealed class PersonValidatorFake : IPersonValidator
    {
        public PersonCreateDto ValidateAsyncCalledWith { get; set; } = null;

        public Task ValidateAsync(PersonCreateDto item, CancellationToken cancellationToken = default)
        {
            ValidateAsyncCalledWith = item;

            return Task.CompletedTask;
        }
    }
}
