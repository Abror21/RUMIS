using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Tests.Common
{
    internal sealed class ClassifierValidatorFake : IClassifierValidator
    {
        public Classifier ValidateAsyncCalledWith { get; set; } = null;

        public Task ValidateAsync(Classifier item, CancellationToken cancellationToken = default)
        {
            ValidateAsyncCalledWith = item;

            return Task.CompletedTask;
        }
    }
}
