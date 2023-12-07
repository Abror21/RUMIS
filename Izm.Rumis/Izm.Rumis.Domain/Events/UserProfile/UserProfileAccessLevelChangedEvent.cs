using Izm.Rumis.Domain.Models;
using MediatR;
using System;

namespace Izm.Rumis.Domain.Events.UserProfile
{
    public record UserProfileAccessLevelChangedEvent(Guid UserProfileId, AccessLevel AccessLevel) : INotification;
}
