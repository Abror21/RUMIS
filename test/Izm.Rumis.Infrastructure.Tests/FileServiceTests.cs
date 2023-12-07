using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Services;
using Izm.Rumis.Infrastructure.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Infrastructure.Tests
{
    public class FileServiceTests
    {
        public FileServiceTests()
        {
            this.testId = Guid.NewGuid();
            this.anotherTestId = Guid.NewGuid();
            this.testIds = new List<Guid> { testId, anotherTestId };
            this.testContent = Encoding.UTF8.GetBytes("test");
        }

        private readonly Guid testId;
        private readonly Guid anotherTestId;
        private readonly IEnumerable<Guid> testIds;
        private readonly byte[] testContent;

        [Fact]
        public async Task Get_Throws_EmptyId()
        {
            using var db = ServiceFactory.ConnectDb();

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            {
                return CreateService(db).GetAsync(Guid.Empty);
            });

            Assert.Equal("file.emptyId", ex.Message);
        }

        [Fact]
        public async Task Get_Throws_NotFound()
        {
            using var db = ServiceFactory.ConnectDb();

            var ex = await Assert.ThrowsAsync<Exception>(() =>
            {
                return CreateService(db).GetAsync(testId);
            });

            Assert.Equal("file.notFound", ex.Message);
        }

        [Fact]
        public async Task Get_Returns()
        {
            using var db = ServiceFactory.ConnectDb();

            db.Files.AddRange(
                new File { Name = "x" },
                new File { Id = testId, Name = "x" }
            );

            db.SaveChanges();

            var data = await CreateService(db).GetAsync(testId);

            Assert.NotNull(data);
            Assert.Equal(testId, data.Id);
        }

        [Fact]
        public async Task AddOrUpdate_Throws_EmptyId()
        {
            using var db = ServiceFactory.ConnectDb();

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            {
                return CreateService(db).AddOrUpdateAsync(Guid.Empty, new FileDto());
            });

            Assert.Equal("file.emptyId", ex.Message);
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        public async Task AddOrUpdate_Throws_EmptyContent(bool nullDto, bool nullContent, bool emptyContent)
        {
            using var db = ServiceFactory.ConnectDb();

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            {
                FileDto dto = null;

                if (nullDto)
                    dto = null;
                else if (nullContent)
                    dto = new FileDto { Content = null };
                else if (emptyContent)
                    dto = new FileDto { Content = new byte[] { } };

                return CreateService(db).AddOrUpdateAsync(testId, dto);
            });

            Assert.Equal("file.emptyContent", ex.Message);
        }

        [Fact]
        public async Task AddOrUpdate_Throws_NoFileName()
        {
            using var db = ServiceFactory.ConnectDb();

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            {
                return CreateService(db).AddOrUpdateAsync(testId, new FileDto
                {
                    Content = testContent
                });
            });

            Assert.Equal("file.emptyFileName", ex.Message);
        }

        [Fact]
        public async Task Add_Succeeds()
        {
            const string filename = "test.txt";

            using var db = ServiceFactory.ConnectDb();

            await CreateService(db).AddOrUpdateAsync(testId, new FileDto
            {
                Content = testContent,
                FileName = filename
            });

            var file = db.Files.FirstOrDefault(t => t.Id == testId);

            Assert.NotNull(file);
            Assert.Equal(filename, file.Name);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            const string filename = "test.txt";

            using var db = ServiceFactory.ConnectDb();

            db.Files.Add(new File { Id = testId, Name = "x" });

            db.SaveChanges();

            await CreateService(db).AddOrUpdateAsync(testId, new FileDto
            {
                Content = testContent,
                FileName = filename
            });

            var file = db.Files.FirstOrDefault(t => t.Id == testId);

            Assert.NotNull(file);
            Assert.Equal(filename, file.Name);
        }

        [Fact]
        public async Task Delete_Throws_EmptyId()
        {
            using var db = ServiceFactory.ConnectDb();

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            {
                return CreateService(db).DeleteAsync(Guid.Empty);
            });

            Assert.Equal("file.emptyId", ex.Message);
        }

        [Fact]
        public async Task Delete_Throws_NotFound()
        {
            using var db = ServiceFactory.ConnectDb();

            var ex = await Assert.ThrowsAsync<Exception>(() =>
            {
                return CreateService(db).DeleteAsync(testId);
            });

            Assert.Equal("file.deleteFailed", ex.Message);
        }

        [Fact]
        public async Task Delete_Succeeds()
        {
            using var db = ServiceFactory.ConnectDb();

            db.Files.Add(new File { Id = testId, Name = "x" });

            db.SaveChanges();

            await CreateService(db).DeleteAsync(testId);

            var file = db.Files.FirstOrDefault(t => t.Id == testId);

            Assert.Null(file);
        }

        [Fact]
        public async Task DeleteRange_Throws_NotFound()
        {
            using var db = ServiceFactory.ConnectDb();

            var ex = await Assert.ThrowsAsync<Exception>(() =>
            {
                return CreateService(db).DeleteRangeAsync(testIds);
            });

            Assert.Equal("file.deleteFailed", ex.Message);
        }

        [Fact]
        public async Task DeleteRange_Succeeds()
        {
            using var db = ServiceFactory.ConnectDb();

            db.Files.Add(new File { Id = testId, Name = "x" });
            db.Files.Add(new File { Id = anotherTestId, Name = "y" });

            db.SaveChanges();

            await CreateService(db).DeleteRangeAsync(testIds);

            var file = db.Files.FirstOrDefault();

            Assert.Null(file);
        }

        private FileService CreateService(AppDbContext db)
        {
            return new FileService(db, new AmazonS3ServiceFake(), new FileServiceOptions(), new LoggerFake<FileService>());
        }
    }
}
