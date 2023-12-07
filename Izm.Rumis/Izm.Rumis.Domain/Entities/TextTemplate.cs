using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class TextTemplate : Entity<int>
    {
        [Required]
        [MaxLength(100)]
        public string Code { get; set; }

        [Required]
        [MaxLength(250)]
        public string Title { get; set; }

        public string Content { get; set; }
    }
}
