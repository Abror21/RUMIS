using Izm.Rumis.Infrastructure;
using System.Linq;

namespace Izm.Rumis.Application.Tests.Common
{
    internal static class Data
    {
        public static void Seed(AppDbContext db)
        {
            var seeder = new Seeder(db);

            seeder.Save();

            System.Diagnostics.Debug.WriteLine($"Classifier values added: {db.Classifiers.Count()}.");
        }

        public static class ClassifierId
        {
        }
    }
}
