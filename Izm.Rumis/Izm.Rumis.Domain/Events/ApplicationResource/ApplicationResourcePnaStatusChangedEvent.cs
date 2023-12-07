using MediatR;
using System;

namespace Izm.Rumis.Domain.Events.ApplicationResource
{
    public sealed record ApplicationResourcePnaStatusChangedEvent(Guid ApplicationResourceId, Guid PnaStatusId) : INotification;
}
