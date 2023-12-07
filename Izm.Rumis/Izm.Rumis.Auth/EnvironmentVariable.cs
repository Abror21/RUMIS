namespace Izm.Rumis.Auth
{
    public static class EnvironmentVariable
    {
        public static string AdfsBaseUrl => "ADFS_BASE_URL";
        public static string AdfsEnabled => "ADFS_ENABLED";
        public static string AdfsMetadataAddress => "ADFS_METADATA_ADDRESS";
        public static string AdfsWtrealm => "ADFS_WTREALM";
        public static string ErrorRedirectUrl => "ERROR_REDIRECT_URL";
        public static string SignOutRedirectUrl => "SIGN_OUT_REDIRECT_URL";
        public static string TicketPassword => "TICKET_PASSWORD";
        public static string TicketReplyUrl => "TICKET_REPLY_URL";
        public static string WindowsEnabled => "WINDOWS_ENABLED";
    }
}
