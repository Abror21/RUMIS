using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Api.Models
{
    public class ParameterModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
    }

    public class ParameterUpdateModel
    {
        [Required]
        [MaxLength(250)]
        public string Value { get; set; }
    }
}
