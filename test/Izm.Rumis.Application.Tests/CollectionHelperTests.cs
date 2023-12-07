using Izm.Rumis.Application.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public class CollectionHelperTests
    {
        [Fact]
        public void ComparesTwoCollections()
        {
            var result = CollectionHelper.Compare(new int[] { 1, 2 }, new int[] { 3, 1 }, (a, b) => a == b);

            Assert.Single(result.NotInLeft, n => n == 3);
            Assert.Single(result.NotInRight, n => n == 2);
        }

        [Fact]
        public void UpdatesSet()
        {
            using (var db = ConnectDb())
            {
                const int id = 1;

                db.Entities.Add(new Entity { Id = id });
                db.Children.AddRange(
                    new Child { ParentId = id, Name = "a" },
                    new Child { ParentId = id, Name = "b" },
                    new Child { ParentId = id, Name = "c" });
                db.SaveChanges();

                var entity = db.Entities.FirstOrDefault(t => t.Id == id);

                CollectionHelper.UpdateSet(db.Children, entity.Children, new Child[] {
                        new Child { Name = "c" },
                        new Child { Name = "b" },
                        new Child { Name = "d" },
                        new Child { Name = "e" }
                    }, (e, dto) => e.Name == dto.Name, (e, dto) =>
                    {
                        e.ParentId = id;
                        e.Name = dto.Name;
                    });

                db.SaveChanges();

                var children = db.Entities.FirstOrDefault(t => t.Id == id).Children;

                Assert.Equal(4, children.Count);

                Assert.Contains(children, t => t.Name.Equals("c"));
                Assert.Contains(children, t => t.Name.Equals("b"));
                Assert.Contains(children, t => t.Name.Equals("d"));
                Assert.Contains(children, t => t.Name.Equals("e"));
            }
        }

        private static TestDbContext ConnectDb()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString("N"))
                .Options;

            var db = new TestDbContext(options);

            return db;
        }

        private class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

            public DbSet<Entity> Entities { get; set; }
            public DbSet<Child> Children { get; set; }
        }

        private class Entity
        {
            public int Id { get; set; }
            public virtual ICollection<Child> Children { get; set; }
        }

        private class Child
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public int ParentId { get; set; }
            public virtual Entity Parent { get; set; }
        }

        private class ChildDto
        {
            public string Name { get; set; }
        }
    }
}
