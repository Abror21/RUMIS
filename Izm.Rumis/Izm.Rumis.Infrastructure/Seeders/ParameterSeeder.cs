using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Seeders
{
    internal static class ParameterSeeder
    {
        public static async Task SeedAsync(AppDbContext db, string appUrl)
        {
            var helper = new SeedHelper(UserIds.Application);
            var csvItems = helper.ReadFromCsv<ImportParameter>("parameters.csv");
            var exCodes = db.Parameters.Select(t => t.Code).ToList();

            foreach (var paramToImport in csvItems)
            {
                if (!string.IsNullOrEmpty(paramToImport.Code) && !exCodes.Contains(paramToImport.Code))
                {
                    db.Parameters.Add(helper.Audit(new Parameter
                    {
                        Code = paramToImport.Code,
                        Value = paramToImport.Value
                    }));
                }
            }

            var appUrlParam = db.Parameters.FirstOrDefault(t => t.Code == ParameterCode.AppUrl);

            if (appUrlParam == null)
            {
                appUrlParam = helper.Audit(new Parameter { Code = ParameterCode.AppUrl });
                db.Parameters.Add(appUrlParam);
            }

            appUrlParam.Value = appUrl;

            await db.SaveChangesAsync();
        }

        private class ImportParameter
        {
            public string Code { get; set; }
            public string Value { get; set; }
        }
    }
}
