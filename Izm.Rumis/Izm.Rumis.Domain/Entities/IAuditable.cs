using System;

namespace Izm.Rumis.Domain.Entities
{
    public interface IAuditable
    {
        DateTime Created { get; set; }
        DateTime Modified { get; set; }
        Guid CreatedById { get; set; }
        Guid ModifiedById { get; set; }
    }
}
