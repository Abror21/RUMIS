using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Events.ApplicationResource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Izm.Rumis.Domain.Entities
{
    public class ApplicationResource : Entity<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string PNANumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AssignedResourceReturnDate { get; protected set; }

        [Column(TypeName = "longtext")]
        public string Notes { get; set; }

        public DateTime? ReturnResourceDate { get; set; }

        public Guid ApplicationId { get; set; }
        public virtual Application Application { get; set; }

        public Guid? AssignedResourceId { get; set; }
        public virtual Resource AssignedResource { get; set; }

        [ClassifierType(ClassifierTypes.PnaCancelingReason)]
        public Guid? CancelingReasonId { get; set; }
        public virtual Classifier CancelingReason { get; set; }

        [Column(TypeName = "longtext")]
        public string CancelingDescription { get; set; }

        [ClassifierType(ClassifierTypes.ResourceStatus)]
        public Guid? AssignedResourceStateId { get; set; }
        public virtual Classifier AssignedResourceState { get; set; }

        [ClassifierType(ClassifierTypes.PnaStatus)]
        public Guid PNAStatusId { get; protected set; }
        public virtual Classifier PNAStatus { get; protected set; }

        [ClassifierType(ClassifierTypes.ResourceReturnStatus)]
        public Guid? ReturnResourceStateId { get; set; }
        public virtual Classifier ReturnResourceState { get; set; }

        public virtual ICollection<ApplicationResourceAttachment> ApplicationResourceAttachmentList { get; protected set; } = new List<ApplicationResourceAttachment>();
        public virtual ICollection<ApplicationResourceContactPerson> ApplicationResourceContactPersons { get; protected set; } = new List<ApplicationResourceContactPerson>();

        public ApplicationResource()
        {
            Id = Guid.NewGuid();

            Events.Add(new ApplicationResourceCreatedEvent(Id));
        }

        public void SetApplicationResourceReturnDeadline(DateTime? date)
        {
            if (AssignedResourceReturnDate == date)
                return;

            AssignedResourceReturnDate = date;

            if (!Events.Any(t => t.GetType() == typeof(ApplicationResourceCreatedEvent)))
                Events.Add(new ApplicationResourceReturnDeadlineChangedEvent(Id, date));
        }

        public void SetPnaStatus(Guid statusId)
        {
            if (PNAStatusId == statusId)
                return;

            PNAStatusId = statusId;

            Events.Add(new ApplicationResourcePnaStatusChangedEvent(Id, PNAStatusId));
        }
    }
}
