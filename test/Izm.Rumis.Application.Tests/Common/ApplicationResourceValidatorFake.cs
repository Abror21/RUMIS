using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Validators;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Tests.Common
{
    internal sealed class ApplicationResourceValidatorFake : IApplicationResourceValidator
    {
        public ApplicationResourceCreateDto ValidateAsyncCalledWith { get; set; } = null;
        public ApplicationResourceReturnEditDto ValidateResourceStatusAsyncCalledWith { get; set; } = null;

        public Task ValidateAsync(ApplicationResourceCreateDto item, CancellationToken cancellationToken = default)
        {
            ValidateAsyncCalledWith = item;

            return Task.CompletedTask;
        }

        public Task ValidateResourceStatusAsync(ApplicationResourceReturnEditDto item, CancellationToken cancellationToken = default)
        {
            ValidateResourceStatusAsyncCalledWith = item;

            return Task.CompletedTask;
        }
    }
}
