using System.Text.Json.Serialization;

namespace Izm.Rumis.Infrastructure.Vraa.Models
{
    public class IntrospectResult
    {
        [JsonPropertyName("active")]
        public string Active { get; set; }

        [JsonPropertyName("given_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("family_name")]
        public string LastName { get; set; }

        //[JsonPropertyName("legalentity")]
        //public string LegalEntity { get; set; }

        [JsonPropertyName("ppid")]
        public string PrivatePersonalIdentifier { get; set; }
    }
}
