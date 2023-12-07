using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Izm.Rumis.Application.Helpers
{
    public static class ApplicationResourceHtmlTemplateHelper
    {
        private const string dateFormat = "dd.MM.yyyy";

        public static IDictionary<string, object> CreateProperyMap(ApplicationResource entity)
        {
            return new Dictionary<string, object>
            {
                { Placeholder.ResourceAcquisitionsValue, entity.AssignedResource == null ? string.Empty : entity.AssignedResource.AcquisitionsValue.ToString() },
                { Placeholder.DueDate, entity.AssignedResourceReturnDate == null ? string.Empty : entity.AssignedResourceReturnDate.Value.ToString(dateFormat) },
                { Placeholder.IssuedDate, entity.ApplicationResourceAttachmentList.Any(t => t.DocumentType.Code == DocumentType.PNA) 
                    ? entity.ApplicationResourceAttachmentList.First(t => t.DocumentType.Code == DocumentType.PNA).DocumentDate.ToString(dateFormat) 
                    : string.Empty },
                { Placeholder.EducationalInstitution, entity.Application.EducationalInstitution.Name },
                { Placeholder.ResourceInventoryNumber, entity.AssignedResource == null ? string.Empty : entity.AssignedResource.InventoryNumber },
                { Placeholder.ResourceName, entity.AssignedResource == null ? string.Empty : entity.AssignedResource.ToString() },
                { Placeholder.ResourceModelIdentifier, entity.AssignedResource == null ? string.Empty : entity.AssignedResource.ModelIdentifier },
                { Placeholder.NotesIssued,  entity.AssignedResource == null ? string.Empty : entity.AssignedResource.Notes },
                { Placeholder.PnaNumber, entity.PNANumber },
                { Placeholder.User, entity.Application.ResourceTargetPerson.ToString() },
                { Placeholder.ResourceSerialNumber, entity.AssignedResource == null ? string.Empty : entity.AssignedResource.SerialNumber },
                { Placeholder.Recipient, entity.Application.SubmitterPerson.ToString() },
            };
        }
    }
}
