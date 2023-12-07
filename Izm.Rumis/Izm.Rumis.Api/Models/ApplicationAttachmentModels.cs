using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Api.Models
{
    public class ApplicationAttachmentResponse
    {
        public Guid Id { get; set; }
        public string AttachmentNumber { get; set; }
        public DateTime AttachmentDate { get; set; }
        public FileData File { get; set; }

        public class FileData
        {
            public Guid Id { get; set; }
            public string FileName { get; set; }
        }
    }

    public class ApplicationAttachmentEditRequest
    {
        [Required]
        [MaxLength(100)]
        public string AttachmentNumber { get; set; }

        [Required]
        public DateTime AttachmentDate { get; set; }
    }

    public class ApplicationAttachmentCreateRequest : ApplicationAttachmentEditRequest
    {
        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }

    public class ApplicationAttachmentUpdateRequest : ApplicationAttachmentEditRequest
    {
        /// <summary>
        /// Pass null to leave current file as it is.
        /// </summary>
        public IFormFile File { get; set; }
    }
}
