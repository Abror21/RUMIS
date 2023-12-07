using MediatR;
using System.Collections.Generic;

namespace Izm.Rumis.Domain.Entities
{
    public interface IEventEntity
    {
        IList<INotification> Events { get; }
    }
}
