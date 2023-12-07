using System;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class ApplicationAttachment : Entity<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string AttachmentNumber { get; set; }

        public DateOnly AttachmentDate { get; set; }

        public Guid ApplicationId { get; set; }
        public virtual Application Application { get; set; }

        public Guid FileId { get; set; }
        public virtual File File { get; set; }
    }
}
