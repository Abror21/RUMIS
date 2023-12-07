using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class Supervisor : Entity<int>
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        public bool? IsActive { get; set; }
        public string Type { get; set; }

        public virtual ICollection<DocumentTemplate> DocumentTemplates { get; set; } = new List<DocumentTemplate>();
        public virtual ICollection<EducationalInstitution> EducationalInstitutions { get; protected set; } = new List<EducationalInstitution>();
    }
}
