using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Infrastructure.ResourceImport.Models
{
    public class ResourcesImportData
    {
        [JsonProperty("Paveida ID")]
        [ClassifierType(ClassifierTypes.ResourceSubType)]
        public Guid ResourceSubTypeId { get; set; }

        [JsonProperty("Nosaukuma ID")]
        [ClassifierType(ClassifierTypes.ResourceModelName)]
        public Guid ModelNameId { get; set; }

        [JsonProperty("Ražotāja ID")]
        [ClassifierType(ClassifierTypes.ResourceManufacturer)]
        public Guid ManufacturerId { get; set; }

        [JsonProperty("Modelis")]
        public string ModelIdentifier { get; set; }

        [Required]
        [JsonProperty("Sērijas Nr.")]
        public string SerialNumber { get; set; }

        [Required]
        [JsonProperty("Inventāra Nr")]
        public string InventoryNumber { get; set; }

        [JsonProperty("Ražošanas gads")]
        public int ManufactureYear { get; set; }

        [JsonProperty("Iestādes piešķirtais nosaukums")]
        public string ResourceName { get; set; }

        [JsonProperty("Ieg.v ID")]
        [ClassifierType(ClassifierTypes.ResourceAcquisitionType)]
        public Guid AcquisitionTypeId { get; set; }

        [JsonProperty("Sociālā atbalsta ID")]
        public bool SocialSupportResource { get; set; }

        [JsonProperty("Izm.m. ID")]
        [ClassifierType(ClassifierTypes.ResourceUsingPurpose)]
        public Guid UsagePurposeTypeId { get; set; }

        [JsonProperty("Mērķa grupas ID")]
        [ClassifierType(ClassifierTypes.TargetGroup)]
        public Guid TargetGroupId { get; set; }

        [JsonProperty("Atraš.v. ID")]
        [ClassifierType(ClassifierTypes.ResourceLocation)]
        public Guid ResourceLocationId { get; set; }

        [JsonProperty("Iegādes vērtība")]
        public decimal AcquisitionsValue { get; set; }
    }
}
