using System.Collections.Generic;
using System.Text;

namespace Izm.Rumis.Application
{
    public static class TextTemplateParser
    {
        private const string prefix = "{{";
        private const string suffix = "}}";

        public static string Parse(string textTemplate, IDictionary<string, object> data)
        {
            var builder = new StringBuilder(textTemplate);

            foreach (var property in data)
                builder.Replace(GetToken(property.Key), property.Value?.ToString() ?? string.Empty);

            return builder.ToString();
        }

        private static string GetToken(string key) => $"{prefix}{key}{suffix}";
    }
}
