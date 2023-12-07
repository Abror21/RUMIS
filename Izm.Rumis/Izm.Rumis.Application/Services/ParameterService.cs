using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public class ParameterService : IParameterService
    {
        private readonly IAppDbContext db;

        public ParameterService(IAppDbContext db)
        {
            this.db = db;
        }

        /// <inheritdoc/>
        public SetQuery<Parameter> Get()
        {
            return new SetQuery<Parameter>(db.Parameters.AsNoTracking());
        }

        /// <inheritdoc/>
        public string GetValue(string code)
        {
            return Get().Where(t => t.Code == code).First(t => t.Value);
        }

        /// <inheritdoc/>
        public string FindValue(string code, IEnumerable<Parameter> parameters)
        {
            return parameters.Where(t => t.Code == code).FirstOrDefault()?.Value ?? null;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(int id, string value, CancellationToken cancellationToken = default)
        {
            var entity = await db.Parameters.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            entity.Value = value;

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
