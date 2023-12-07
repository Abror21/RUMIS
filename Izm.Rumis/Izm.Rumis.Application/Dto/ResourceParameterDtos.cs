using System;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Application.Dto
{
    public class ResourceParameterDto
    {
        public Guid? Id { get; set; }
        [MaxLength(100)]
        public string Value { get; set; }
        public Guid? ParameterId { get; set; }
    }
}
