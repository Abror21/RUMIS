using Izm.Rumis.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Izm.Rumis.Infrastructure.Modif
{
    public interface IModifService
    {
        //Task<IEnumerable<ModifDto<SampleEntityModifDto>>> GetSampleEntityChangesAsync(int studyDirectionId);
    }

    public class ModifService : IModifService
    {
        private readonly IModifDbContext modifDb;
        private readonly IAppDbContext appDb;

        public ModifService(IModifDbContext modifDb, IAppDbContext appDb)
        {
            this.modifDb = modifDb;
            this.appDb = appDb;
        }

        //public async Task<IEnumerable<ModifDto<SampleEntityModifDto>>> GetSampleEntityChangesAsync(int entityId)
        //{
        //    var query = from m in modifDb.SampleEntityModifs
        //                join c1 in appDb.Classifiers on m.SomeClassifierId equals c1.Id into c11
        //                from cls in c11.DefaultIfEmpty()
        //                join u in appDb.UserProfiles on m.ModifiedById equals u.Id into u1
        //                from usr in u1.DefaultIfEmpty()
        //                where m.Id == entityId
        //                orderby m._Date ascending
        //                select new ModifDto<SampleEntityModifDto>
        //                {
        //                    Action = m._Action,
        //                    Author = usr.FullName + " (" + usr.Name + ")",
        //                    Date = m._Date,
        //                    Id = m._Id,
        //                    Data = new SampleEntityModifDto
        //                    {
        //                        SomeClassifierId = m.SomeClassifierId,
        //                        SomeClassifier = m.SomeClassifierId == null ? null : new ClassifierValue
        //                        {
        //                            Value = cls.Value
        //                        }
        //                    }
        //                };

        //    var data = await query.Take(300).ToListAsync();

        //    var latest = GetLatest(data, (a, b) =>
        //    {
        //        return a.SomeClassifierId == b.SomeClassifierId;
        //    });

        //    return latest;
        //}

        private IEnumerable<ModifDto<T>> GetLatest<T>(IList<ModifDto<T>> data, Func<T, T, bool> equalityComparer)
        {
            var distinct = new List<ModifDto<T>>();

            for (int i = 0; i < data.Count; i++)
            {
                var row = data[i];

                if (i == 0)
                {
                    distinct.Add(row);
                }
                else
                {
                    var curr = row.Data;
                    var prev = data[i - 1].Data;

                    // add only if something has been changed
                    if (!equalityComparer(curr, prev))
                        distinct.Add(row);
                }
            }

            return distinct.OrderByDescending(t => t.Date).ToList();
        }
    }
}
