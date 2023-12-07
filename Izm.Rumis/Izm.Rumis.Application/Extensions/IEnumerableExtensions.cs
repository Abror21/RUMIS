using Izm.Rumis.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Izm.Rumis.Application.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IOrderedEnumerable<T> OrderById<T>(this IEnumerable<T> source) where T : IEntity<int>
        {
            return source.OrderBy(t => t.Id);
        }
    }
}
