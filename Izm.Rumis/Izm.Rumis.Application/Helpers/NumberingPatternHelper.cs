namespace Izm.Rumis.Application.Helpers
{
    public static class NumberingPatternHelper
    {
        public static string ApplicationKeyFormat(string institutionCode = DefaultInstitution)
        {
            return $"PIE-{institutionCode}";
        }
        public static string ResourceKeyFormat(string institutionCode = DefaultInstitution)
        {
            return $"RES-{institutionCode}";
        }
        public static string ApplicationResourcesKeyFormat(string institutionCode = DefaultInstitution)
        {
            return $"PNA-{institutionCode}";
        }
        public static string ApplicationNumberFormat(string institutionCode = DefaultInstitution, long serialNumber = 1)
        {
            return $"PIE-{institutionCode}-{serialNumber}";
        }

        public static string ResourceNumberFormat(string institutionCode = DefaultInstitution, long serialNumber = 1)
        {
            return $"RES-{institutionCode}-{serialNumber}";
        }

        public static string ApplicationResourcesNumberFormat(string institutionCode = DefaultInstitution, long serialNumber = 1)
        {
            return $"PNA-{institutionCode}-{serialNumber}";
        }

        public const string DefaultInstitution = "defaultInstitution";
    }
}
