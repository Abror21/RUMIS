using System;

namespace Izm.Rumis.Application.Dto
{
    public abstract class ApplicationAttachmentEditDto
    {
        public string AttachmentNumber { get; set; }
        public DateOnly AttachmentDate { get; set; }
        /// <summary>
        /// Pass null to remove the file.
        /// </summary>
        public FileDto File { get; set; }
    }

    public class ApplicationAttachmentCreateDto : ApplicationAttachmentEditDto
    {
        public Guid ApplicationId { get; set; }
    }

    public class ApplicationAttachmentUpdateDto : ApplicationAttachmentEditDto { }
}
