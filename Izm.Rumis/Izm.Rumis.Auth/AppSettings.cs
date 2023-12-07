namespace Izm.Rumis.Auth
{
    public class AppSettings
    {
        public string TicketPassword { get; set; }
        public string TicketReplyUrl { get; set; }
        public string SignOutRedirectUrl { get; set; }
        public string ErrorRedirectUrl { get; set; }

        public bool AdfsEnabled { get; set; }
        public bool WindowsEnabled { get; set; }
    }
}
