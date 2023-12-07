using Izm.Rumis.Domain.Enums;

namespace Izm.Rumis.Api.Models
{
    public class PagingRequest
    {
        public int? Page { get; set; }
        public int? Take { get; set; }
        public string Sort { get; set; }
        public SortDirection? SortDir { get; set; }
    }
}
