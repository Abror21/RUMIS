namespace Izm.Rumis.Tasks
{
    internal static class EnvironmentVariable
    {
        public static string DatabaseConnectionString => "DATABASE_CONNECTION_STRING";
        public static string MySqlVersion => "MYSQL_VERSION";
        public static string RedisConnectionString => "REDIS_CONNECTION_STRING";
        public static string SessionIdleTimeoutInMinutes => "SESSION_IDLE_TIMEOUT_IN_MINUTES";
        public static string S3AccessKey => "S3_ACCESS_KEY";
        public static string S3SecretKey => "S3_SECRET_KEY";
        public static string S3StorageUrl => "S3_STORAGE_URL";
        public static string S3UseHttp => "S3_USE_HTTP";
        public static string VISSServiceEndpointAddress => "VISS_SERVICE_ENDPOINT_ADDRESS";
        public static string VIISCredentialsUsername => "VIIS_CREDENTIALS_USERNAME";
        public static string VIISCredentialsPassword => "VIIS_CREDENTIALS_PASSWORD";
        public static string ViisStudentPersonCodeCacheDuration => "VIIS_STUDENT_PERSON_CODE_CACHE_DURATION";

    }
}
