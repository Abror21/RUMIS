using Izm.Rumis.Domain.Events.User;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Identity.EventHandlers.UserCreated
{
    public sealed class CreateUserIdentityEventHandler : INotificationHandler<UserCreatedEvent>
    {
        private readonly IIdentityDbContext db;

        public CreateUserIdentityEventHandler(IIdentityDbContext db)
        {
            this.db = db;
        }

        public Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken = default)
        {
            if (notification.User.PersonTechnical == null)
                return Task.CompletedTask;

            var identityUser = new IdentityUser
            {
                User = notification.User,
                Logins = new List<IdentityUserLogin>
                {
                    new IdentityUserLogin
                    {
                        AuthType = UserAuthType.Adfs
                    }
                }
            };

            db.IdentityUsers.Add(identityUser);

            return Task.CompletedTask;
        }
    }
}
