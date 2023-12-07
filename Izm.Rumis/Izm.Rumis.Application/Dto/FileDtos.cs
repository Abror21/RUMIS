using Izm.Rumis.Domain.Enums;

namespace Izm.Rumis.Application.Dto
{
    public class FileDto
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
        public FileSourceType SourceType { get; set; } = FileSourceType.Database;

        public bool HasValue => Content != null;
    }
}
