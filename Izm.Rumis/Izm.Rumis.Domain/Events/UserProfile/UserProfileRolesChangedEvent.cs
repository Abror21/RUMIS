using MediatR;
using System;

namespace Izm.Rumis.Domain.Events.UserProfile
{
    public record UserProfileRolesChangedEvent(Guid UserProfileId) : INotification;
}
