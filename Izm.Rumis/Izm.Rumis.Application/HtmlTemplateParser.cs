using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Izm.Rumis.Application
{
    /// <summary>
    /// HTML template parsing utility with value token replacing support.
    /// </summary>
    public static class HtmlTemplateParser
    {
        /*
            <html>
            <head>
	            <meta charset="utf-8">
	            <style>...</style>
            </head>
            <body>
	            {{t-header}} -- token replacement by template ID
	            <p>{{region}}</p> -- token replacement by global property value
	            <div>{{t-person:persons}}</div> -- token replacement by template ID with provided data
	            <script id="t-header" type="text/template">
		            <header>
			            <img src="data:image/png;base64,...">
			            <div>{{region_header}}</div> -- token replacement by global property value
		            </header>
	            </script>
	            <script id="t-person" type="text/template">
		            <div class="person">
			            <div class="person-name">/{{name}}/</div> -- token replacement by property value from person object
		            </div>
	            </script>
            </body>
            </html>
         */

        public static string Parse(string template, IDictionary<string, object> values)
        {
            return Parse(template, values, null);
        }

        public static string Parse(string template, IDictionary<string, object> values, IDictionary<string, string> inlineTemplates)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            template = template
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\t", string.Empty);

            if (inlineTemplates == null)
            {
                // grab all inline templates
                var templateMatches = Regex.Matches(template,
                    @"<script id=""(t-[a-z0-9-]+)"" type=""text/template"">(((?!<script).)+)</script>",
                    RegexOptions.Multiline | RegexOptions.IgnoreCase);

                inlineTemplates = new Dictionary<string, string>();

                foreach (Match m in templateMatches)
                {
                    inlineTemplates.Add(m.Groups[1].Value, m.Groups[2].Value);

                    // remove inline template from the result
                    template = template.Replace(m.Value, string.Empty);
                }
            }

            // replace simple tokens
            foreach (var pair in values)
            {
                template = template.Replace("{{" + pair.Key + "}}", (pair.Value ?? string.Empty).ToString());
            }

            // process inline templates
            var templateTokenMatches = Regex.Matches(template, @"{{(t-[a-z0-9\-]+)(:[a-z_]+)?}}");

            foreach (Match m in templateTokenMatches)
            {
                string inlineParsed = string.Empty;
                var templateName = m.Groups[1].Value;
                var templateValueKey = m.Groups[2].Value;

                // parse inline template
                if (inlineTemplates.ContainsKey(templateName))
                {
                    var inlineValues = !string.IsNullOrEmpty(templateValueKey)
                        // inline template value must be a collection
                        ? values[templateValueKey.Substring(1)] as IEnumerable<IDictionary<string, object>>
                        : null;

                    if (inlineValues != null)
                    {
                        // process template for every value object
                        foreach (var iv in inlineValues)
                            inlineParsed += Parse(inlineTemplates[templateName], iv, inlineTemplates);
                    }
                    else
                    {
                        inlineParsed = Parse(inlineTemplates[templateName], values, inlineTemplates);
                    }
                }

                template = template.Replace(m.Value, inlineParsed);
            }

            return template;
        }
    }
}
