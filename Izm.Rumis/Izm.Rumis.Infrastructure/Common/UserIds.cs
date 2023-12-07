using System;
using System.Collections.Generic;

namespace Izm.Rumis.Infrastructure.Common
{
    public static class UserIds
    {
        private static HashSet<int> usedIds = new HashSet<int>();

        // Anonymous user should not be in the database at all!
        public static readonly Guid Anonymous = Guid.Empty;
        // System users should have only users without logins!
        public static readonly Guid Application = CreateSystemUserId(0);
        public static readonly Guid Tasks = CreateSystemUserId(1);
        // add other predefined users if needed...
        public static readonly Guid EServiceUser = CreateSystemUserId(2);

        // system user IDs must be in the following format: 00000000-1111-00XX-0000-000000000000
        private const string systemIdPrefix = "00000000-1111";

        public static bool IsSystemId(Guid id)
        {
            return id.ToString().StartsWith(systemIdPrefix);
        }

        private static Guid CreateSystemUserId(int number)
        {
            // ensure unique user ids (throws an exception if exists)
            usedIds.Add(number);

            var key = $"000{number}".Substring(0, 4);
            return new Guid($"{systemIdPrefix}-{key}-0000-000000000000");
        }
    }
}
