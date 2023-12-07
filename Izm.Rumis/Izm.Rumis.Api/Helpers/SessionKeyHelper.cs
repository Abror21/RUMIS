using System;

namespace Izm.Rumis.Api.Helpers
{
    internal static class SessionKeyHelper
    {
        public static string GetKillUserProfileSessionKey(Guid userProfileId) => $"kill_profile_token_{userProfileId}";
        public static string GetUserProfileTokenCreatedSessionKey(Guid userProfileId) => $"created_profile_token_{userProfileId}";
    }
}
