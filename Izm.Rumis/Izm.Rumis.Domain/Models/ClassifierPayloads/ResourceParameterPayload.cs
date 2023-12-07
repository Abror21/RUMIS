using System.Text.Json.Serialization;

namespace Izm.Rumis.Domain.Models.ClassifierPayloads
{
    public sealed class ResourceParameterPayload
    {
        [JsonPropertyName("resource_parameter_unit_of_measurement")]
        public string Measurement { get; set; }
    }
}
