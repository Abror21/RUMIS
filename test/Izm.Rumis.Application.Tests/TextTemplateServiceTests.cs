using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public class TextTemplateServiceTests
    {
        [Fact]
        public void Get_ReturnsData()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                db.TextTemplates.AddRange(
                    new TextTemplate { Code = "x", Title = "x" },
                    new TextTemplate { Code = "x", Title = "x" });

                db.SaveChanges();

                var data = CreateService(db).Get().List();

                Assert.Equal(2, data.Count());
            }
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const string code = "c";

                await CreateService(db).CreateAsync(new TextTemplateEditDto
                {
                    Code = code,
                    Title = "x"
                });

                var exists = db.TextTemplates.Any(t => t.Code == code);

                Assert.True(exists);
            }
        }

        [Fact]
        public async Task Create_ThrowsValidation_EmptyCode()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                await Assert.ThrowsAsync<ValidationException>(() =>
                {
                    return CreateService(db).CreateAsync(new TextTemplateEditDto { Title = "x" });
                });
            }
        }

        [Fact]
        public async Task Create_ThrowsValidation_EmptyTitle()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                await Assert.ThrowsAsync<ValidationException>(() =>
                {
                    return CreateService(db).CreateAsync(new TextTemplateEditDto { Code = "x" });
                });
            }
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const int id = 1;
                const string content = "a";

                db.TextTemplates.AddRange(new TextTemplate { Id = id, Code = "x", Title = "x" });
                db.SaveChanges();

                await CreateService(db).UpdateAsync(id, new TextTemplateEditDto
                {
                    Code = "x",
                    Title = "x",
                    Content = content
                });

                var item = db.TextTemplates.FirstOrDefault(t => t.Id == id);

                Assert.Equal(content, item.Content);
            }
        }

        [Fact]
        public async Task Update_ThrowsNotFound()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                {
                    return CreateService(db).UpdateAsync(1, new TextTemplateEditDto());
                });
            }
        }

        [Fact]
        public async Task Update_ThrowsValidation_EmptyCode()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const int id = 1;

                db.TextTemplates.AddRange(new TextTemplate { Id = id, Code = "x", Title = "x" });
                db.SaveChanges();

                await Assert.ThrowsAsync<ValidationException>(() =>
                {
                    return CreateService(db).UpdateAsync(id, new TextTemplateEditDto { Title = "x" });
                });
            }
        }

        [Fact]
        public async Task Update_ThrowsValidation_EmptyTitle()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const int id = 1;

                db.TextTemplates.AddRange(new TextTemplate { Id = id, Code = "x", Title = "x" });
                db.SaveChanges();

                await Assert.ThrowsAsync<ValidationException>(() =>
                {
                    return CreateService(db).UpdateAsync(id, new TextTemplateEditDto { Code = "x" });
                });
            }
        }

        [Fact]
        public async Task Delete_Succeeds()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const int id = 1;

                db.TextTemplates.AddRange(new TextTemplate { Id = id, Code = "x", Title = "x" });
                db.SaveChanges();

                await CreateService(db).DeleteAsync(id);

                var exists = db.TextTemplates.Any(t => t.Id == id);

                Assert.False(exists);
            }
        }

        [Fact]
        public async Task Delete_ThrowsNotFound()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                {
                    return CreateService(db).DeleteAsync(1);
                });
            }
        }

        private TextTemplateService CreateService(AppDbContext db)
        {
            return new TextTemplateService(db);
        }
    }
}
