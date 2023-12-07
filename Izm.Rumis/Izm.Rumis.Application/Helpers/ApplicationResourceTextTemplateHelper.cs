using Izm.Rumis.Domain.Entities;
using System.Collections.Generic;

namespace Izm.Rumis.Application.Helpers
{
    public static class ApplicationResourceTextTemplateHelper
    {
        public static IDictionary<string, object> CreatePropertyMap(ApplicationResource entity)
        {
            return new Dictionary<string, object>
            {
                { "ApplicationNumber", entity.Application.ApplicationNumber },
                { "EducationalInstitution", entity.Application.EducationalInstitution.Name },
                { "ResourceSubType", entity.Application.ResourceSubType.Value }
            };
        }
    }
}
