using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Izm.Rumis.Domain.Models.ClassifierPayloads
{
    public sealed class ResourceSubTypePayload
    {
        [JsonPropertyName("resource_type")]
        public string ResourceType { get; set; }

        [JsonPropertyName("groups")]
        public IEnumerable<ResourceParameterGroup> ResourceParameterGroups { get; set; }

        public class ResourceParameterGroup
        {
            [JsonPropertyName("code")]
            public string Code { get; set; }

            [JsonPropertyName("parameters")]
            public IEnumerable<Parameter> Parameters { get; set; }

            public class Parameter
            {
                [JsonPropertyName("code")]
                public string Code { get; set; }

                [JsonPropertyName("isRequired")]
                public bool IsRequired { get; set; }
            }
        }
    }
}