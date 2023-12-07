using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Izm.Rumis.Domain.Entities
{
    public class ResourceParameter : Entity<Guid>
    {
        [MaxLength(100)]
        public string Value { get; set; }

        [ClassifierType(ClassifierTypes.ResourceParameter)]
        public Guid? ParameterId { get; set; }
        public virtual Classifier Parameter { get; set; }
    }
}
