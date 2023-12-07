using MediatR;
using System;

namespace Izm.Rumis.Domain.Events.UserProfile
{
    public record UserProfileDisabledEvent(Guid UserProfileId) : INotification;
}
