using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Seeders
{
    internal class TextTemplateSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            var helper = new SeedHelper(UserIds.Application);

            var csvItems = helper.ReadFromCsv<ImportTextTemplate>("text_templates.csv");
            var exCodes = await db.TextTemplates
                .Select(t => t.Code)
                .ToArrayAsync();

            foreach (var textTemplateToImport in csvItems)
            {
                if (string.IsNullOrEmpty(textTemplateToImport.Code) || exCodes.Contains(textTemplateToImport.Code))
                    continue;

                db.TextTemplates.Add(helper.Audit(new TextTemplate
                {
                    Code = textTemplateToImport.Code,
                    Content = textTemplateToImport.Content,
                    Title = textTemplateToImport.Title
                }));
            }

            await db.SaveChangesAsync();
        }

        private class ImportTextTemplate
        {
            public string Code { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
        }
    }
}
