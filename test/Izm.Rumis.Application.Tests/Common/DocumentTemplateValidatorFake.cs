using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Tests.Common
{
    internal sealed class DocumentTemplateValidatorFake : IDocumentTemplateValidator
    {
        public DocumentTemplate ValidateAsyncCalledWith { get; set; } = null;

        public FileDto ValidateFileCalledWith { get; set; } = null;

        public Task ValidateAsync(DocumentTemplate item, CancellationToken cancellationToken = default)
        {
            ValidateAsyncCalledWith = item;

            return Task.CompletedTask;
        }

        public Task ValidateFileAsync(FileDto item, CancellationToken cancellationToken = default)
        {
            ValidateFileCalledWith = item;

            return Task.CompletedTask;
        }
    }
}
