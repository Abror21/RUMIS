using System.Collections.Generic;

namespace Izm.Rumis.Domain.Constants.Classifiers
{
    public static class ResourceStatus
    {
        public const string Available = "available";
        public const string Damaged = "damaged";
        public const string InUse = "in_use";
        public const string Lost = "lost";
        public const string Maintenance = "maintenance";
        public const string New = "new";
        public const string Reserved = "reserved";
        public const string Stolen = "stolen";
        public const string UnderRepair = "under_repair";
        public const string InReserve = "in_reserve";

        public static IEnumerable<string> ActiveStatuses => new string[]
        {
            New,
            InReserve,
            Available,
            Reserved,
            Maintenance,
            InUse
        };
    }
}
