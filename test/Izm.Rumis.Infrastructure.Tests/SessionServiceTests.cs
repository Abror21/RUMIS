using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Infrastructure.Sessions;
using Izm.Rumis.Infrastructure.Tests.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Infrastructure.Tests
{
    public sealed class SessionServiceTests
    {
        [Fact]
        public async Task DeleteAsync_Succeeds()
        {
            // Assign
            var sessionId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Sessions.Add(new Session()
            {
                Id = sessionId
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.DeleteAsync(sessionId);

            // Assert
            Assert.Empty(db.Sessions);
        }

        [Fact]
        public async Task DeleteAsync_Throws_NotFound()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            _ = Assert.ThrowsAsync<EntityNotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleterRangeAsync_Succeeds()
        {
            // Assign
            var sessionId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Sessions.Add(new Session()
            {
                Id = sessionId
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.DeleteRangeAsync(new Guid[] { sessionId });

            // Assert
            Assert.Empty(db.Sessions);
        }

        [Fact]
        public async Task CreateAsync_CheckData()
        {
            // Assign
            var sessionId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;

            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act
            await service.CreateAsync(sessionId);

            var session = db.Sessions.First();

            // Assert
            Assert.Equal(sessionId, session.Id);
            Assert.True(utcNow < session.Created);
        }

        [Fact]
        public async Task CreateAsync_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act
            await service.CreateAsync(Guid.NewGuid());

            // Assert
            Assert.Single(db.Sessions);
        }

        [Fact]
        public async Task CreateAsync_Throws_AlreadyExists()
        {
            // Assign
            var sessionid = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Sessions.Add(new Session
            {
                Id = sessionid
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => service.CreateAsync(sessionid));
        }

        private SessionService GetService(ISessionDbContext db)
        {
            return new SessionService(
                db: db
                );
        }
    }
}
