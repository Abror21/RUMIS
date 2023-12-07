using Izm.Rumis.Domain.Constants.Classifiers;
using System.Collections.Generic;

namespace Izm.Rumis.Application.Helpers
{
    public static class ApplicationTextTemplateHelper
    {
        public static IDictionary<string, object> CreatePropertyMap(Domain.Entities.Application entity)
        {
            return new Dictionary<string, object>
            {
                { "ApplicationNumber", entity.ApplicationNumber },
                { "EducationalInstitution", entity.EducationalInstitution?.Name },
                { "ResourceTargetPerson", entity.ResourceTargetPerson.ToString() },
                { "ResourceSubType", entity.ResourceSubType?.Value },
                { "PNANumber", entity.GetApplicationResource()?.PNANumber }
            };
        }
    }
}
