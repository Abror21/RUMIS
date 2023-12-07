using MediatR;
using System;

namespace Izm.Rumis.Domain.Events.UserProfile
{
    public record UserProfileEnabledEvent(Guid UserProfileId) : INotification;
}
