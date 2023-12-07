using MediatR;
using System;

namespace Izm.Rumis.Domain.Events.ApplicationResource
{
    public record class ApplicationResourceCreatedEvent(Guid ApplicationResourceId) : INotification;
}
