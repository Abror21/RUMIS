using Izm.Rumis.Api.Services;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal class EncryptionServiceFake : IEncryptionService
    {
        public string DecryptResult { get; set; } = string.Empty;
        public string EncryptResult { get; set; } = string.Empty;

        public string Decrypt(string data)
        {
            return DecryptResult;
        }

        public string Encrypt(string data)
        {
            return EncryptResult;
        }
    }
}
