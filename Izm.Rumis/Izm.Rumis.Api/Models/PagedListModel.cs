using System.Collections.Generic;

namespace Izm.Rumis.Api.Models
{
    public class PagedListModel<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
