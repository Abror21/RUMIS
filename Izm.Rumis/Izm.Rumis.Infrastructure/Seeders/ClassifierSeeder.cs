using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Seeders
{
    internal static class ClassifierSeeder
    {
        public static async Task<int> SeedAsync(AppDbContext db, int currentVersion)
        {
            var helper = new SeedHelper(UserIds.Application);
            int newDataVersion = currentVersion;
            var csvItems = helper.ReadFromCsv<ImportClassifier>("classifiers.csv");

            var updateItems = csvItems.Where(t => t.Version > currentVersion).ToList();
            var deleteItems = csvItems.Where(t => t.Version == -1).ToList();

            bool hasDelete = deleteItems.Any();
            bool hasUpdate = updateItems.Any();

            if (hasDelete || hasUpdate)
            {
                var exItems = await db.Classifiers.ToListAsync();

                if (hasDelete)
                {
                    int buffer = 0;

                    for (int i = 0; i < deleteItems.Count; i++)
                    {
                        var item = deleteItems[0];
                        var exItem = exItems.Where(t => t.Id == item.Id).FirstOrDefault();

                        if (exItem != null)
                        {
                            db.Entry(exItem).State = EntityState.Deleted;
                            buffer++;
                        }

                        if (buffer == 100)
                        {
                            await db.SaveChangesAsync();
                            buffer = 0;
                        }
                    }

                    await db.SaveChangesAsync();
                }

                if (hasUpdate)
                {
                    var maxVersion = updateItems.Max(t => t.Version);

                    if (maxVersion > newDataVersion)
                        newDataVersion = (int)maxVersion;

                    var items = updateItems.Select(t => helper.Audit(new Classifier
                    {
                        Id = t.Id,
                        Code = t.Code,
                        Type = t.Type,
                        Value = t.Value,
                        ActiveFrom = t.ActiveFrom,
                        ActiveTo = t.ActiveTo,
                        Payload = t.Payload,
                        SortOrder = (int?)t.SortOrder,
                        PermissionType = string.IsNullOrEmpty(t.PermissionType) ? UserProfileType.Country : (UserProfileType)Enum.Parse(typeof(UserProfileType), t.PermissionType),
                        IsRequired = t.IsRequired
                    })).ToList();

                    int buffer = 0;

                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        var exItem = exItems.FirstOrDefault(t => t.Id == item.Id);

                        if (exItem == null)
                        {
                            helper.Audit(item);
                            db.Classifiers.Add(item);
                            buffer++;
                        }
                        else
                        {
                            // TODO updating a classifier may conflict with client updates
                            exItem.Code = item.Code;
                            exItem.Type = item.Type;
                            exItem.Value = item.Value;
                            exItem.ActiveFrom = item.ActiveFrom;
                            exItem.ActiveTo = item.ActiveTo;
                            exItem.PermissionType = item.PermissionType;
                            exItem.Payload = item.Payload;
                            exItem.SortOrder = item.SortOrder;
                            exItem.IsRequired = item.IsRequired;

                            helper.Audit(exItem);

                            db.Entry(exItem).State = EntityState.Modified;

                            buffer++;
                        }

                        if (buffer == 100)
                        {
                            await db.SaveChangesAsync();
                            buffer = 0;
                        }
                    }

                    await db.SaveChangesAsync();
                }
            }

            return newDataVersion;
        }

        private class ImportClassifier
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
            public DateTime? ActiveFrom { get; set; }
            public DateTime? ActiveTo { get; set; }
            public string Payload { get; set; }
            public decimal Version { get; set; }
            public decimal? SortOrder { get; set; }
            public string PermissionType { get; set; }
            public bool IsRequired => ClassifierTypes.RequiredStatuses.Contains(Type);
        }
    }
}
