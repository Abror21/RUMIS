using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Izm.Rumis.Domain.Entities
{
    public abstract class Entity<T> : IEntity<T>, IAuditable, IEventEntity//, ISoftDeletable
    {
        public T Id { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Modified { get; set; } = DateTime.UtcNow;
        //public bool IsDeleted { get; set; }

        [ForeignKey("CreatedBy")]
        public Guid CreatedById { get; set; }

        [ForeignKey("ModifiedBy")]
        public Guid ModifiedById { get; set; }

        public virtual User CreatedBy { get; set; }
        public virtual User ModifiedBy { get; set; }

        public IList<INotification> Events { get; } = new List<INotification>();
    }
}
