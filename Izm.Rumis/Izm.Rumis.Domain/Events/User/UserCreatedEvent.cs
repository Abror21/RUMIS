using MediatR;

namespace Izm.Rumis.Domain.Events.User
{
    public record UserCreatedEvent(Entities.User User) : INotification;
}
