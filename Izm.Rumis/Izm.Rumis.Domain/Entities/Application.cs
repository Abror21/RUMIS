using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Events.Application;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Izm.Rumis.Domain.Entities
{
    public class Application : Entity<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string ApplicationNumber { get; set; }
        public DateTime ApplicationDate { get; set; }

        [Column(TypeName = "longtext")]
        public string DeclineReason { get; set; }

        public int? ResourceTargetPersonClassGrade { get; set; }
        public int? MonitoringClassGrade { get; set; }

        [MaxLength(10)]
        public string ResourceTargetPersonClassParallel { get; set; }
        public string MonitoringClassParallel { get; set; }

        [MaxLength(50)]
        public string ResourceTargetPersonGroup { get; set; }
        public string MonitoringGroup { get; set; }

        [MaxLength(200)]
        public string ResourceTargetPersonEducationalProgram { get; set; }

        [Column(TypeName = "longtext")]
        public string ApplicationStatusHistory { get; set; }

        public bool? SocialStatus { get; set; }
        public bool? SocialStatusApproved { get; set; }

        [Column(TypeName = "longtext")]
        public string Notes { get; set; }

        public Guid? ApplicationDuplicateId { get; set; }
        public virtual Application ApplicationDuplicate { get; set; }

        public int EducationalInstitutionId { get; set; }
        public virtual EducationalInstitution EducationalInstitution { get; set; }

        public Guid SubmitterPersonId { get; set; }
        public virtual PersonTechnical SubmitterPerson { get; set; }

        public Guid ContactPersonId { get; set; }
        public virtual PersonTechnical ContactPerson { get; set; }

        public Guid ResourceTargetPersonId { get; set; }
        public virtual PersonTechnical ResourceTargetPerson { get; set; }

        [ClassifierType(ClassifierTypes.ApplicantRole)]
        public Guid SubmitterTypeId { get; set; }
        public virtual Classifier SubmitterType { get; set; }

        [ClassifierType(ClassifierTypes.ResourceTargetPersonType)]
        public Guid ResourceTargetPersonTypeId { get; set; }
        public virtual Classifier ResourceTargetPersonType { get; set; }

        [ClassifierType(ClassifierTypes.ResourceTargetPersonEducationalStatus)]
        public Guid? ResourceTargetPersonEducationalStatusId { get; set; }
        public virtual Classifier ResourceTargetPersonEducationalStatus { get; set; }

        [ClassifierType(ClassifierTypes.ResourceTargetPersonEducationalStatus)]
        public Guid? MonitoringEducationalStatusId { get; protected set; }
        public virtual Classifier MonitoringEducationalStatus { get; protected set; }

        [ClassifierType(ClassifierTypes.ResourceTargetPersonWorkStatus)]
        public Guid? ResourceTargetPersonWorkStatusId { get; set; }
        public virtual Classifier ResourceTargetPersonWorkStatus { get; set; }

        [ClassifierType(ClassifierTypes.ResourceTargetPersonWorkStatus)]
        public Guid? MonitoringWorkStatusId { get; protected set; }
        public virtual Classifier MonitoringWorkStatus { get; protected set; }

        [ClassifierType(ClassifierTypes.ResourceTargetPersonEducationalSubStatus)]
        public Guid? ResourceTargetPersonEducationalSubStatusId { get; set; }
        public virtual Classifier ResourceTargetPersonEducationalSubStatus { get; set; }

        [ClassifierType(ClassifierTypes.ResourceTargetPersonEducationalSubStatus)]
        public Guid? MonitoringEducationalSubStatusId { get; set; }
        public virtual Classifier MonitoringEducationalSubStatus { get; set; }

        [ClassifierType(ClassifierTypes.ApplicationStatus)]
        public Guid ApplicationStatusId { get; protected set; }
        public virtual Classifier ApplicationStatus { get; protected set; }

        [ClassifierType(ClassifierTypes.ResourceSubType)]
        public Guid ResourceSubTypeId { get; set; }
        public virtual Classifier ResourceSubType { get; set; }

        public virtual ICollection<ApplicationSocialStatus> ApplicationSocialStatuses { get; set; } = new List<ApplicationSocialStatus>();
        public virtual ICollection<ApplicationResource> ApplicationResources { get; set; } = new List<ApplicationResource>();

        public Application()
        {
            Id = Guid.NewGuid();
        }

        public Application(Guid applicationStatusId) : this()
        {
            SetApplicationStatus(applicationStatusId);
        }

        public ApplicationResource GetApplicationResource()
        {
            return ApplicationResources.Any(n => PnaStatus.ActiveStatuses.Contains(n.PNAStatus.Code))
                    ? ApplicationResources
                        .Where(n => PnaStatus.ActiveStatuses.Contains(n.PNAStatus.Code))
                        .FirstOrDefault()
                    : ApplicationResources
                        .Where(n => PnaStatus.NonActiveStatuses.Contains(n.PNAStatus.Code))
                        .OrderBy(n => n.Created)
                        .LastOrDefault();
        }

        public void SetApplicationStatus(Guid id)
        {
            ApplicationStatusId = id;

            Events.Add(new ApplicationStatusChangedEvent(Id, ApplicationStatusId));
        }

        public void SetMonitoringEducationalStatus(Guid id)
        {
            MonitoringEducationalStatusId = id;

            Events.Add(new ApplicationPersonStatusChangedEvent(Id, MonitoringEducationalStatusId.Value));
        }

        public void SetMonitoringWorkStatus(Guid id)
        {
            MonitoringWorkStatusId = id;

            Events.Add(new ApplicationPersonStatusChangedEvent(Id, MonitoringWorkStatusId.Value));
        }

    }
}
