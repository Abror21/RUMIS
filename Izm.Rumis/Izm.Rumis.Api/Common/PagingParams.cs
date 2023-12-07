using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Common
{
    public class PagingParams<T>
    {
        public PagingParams(PagingRequest paging)
        {
            if (paging == null)
                paging = new PagingRequest();

            this.paging = paging;
        }

        private readonly PagingRequest paging;

        public Expression<Func<T, object>> DefaultSortBy { get; private set; }
        public SortDirection DefaultSortDirection { get; private set; }
        public int MaxPageSize { get; private set; } = 30;

        /// <summary>
        /// Actual page size
        /// </summary>
        public int PageSize
        {
            get
            {
                var value = paging.Take ?? MaxPageSize;

                if (value > MaxPageSize)
                    value = MaxPageSize;

                return value;
            }
        }

        public bool ShouldPage => paging.Page.HasValue == true;
        public bool ShouldLimit => paging.Take.HasValue == true;
        public int Page => paging.Page ?? 1;
        public Dictionary<string, Expression<Func<T, object>>> Sorting { get; } = new Dictionary<string, Expression<Func<T, object>>>();

        public PagingParams<T> SetMaxPageSize(int maxPageSize)
        {
            MaxPageSize = maxPageSize;
            return this;
        }

        /// <summary>
        /// Add named sorting.
        /// </summary>
        /// <param name="name">Name of the sorting must match the one provided by <see cref="PagingRequest.Sort"/></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public PagingParams<T> AddSorting(string name, Expression<Func<T, object>> selector)
        {
            Sorting.Add(name.ToLower(), selector);
            return this;
        }

        /// <summary>
        /// Add default sorting.
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public PagingParams<T> AddDefaultSorting(Expression<Func<T, object>> selector, SortDirection direction = SortDirection.Asc)
        {
            DefaultSortBy = selector;
            DefaultSortDirection = direction;

            return this;
        }

        public void ApplyToQuery(SetQuery<T> query)
        {
            if (!string.IsNullOrEmpty(paging.Sort) && Sorting.Keys.Contains(paging.Sort.ToLower()))
            {
                var instruction = Sorting[paging.Sort.ToLower()];

                if (instruction != null)
                {
                    query.OrderBy(instruction, paging.SortDir ?? SortDirection.Asc);
                }
            }
            else if (DefaultSortBy != null)
            {
                query.OrderBy(DefaultSortBy, DefaultSortDirection);
            }

            if (ShouldPage)
                query.Skip(((paging.Page ?? 1) - 1) * PageSize).Take(PageSize);
            else if (ShouldLimit)
                query.Take(paging.Take.Value);
        }

        public static SetQuery<T> Apply(PagingParams<T> paging, SetQuery<T> query)
        {
            if (paging != null)
                paging.ApplyToQuery(query);

            return query;
        }
    }
}
