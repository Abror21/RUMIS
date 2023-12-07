using Izm.Rumis.Api.Services;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal class PasswordManagerFake : IPasswordManager
    {
        public Task ChangePassword(string username, string currentPassword, string newPassword)
        {
            return Task.CompletedTask;
        }

        public Task RecoverPassword(string username, bool force)
        {
            return Task.CompletedTask;
        }

        public Task ResetPassword(string secret, string password)
        {
            return Task.CompletedTask;
        }
    }
}
