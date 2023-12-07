using System.Collections.Generic;

namespace Izm.Rumis.Domain.Constants.Classifiers
{
    public static class PnaStatus
    {
        public const string Preparing = "preparing";
        public const string Prepared = "prepared";
        public const string Issued = "issued";
        public const string Returned = "returned";
        public const string Stolen = "stolen";
        public const string Lost = "lost";
        public const string Cancelled = "cancelled";

        public static IEnumerable<string> ActiveStatuses => new string[]
        {
            Preparing,
            Prepared,
            Issued
        };

        public static IEnumerable<string> NonActiveStatuses => new string[]
        {
            Returned,
            Stolen,
            Lost,
            Cancelled
        };
    }
}
