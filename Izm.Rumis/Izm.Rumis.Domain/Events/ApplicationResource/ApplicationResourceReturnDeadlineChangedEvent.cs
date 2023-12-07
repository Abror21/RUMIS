using MediatR;
using System;

namespace Izm.Rumis.Domain.Events.ApplicationResource
{
    public record ApplicationResourceReturnDeadlineChangedEvent(Guid ApplicationResourceId, DateTime? AssignedResourceReturnDate) : INotification;
}
