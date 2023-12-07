using MediatR;
using System;

namespace Izm.Rumis.Domain.Events.Application
{
    public record ApplicationPersonStatusChangedEvent(Guid ApplicationId, Guid StatusId) : INotification;
}
