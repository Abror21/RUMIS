using Izm.Rumis.Application.Models.Application;
using System;

namespace Izm.Rumis.Infrastructure.EServices.Models
{
    public record EServiceApplicationCreateResult(Guid Id, string Number)
    {
        public static EServiceApplicationCreateResult From(ApplicationCreateResult applicationCreateResult)
            => new(applicationCreateResult.Id, applicationCreateResult.Number);
    }
}
