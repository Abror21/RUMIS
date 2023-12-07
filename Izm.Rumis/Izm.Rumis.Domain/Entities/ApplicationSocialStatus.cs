using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using System;

namespace Izm.Rumis.Domain.Entities
{
    public class ApplicationSocialStatus : Entity<Guid>
    {
        public Guid ApplicationId { get; set; }
        public virtual Application Application { get; set; }

        [ClassifierType(ClassifierTypes.SocialStatus)]
        public Guid SocialStatusId { get; set; }
        public virtual Classifier SocialStatus { get; set; }

        public bool? SocialStatusApproved { get; set; }
    }
}
