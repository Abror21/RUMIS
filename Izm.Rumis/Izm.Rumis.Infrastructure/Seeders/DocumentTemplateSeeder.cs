using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Seeders
{
    internal class DocumentTemplateSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            var helper = new SeedHelper(UserIds.Application);

            var templates = db.DocumentTemplates.Select(t => t.Code).ToList();

            Action<string, string, Guid> add = (code, title, resourceTypeId) =>
            {
                if (!templates.Contains(code))
                    db.DocumentTemplates.Add(helper.Audit(new DocumentTemplate
                    {
                        Code = code,
                        Title = title,
                        ResourceTypeId = resourceTypeId
                    }));
            };

            // add document templates here
            // add(DocumentTemplateCode.XXX, "xxx");

            await db.SaveChangesAsync();
        }
    }
}
