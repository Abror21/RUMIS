using MediatR;

namespace Izm.Rumis.Domain.Events.UserProfile
{
    public record UserProfileCreatedEvent(Entities.UserProfile UserProfile) : INotification;
}
