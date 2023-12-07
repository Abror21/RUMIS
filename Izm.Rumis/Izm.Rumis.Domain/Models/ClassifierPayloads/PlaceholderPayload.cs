using System.Text.Json.Serialization;

namespace Izm.Rumis.Domain.Models.ClassifierPayloads
{
    public sealed class PlaceholderPayload
    {
        [JsonPropertyName("test_value")]
        public string Value { get; set; }
    }
}
