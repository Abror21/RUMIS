using Izm.Rumis.Api.Common;
using Izm.Rumis.Application.Common;

namespace Izm.Rumis.Api.Extensions
{
    public static class SetQueryExtensions
    {
        public static SetQuery<T> Paging<T>(this SetQuery<T> query, PagingParams<T> pagingParams)
        {
            if (pagingParams != null)
                pagingParams.ApplyToQuery(query);

            return query;
        }

        public static SetQuery<T> Filter<T>(this SetQuery<T> query, Filter<T> filter)
        {
            if (filter != null)
                filter.ApplyToQuery(query);

            return query;
        }
    }
}
