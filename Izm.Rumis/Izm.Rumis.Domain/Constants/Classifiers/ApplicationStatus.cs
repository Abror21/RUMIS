using System.Collections.Generic;

namespace Izm.Rumis.Domain.Constants.Classifiers
{
    public static class ApplicationStatus
    {
        public const string Submitted = "submitted";
        public const string Postponed = "postponed";
        public const string Confirmed = "confirmed";
        public const string Declined = "declined";
        public const string Withdrawn = "withdrawn";
        public const string Deleted = "deleted";

        // Active statuses, excluding Confirmed; check PNA status in that case
        public static IEnumerable<string> ActiveStatuses => new string[]
        {
            Submitted,
            Postponed
        };
    }
}
