using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Common
{
    public abstract class Filter<T>
    {
        public void ApplyToQuery(SetQuery<T> query)
        {
            var filters = GetFilters();

            foreach (var filter in filters)
                query.Where(filter);
        }

        protected abstract Expression<Func<T, bool>>[] GetFilters();
    }
}
