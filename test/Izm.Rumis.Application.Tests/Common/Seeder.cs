using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure;
using System;

namespace Izm.Rumis.Application.Tests.Common
{
    internal class Seeder
    {
        private readonly AppDbContext db;

        public Seeder(AppDbContext db)
        {
            this.db = db;
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public Classifier AddClassifier(string type, Guid id, string code = null, string payload = null)
        {
            var item = new Classifier
            {
                Id = id,
                Code = code,
                Type = type,
                Value = string.Empty,
                Payload = payload
            };

            db.Classifiers.Add(item);

            return item;
        }
    }
}
