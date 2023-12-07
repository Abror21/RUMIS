using Izm.Rumis.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class GdprAuditData
    {
        public Guid Id { get; set; }

        public PersonDataType Type { get; set; }

        [MaxLength(100)]
        public string Value { get; set; }

        public Guid GdprAuditId { get; set; }
        public virtual GdprAudit Gdpr { get; set; }
    }
}
