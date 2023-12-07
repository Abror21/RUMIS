using System;

namespace Izm.Rumis.Api.Options
{
    public class AuthSettings
    {
        public string AppUrl { get; set; }
        public string ExternalUrl { get; set; }
        public string ExternalProvider { get; set; }
        public string ExternalAdminRole { get; set; }
        public string AdminUserName { get; set; }
        public string TicketPassword { get; set; }
        //public TimeSpan TokenLifeTime { get; set; } = TimeSpan.FromMinutes(10);
        public string TokenSecurityKey { get; set; }

        public bool FormsEnabled { get; set; }
        public bool ExternalEnabled { get; set; }
        public TimeSpan SessionIdleTimeout { get; set; } = TimeSpan.FromMinutes(15);
        public TimeSpan NotifyBeforeSessionTimeout { get; set; } = TimeSpan.FromMinutes(2);
    }
}
