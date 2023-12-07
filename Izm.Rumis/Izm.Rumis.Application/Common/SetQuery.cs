using Izm.Rumis.Application.Extensions;
using Izm.Rumis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Common
{
    /// <summary>
    /// Set query allows to query an object set using most common operations and mapping.
    /// For advanced scenarios consider preparing a query inside a service method.
    /// </summary>
    /// <remarks>This is a wrapper around IQueryable to prevent it from using directly outside of the application layer.</remarks>
    /// <typeparam name="T"></typeparam>
    public class SetQuery<T>
    {
        public SetQuery(IQueryable<T> set)
        {
            this.set = set;
        }

        internal IQueryable<T> set;
        private bool isOrdered;

        public SetQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            set = set.Where(predicate);
            return this;
        }

        public SetQuery<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> keySelector, SortDirection direction = SortDirection.Asc)
        {
            if (isOrdered)
            {
                var query = set as IOrderedQueryable<T>;
                set = direction == SortDirection.Desc ? query.ThenByDescending(keySelector) : query.ThenBy(keySelector);
            }
            else
            {
                set = direction == SortDirection.Desc ? set.OrderByDescending(keySelector) : set.OrderBy(keySelector);
            }

            isOrdered = true;

            return this;
        }

        public SetQuery<T> OrderBy(Expression<Func<T, object>> keySelector, SortDirection direction = SortDirection.Asc)
        {
            if (isOrdered)
            {
                var query = set as IOrderedQueryable<T>;
                set = direction == SortDirection.Desc ? query.ThenByDescending(keySelector) : query.ThenBy(keySelector);
            }
            else
            {
                set = direction == SortDirection.Desc ? set.OrderByDescending(keySelector) : set.OrderBy(keySelector);
            }

            isOrdered = true;

            return this;
        }

        public SetQuery<T> Skip(int count)
        {
            set = set.Skip(count);
            return this;
        }

        public SetQuery<T> Take(int count)
        {
            set = set.Take(count);
            return this;
        }

        public SetQuery<T> Page(int size, int page = 1)
        {
            set = set.Skip((page - 1) * size).Take(size);
            return this;
        }

        public T First()
        {
            return set.FirstOrDefault();
        }

        public TResult First<TResult>(Expression<Func<T, TResult>> map)
        {
            return set.Select(map).FirstOrDefault();
        }

        public Task<T> FirstAsync(CancellationToken cancellationToken = default)
        {
            return set.FirstOrDefaultAsync(cancellationToken);
        }

        public Task<TResult> FirstAsync<TResult>(Expression<Func<T, TResult>> map, CancellationToken cancellationToken = default)
        {
            return set.Select(map).FirstOrDefaultAsync(cancellationToken);
        }

        public IEnumerable<T> List(bool distinct = false)
        {
            return distinct ? set.Distinct().ToList() : (IEnumerable<T>)set.ToList();
        }

        public IEnumerable<TResult> List<TResult>(Expression<Func<T, TResult>> map, bool distinct = false)
        {
            return distinct ? set.Select(map).Distinct().ToList() : (IEnumerable<TResult>)set.Select(map).ToList();
        }

        public Task<List<T>> ListAsync(bool distinct = false, CancellationToken cancellationToken = default)
        {
            return distinct ? set.Distinct().ToListAsync(cancellationToken) : set.ToListAsync(cancellationToken);
        }

        public Task<List<TResult>> ListAsync<TResult>(Expression<Func<T, TResult>> map, bool distinct = false, CancellationToken cancellationToken = default)
        {
            return distinct
                ? set.Select(map).Distinct().ToListAsync(cancellationToken)
                : set.Select(map).ToListAsync(cancellationToken);
        }

        public int Count()
        {
            return set.Count();
        }

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return set.CountAsync(cancellationToken);
        }

        public bool Any()
        {
            return set.Any();
        }
    }
}
