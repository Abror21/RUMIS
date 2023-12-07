using Izm.Rumis.Domain.Enums;
using System;

namespace Izm.Rumis.Application.Models
{
    public class FileEntry
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string ContentType { get; set; }
        public FileSourceType SourceType { get; set; }
        public int Length { get; set; }
        public byte[] Content { get; set; }
        public string BucketName { get; set; }
    }
}
