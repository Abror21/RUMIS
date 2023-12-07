using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Common;
using Izm.Rumis.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure
{
    public static class AppDbContextSetup
    {
        public static async Task MigrateAsync(AppDbContext context)
        {
            context.Database.SetCommandTimeout(TimeSpan.FromHours(1));

            await context.Database.MigrateAsync();
        }

        public static async Task SeedAsync(AppDbContext context, AppDbContextSeedOptions options, IdentityOptions identityOptions)
        {
            // current modif script is supported only by SqlServer
            //SetupModif(context);

            EnsureSystemUsers(context);

            var dataVersionParam = context.Parameters.Where(t => t.Code == ParameterCode.DataVersion).FirstOrDefault();
            int dataVersion = dataVersionParam == null || string.IsNullOrEmpty(dataVersionParam.Value)
                ? 0 : int.Parse(dataVersionParam.Value);

            int newDataVersion = dataVersion;

            await ParameterSeeder.SeedAsync(context, options.AppUrl);
            await RoleSeeder.SeedAsync(context);
            await IdentitySeeder.SeedAsync(context, context, identityOptions);
            await TextTemplateSeeder.SeedAsync(context);
            await DocumentTemplateSeeder.SeedAsync(context);

            var classifierVersion = await ClassifierSeeder.SeedAsync(context, dataVersion);

            if (classifierVersion > newDataVersion)
                newDataVersion = classifierVersion;

            if (dataVersionParam == null)
            {
                dataVersionParam = new Parameter
                {
                    Code = ParameterCode.DataVersion,
                    Created = DateTime.UtcNow,
                    CreatedById = UserIds.Application
                };

                context.Parameters.Add(dataVersionParam);
            }
            else
            {
                context.Entry(dataVersionParam).State = EntityState.Modified;
            }

            dataVersionParam.Modified = DateTime.UtcNow;
            dataVersionParam.ModifiedById = UserIds.Application;
            dataVersionParam.Value = newDataVersion.ToString();

            context.SaveChanges();
        }

        private static void EnsureSystemUsers(AppDbContext context)
        {
            var users = new User[]
            {
                User.Create(
                    id: UserIds.Application,
                    name: UserNames.Application,
                    isTechnical: true,
                    isHidden: true
                    ),
                User.Create(
                    id: UserIds.EServiceUser,
                    name: UserNames.EServiceUser,
                    isTechnical: true,
                    isHidden: true
                    ),
                User.Create(
                    id: UserIds.Tasks,
                    name: UserNames.Tasks,
                    isTechnical: true,
                    isHidden: true
                    )
            };

            foreach (var user in users)
            {
                var exists = context.Users.Any(t => t.Id == user.Id);

                if (exists)
                    continue;

                context.Users.Add(user);
            }

            // ensure empty/anonymous user is not in the database
            var emptyUser = context.Users.Find(UserIds.Anonymous);

            if (emptyUser != null)
                context.Users.Remove(emptyUser);

            context.SaveChanges();
        }

        private static void SetupModif(AppDbContext context)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"{assembly.GetName().Name}.Scripts.CreateModifProcedure.sql";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var content = reader.ReadToEnd();
                context.Database.ExecuteSqlRaw("if object_id('dbo.Generate_UpdateModifTable', 'P') is not null drop procedure [dbo].[Generate_UpdateModifTable]");
                context.Database.ExecuteSqlRaw(content);
            }

            var tables = new string[]
            {
                "Parameters",
                "Classifiers",
                "TextTemplates",
                "DocumentTemplates",
                "UserProfiles",
                // add custom tables here
            };

            Action run = () =>
            {
                foreach (var table in tables)
                    context.Database.ExecuteSqlRaw($"exec Generate_UpdateModifTable @TableName = '{table}'");

                context.SaveChanges();
            };

            run();
        }
    }

    public class AppDbContextSeedOptions
    {
        public string AppUrl { get; set; }
    }
}
