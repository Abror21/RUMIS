using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Validators;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Tests.Common
{
    internal sealed class ApplicationValidatorFake : IApplicationValidator
    {
        public string ValidateCalledWith { get; set; } = null;
        public bool ValidateThrowsException { get; set; } = false;
        public ApplicationCreateDto ValidateAsyncCalledWith { get; set; } = null;

        public void Validate(string entityStatusCode, string itemStatusCode)
        {
            ValidateCalledWith = entityStatusCode + itemStatusCode;

            if (ValidateThrowsException)
                throw new ValidationException();

            return;
        }

        public Task ValidateAsync(ApplicationCreateDto item, CancellationToken cancellationToken = default)
        {
            ValidateAsyncCalledWith = item;

            return Task.CompletedTask;
        }
    }
}
