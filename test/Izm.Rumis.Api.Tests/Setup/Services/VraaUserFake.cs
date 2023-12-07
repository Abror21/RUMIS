using Izm.Rumis.Infrastructure.Vraa;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal sealed class VraaUserFake : IVraaUser
    {
        public string FirstName { get; set; } = "FirstName";
        public string LastName { get; set; } = "LastName";
        public string PrivatePersonalIdentifier { get; set; } = "00000000000";
    }
}
