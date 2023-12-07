namespace Izm.Rumis.Api
{
    internal static class EnvironmentVariable
    {
        public static string AdminPassword => "ADMIN_PASSWORD";
        public static string AppUrl => "APP_URL";
        public static string AuthEncryptionPassword => "AUTH_ENCRYPTION_PASSWORD";
        public static string AuthEncryptionSalt => "AUTH_ENCRYPTION_SALT";
        public static string AuthExternalAdminUserName => "AUTH_EXTERNAL_ADMIN_USERNAME";
        public static string AuthExternalEnabled => "AUTH_EXTERNAL_ENABLED";
        public static string AuthExternalProvider => "AUTH_EXTERNAL_PROVIDER";
        public static string AuthExternalUrl => "AUTH_EXTERNAL_URL";
        public static string AuthFormsEnabled => "AUTH_FORMS_ENABLED";
        public static string AuthNotifyBeforeTimeoutInMinutes => "AUTH_NOTIFY_BEFORE_TIMEOUT_IN_MINUTES";
        public static string AuthTicketPassword => "AUTH_TICKET_PASSWORD";
        //public static string AuthTokenLifeTimeInMinutes => "AUTH_TOKEN_LIFETIME_IN_MINUTES";
        public static string AuthTokenSecurityKey => "AUTH_TOKEN_SECURITY_KEY";
        //public static string AuthUserProfileTokenLifeTimeInMinutes => "AUTH_USER_PROFILE_TOKEN_LIFETIME_IN_MINUTES";
        public static string AuthUserProfileTokenSecurityKey => "AUTH_USER_PROFILE_TOKEN_SECURITY_KEY";
        public static string DatabaseConnectionString => "DATABASE_CONNECTION_STRING";
        public static string EAddressServiceApiUrl => "EADDRESS_SERVICE_API_URL";
        public static string EAddressValidateServerCertificate => "EADDRESS_VALIDATE_SERVER_CERTIFICATE";
        public static string EServicePublicUrl => "ESERVICE_PUBLIC_URL";
        public static string ExternalAdminRole => "EXTERNAL_ADMIN_ROLE";
        public static string MinThreadCount => "MIN_THREAD_COUNT";
        public static string MySqlVersion => "MYSQL_VERSION";
        public static string NotificationsEnabled => "NOTIFICATIONS_ENABLED";
        public static string NotificationsEAddressEnabled => "NOTIFICATIONS_EADDRESS_ENABLED";
        public static string RedisConnectionString => "REDIS_CONNECTION_STRING";
        public static string SessionIdleTimeoutInMinutes => "SESSION_IDLE_TIMEOUT_IN_MINUTES";
        public static string StaticFileCacheDuration => "STATIC_FILE_CACHE_DURATION";
        public static string StaticFilePath => "STATIC_FILE_PATH";
        public static string S3AccessKey => "S3_ACCESS_KEY";
        public static string S3BucketName => "S3_BUCKET_NAME";
        public static string S3SecretKey => "S3_SECRET_KEY";
        public static string S3StorageUrl => "S3_STORAGE_URL";
        public static string S3UseHttp => "S3_USE_HTTP";
        public static string SmtpEnabledSsl => "SMTP_ENABLESSL";
        public static string SmtpFrom => "SMTP_FROM";
        public static string SmtpPassword => "SMTP_PASSWORD";
        public static string SmtpPort => "SMTP_PORT";
        public static string SmtpServer => "SMTP_SERVER";
        public static string SmtpUsername => "SMTP_USERNAME";
        public static string ViisServiceEndpointAddress => "VISS_SERVICE_ENDPOINT_ADDRESS";
        public static string ViisCredentialsUsername => "VIIS_CREDENTIALS_USERNAME";
        public static string ViisCredentialsPassword => "VIIS_CREDENTIALS_PASSWORD";
        public static string ViisStudentPersonCodeCacheDuration => "VIIS_STUDENT_PERSON_CODE_CACHE_DURATION";
        public static string VraaBaseUrl => "VRAA_BASE_URL";
        public static string VraaClientId => "VRAA_CLIENT_ID";
        public static string VraaClientSecret => "VRAA_CLIENT_SECRET";
    }
}
