using Microsoft.AspNetCore.Http;

namespace Izm.Rumis.Api.Models
{
    public class FileModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public IFormFile File { get; set; }
    }
}
