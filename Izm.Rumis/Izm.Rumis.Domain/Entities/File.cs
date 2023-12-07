using Izm.Rumis.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class File : Entity<Guid>
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(10)]
        public string Extension { get; set; }

        [MaxLength(100)]
        public string ContentType { get; set; }

        [Required]
        public FileSourceType SourceType { get; set; }

        public int Length { get; set; }
        public byte[] Content { get; set; }

        [MaxLength(100)]
        public string BucketName { get; set; }
    }
}
  