using System;

namespace Izm.Rumis.Infrastructure.ResourceImport.Models
{
    public class ResourcesImportDataProperty
    {
        public Type Type { get; set; }
        public bool IsRequired { get; set; }
        public string ClassifierType { get; set; }
    }
}
