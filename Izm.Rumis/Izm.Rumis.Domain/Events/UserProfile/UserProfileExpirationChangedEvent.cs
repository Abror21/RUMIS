using MediatR;
using System;

namespace Izm.Rumis.Domain.Events.UserProfile
{
    public record UserProfileExpirationChangedEvent(Guid UserProfileId, DateTime Expires) : INotification;
}
