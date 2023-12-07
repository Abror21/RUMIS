using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Tests.Common
{
    internal sealed class ResourceValidatorFake : IResourceValidator
    {
        public ResourceCreateDto ValidateAsyncCreateCalledWith { get; set; } = null;
        public Resource ValidateAsyncUpdateCalledWith { get; set; } = null;
        public FileDto ValidateFileCalledWith { get; set; } = null;


        public Task ValidateAsync(ResourceCreateDto item, CancellationToken cancellationToken = default)
        {
            ValidateAsyncCreateCalledWith = item;

            return Task.CompletedTask;
        }

        public Task ValidateAsync(Resource item, CancellationToken cancellationToken = default)
        {
            ValidateAsyncUpdateCalledWith = item;

            return Task.CompletedTask;
        }

        public void ValidateFile(FileDto item)
        {
            ValidateFileCalledWith = item;

            return;
        }
    }
}
