using System.Collections.Generic;

namespace Izm.Rumis.Domain.Constants.Classifiers
{
    public static class SocialStatus
    {
        public const string PoorFamily = "poor_family";
        public const string LowIncomeFamily = "low_income_family";
        public const string Disability = "disability";
        public const string MedicalRecommendations = "medical_recommendations";

        public static IEnumerable<KeyValuePair<string, string>> ViisStatusPairs => new[]
       {
             new KeyValuePair<string, string> (MedicalRecommendations, "P"),
             new KeyValuePair<string, string> (Disability, "I"),
             new KeyValuePair<string, string> (LowIncomeFamily, "M"),
             new KeyValuePair<string, string> (PoorFamily, "T")
       };
    }
}
