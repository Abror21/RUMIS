using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Tests.Common
{
    internal sealed class ApplicationAttachmentValidatorFake : IApplicationAttachmentValidator
    {
        public void Validate(ApplicationAttachment item)
        {
            return;
        }

        public void ValidateFile(FileDto item)
        {
            return;
        }
    }
}
