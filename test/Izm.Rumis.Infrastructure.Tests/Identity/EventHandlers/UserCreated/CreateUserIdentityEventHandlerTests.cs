using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Events.User;
using Izm.Rumis.Infrastructure.Identity;
using Izm.Rumis.Infrastructure.Identity.EventHandlers.UserCreated;
using Izm.Rumis.Infrastructure.Tests.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Infrastructure.Tests.Identity.EventHandlers.UserCreated
{
    public sealed class CreateUserIdentityEventHandlerTests
    {
        [Fact]
        public async Task Handle_Succeeds_NoPerson()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var userCreatedEvent = new UserCreatedEvent(User.Create());

            var handler = GetHandler(db);

            // Act
            await handler.Handle(userCreatedEvent);

            // Assert
            Assert.Empty(db.IdentityUsers);
        }

        [Fact]
        public async Task Handle_Succeeds_HasPerson()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            await db.SaveChangesAsync();

            var personTechnical = new PersonTechnical
            {
                User = User.Create()
            };

            db.PersonTechnicals.Add(personTechnical);

            var userCreatedEvent = new UserCreatedEvent(personTechnical.User);

            var handler = GetHandler(db);

            // Act
            await handler.Handle(userCreatedEvent);

            var idnetityUser = db.IdentityUsers.Local.First();

            // Assert
            Assert.Single(idnetityUser.Logins);
            Assert.Equal(UserAuthType.Adfs, idnetityUser.Logins.First().AuthType);
            Assert.False(idnetityUser.IsDisabled);
        }

        private CreateUserIdentityEventHandler GetHandler(IIdentityDbContext db)
        {
            return new CreateUserIdentityEventHandler(db);
        }
    }
}
