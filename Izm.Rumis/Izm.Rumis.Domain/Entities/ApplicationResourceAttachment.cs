using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using System;

namespace Izm.Rumis.Domain.Entities
{
    public class ApplicationResourceAttachment : Entity<Guid>
    {
        public DateOnly DocumentDate { get; set; }

        public Guid ApplicationResourceId { get; set; }
        public virtual ApplicationResource ApplicationResource { get; set; }

        [ClassifierType(ClassifierTypes.DocumentType)]
        public Guid? DocumentTypeId { get; set; }
        public virtual Classifier DocumentType { get; set; }

        public int? DocumentTemplateId { get; set; }
        public virtual DocumentTemplate DocumentTemplate { get; set; }

        public Guid? FileId { get; set; }
        public virtual File File { get; set; }
    }
}
