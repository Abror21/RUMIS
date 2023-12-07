using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Domain.Entities;

namespace Izm.Rumis.Application.Validators
{
    public interface IApplicationResourceAttachmentValidator
    {
        /// <summary>
        /// Validate <see cref="ApplicationResourceAttachment"/> item.
        /// </summary>
        /// <param name="item">Item to validate.</param>
        /// <returns></returns>
        void Validate(ApplicationResourceAttachment item);
    }

    public sealed class ApplicationResourceAttachmentValidator : IApplicationResourceAttachmentValidator
    {
        // <inheritdoc/>
        public void Validate(ApplicationResourceAttachment item)
        {
            if (!item.DocumentTemplateId.HasValue)
                throw new ValidationException(Error.DocumentTemplateRequired);
        }

        public static class Error
        {
            public const string DocumentTemplateRequired = "applicationResourceAttachment.documentTemplateRequired";
      
        }
    }
}
