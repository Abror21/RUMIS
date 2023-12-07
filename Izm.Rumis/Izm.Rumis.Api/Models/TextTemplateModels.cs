using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Api.Models
{
    public class TextTemplateModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string Content { get; set; }
    }

    public class TextTemplateEditModel
    {
        [Required]
        [MaxLength(250)]
        public string Title { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [MaxLength(4000)]
        public string Content { get; set; }
    }
}
