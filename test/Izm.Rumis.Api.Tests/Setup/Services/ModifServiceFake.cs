using Izm.Rumis.Infrastructure.Modif;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal class ModifServiceFake : IModifService
    {
        public IEnumerable<SampleEntityModifDto> SampleEntities { get; set; } = new SampleEntityModifDto[] { };

        public Task<IEnumerable<ModifDto<SampleEntityModifDto>>> GetSampleEntityChangesAsync(int entityId)
        {
            var items = SampleEntities.Select(t => new ModifDto<SampleEntityModifDto>
            {
                Data = t
            });

            return Task.FromResult(items);
        }
    }
}
