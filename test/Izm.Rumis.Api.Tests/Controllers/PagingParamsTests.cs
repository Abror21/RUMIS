using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public class PagingParamsTests
    {
        [Fact]
        public void SortsAscending()
        {
            var para = new PagingParams<Item>(new PagingRequest { Sort = "Value" })
                .AddSorting("value", t => t.Value);

            var query = new SetQuery<Item>(new List<Item>
            {
                new Item(2),
                new Item(1)
            }.AsQueryable());

            para.ApplyToQuery(query);

            var data = query.List();

            Assert.Collection(data,
                t => Assert.Equal(1, t.Value),
                t => Assert.Equal(2, t.Value));
        }

        [Fact]
        public void SortsDescending()
        {
            var para = new PagingParams<Item>(new PagingRequest { Sort = "Value", SortDir = SortDirection.Desc })
                .AddSorting("value", t => t.Value);

            var query = new SetQuery<Item>(new List<Item>
            {
                new Item(1),
                new Item(2)
            }.AsQueryable());

            para.ApplyToQuery(query);

            var data = query.List();

            Assert.Collection(data,
                t => Assert.Equal(2, t.Value),
                t => Assert.Equal(1, t.Value));
        }

        [Fact]
        public void LimitsItemCount()
        {
            var para = new PagingParams<Item>(new PagingRequest { Take = 1 });
            var query = new SetQuery<Item>(new List<Item>
            {
                new Item(1),
                new Item(2)
            }.AsQueryable());

            para.ApplyToQuery(query);

            var data = query.List();

            Assert.Single(data);
        }

        [Fact]
        public void Paginates()
        {
            var para = new PagingParams<Item>(new PagingRequest { Take = 1, Page = 2 });
            var query = new SetQuery<Item>(new List<Item>
            {
                new Item(1),
                new Item(2)
            }.AsQueryable());

            para.ApplyToQuery(query);

            var data = query.List();

            Assert.Collection(data, t => Assert.Equal(2, t.Value));
        }

        [Fact]
        public void PaginatesWithoutPageSize()
        {
            var para = new PagingParams<Item>(new PagingRequest { Page = 2 }).SetMaxPageSize(10);
            var query = new SetQuery<Item>(new List<Item>
            {
                new Item(1),
                new Item(2)
            }.AsQueryable());

            para.ApplyToQuery(query);

            var data = query.List();

            Assert.Empty(data);
        }

        [Fact]
        public void SortsAscendingAndLimits()
        {
            var para = new PagingParams<Item>(new PagingRequest { Sort = "Value", Take = 1 })
                .AddSorting("value", t => t.Value);

            var query = new SetQuery<Item>(new List<Item>
            {
                new Item(2),
                new Item(1)
            }.AsQueryable());

            para.ApplyToQuery(query);

            var data = query.List();

            Assert.Collection(data,
                t => Assert.Equal(1, t.Value));
        }

        [Fact]
        public void SortsDescendingAndLimits()
        {
            var para = new PagingParams<Item>(new PagingRequest { Sort = "Value", Take = 1, SortDir = SortDirection.Desc })
                .AddSorting("value", t => t.Value);

            var query = new SetQuery<Item>(new List<Item>
            {
                new Item(1),
                new Item(2)
            }.AsQueryable());

            para.ApplyToQuery(query);

            var data = query.List();

            Assert.Collection(data,
                t => Assert.Equal(2, t.Value));
        }

        [Fact]
        public void SortsAscendingAndPaginates()
        {
            var para = new PagingParams<Item>(new PagingRequest { Sort = "Value", Take = 1, Page = 2 })
                .AddSorting("value", t => t.Value);

            var query = new SetQuery<Item>(new List<Item>
            {
                new Item(2),
                new Item(1)
            }.AsQueryable());

            para.ApplyToQuery(query);

            var data = query.List();

            Assert.Collection(data,
                t => Assert.Equal(2, t.Value));
        }

        [Fact]
        public void SortsDescendingAndPaginates()
        {
            var para = new PagingParams<Item>(new PagingRequest { Sort = "Value", Take = 1, Page = 2, SortDir = SortDirection.Desc })
                .AddSorting("value", t => t.Value);

            var query = new SetQuery<Item>(new List<Item>
            {
                new Item(1),
                new Item(2)
            }.AsQueryable());

            para.ApplyToQuery(query);

            var data = query.List();

            Assert.Collection(data,
                t => Assert.Equal(1, t.Value));
        }

        private class Item
        {
            public Item(int value)
            {
                Value = value;
            }

            public int Value { get; set; }
        }
    }
}
