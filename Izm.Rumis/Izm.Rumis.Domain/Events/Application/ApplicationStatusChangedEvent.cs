using MediatR;
using System;

namespace Izm.Rumis.Domain.Events.Application
{
    public record ApplicationStatusChangedEvent(Guid ApplicationId, Guid ApplicationStatusId) : INotification;
}
