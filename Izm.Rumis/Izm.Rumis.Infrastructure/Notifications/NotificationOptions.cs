namespace Izm.Rumis.Infrastructure.Notifications
{
    public class NotificationOptions
    {
        public bool EAddressEnabled { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public string EServicePublicUrl { get; set; }
        public bool Ignore { get; set; } = false;
    }
}
