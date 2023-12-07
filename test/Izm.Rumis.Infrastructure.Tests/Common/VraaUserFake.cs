using Izm.Rumis.Infrastructure.Vraa;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal sealed class VraaUserFake : IVraaUser
    {
        public string FirstName { get; set; } = "FirstName";
        public string LastName { get; set; } = "LastName";
        public string PrivatePersonalIdentifier { get; set; } = "00000000000";
    }
}
