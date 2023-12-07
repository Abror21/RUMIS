using Izm.Rumis.Domain.Entities;
using System;

namespace Izm.Rumis.Application.Common
{
    public class AuditableEntity<T> where T : class, IAuditable
    {
        public T Entity { get; set; }
        public AuditableEntityUser CreatedBy { get; set; }
        public AuditableEntityUser ModifiedBy { get; set; }
    }

    public class AuditableEntityUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
