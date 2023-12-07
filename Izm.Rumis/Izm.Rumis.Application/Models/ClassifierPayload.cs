using Newtonsoft.Json;

namespace Izm.Rumis.Application.Models
{
    public static class ClassifierPayload
    {
        public static TModel Parse<TModel>(string payload) where TModel : class, new()
        {
            return string.IsNullOrEmpty(payload) ? new TModel() : JsonConvert.DeserializeObject<TModel>(payload);
        }
    }

    // define known classifier payload classes here
    public class ClassifierTypePayload
    {
        public string Group { get; set; }
        public bool? InEServices { get; set; }
    }
}
