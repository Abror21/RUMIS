using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Models;
using System;

namespace Izm.Rumis.Application.Mappers
{
    internal static class GdprAuditaDataMapper
    {
        public static Func<PersonDataProperty, GdprAuditData> Project()
        {
            return property => new GdprAuditData
            {
                Type = property.Type,
                Value = property.Value
            };
        }
    }
}
