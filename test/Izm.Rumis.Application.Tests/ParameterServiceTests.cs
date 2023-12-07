using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public class ParameterServiceTests
    {
        [Fact]
        public void Get_ReturnsData()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                db.Parameters.AddRange(
                    new Parameter { Code = "x", Value = "x" },
                    new Parameter { Code = "x", Value = "x" });

                db.SaveChanges();

                var data = CreateService(db).Get().List();

                Assert.Equal(2, data.Count());
            }
        }

        [Fact]
        public void GetValue_ReturnsData()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const string code = "a";
                const string value = "1";

                db.Parameters.AddRange(
                    new Parameter { Id = 1, Code = code, Value = value });

                db.SaveChanges();

                var dbValue = CreateService(db).GetValue(code);

                Assert.Equal(value, dbValue);
            }
        }

        [Fact]
        public void FindValue_ReturnsValue()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const string code = "a";
                const string value = "1";

                var foundValue = CreateService(db).FindValue(code, new List<Parameter>
                {
                    new Parameter { Code = code, Value = value }
                });

                Assert.Equal(value, foundValue);
            }
        }

        [Fact]
        public void FindValue_ReturnsNull()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                var foundValue = CreateService(db).FindValue("a", new List<Parameter>());
                Assert.Null(foundValue);
            }
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const int id = 1;
                const string value = "a";

                db.Parameters.AddRange(new Parameter { Id = id, Code = "x", Value = "x" });
                db.SaveChanges();

                await CreateService(db).UpdateAsync(id, value);

                var item = db.Parameters.FirstOrDefault(t => t.Id == id);

                Assert.Equal(value, item.Value);
            }
        }

        [Fact]
        public async Task Update_ThrowsNotFound()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                {
                    return CreateService(db).UpdateAsync(2, "a");
                });
            }
        }

        private ParameterService CreateService(AppDbContext db)
        {
            return new ParameterService(db);
        }
    }
}
