using log4net.Appender;
using log4net.Core;
using MySqlConnector;
using System;
using System.Data;

namespace Izm.Rumis.Logging
{
    public class MySqlLog4netAppender : AppenderSkeleton
    {
        private const string tableName = "Log";
        private const string colDate = "Date";
        private const string colThread = "Thread";
        private const string colLevel = "Level";
        private const string colLogger = "Logger";
        private const string colTraceId = "TraceId";
        private const string colMessage = "Message";
        private const string colException = "Exception";
        private const string colUserName = "UserName";
        private const string colIpAddress = "IpAddress";
        private const string colUserAgent = "UserAgent";
        private const string colRequestUrl = "RequestUrl";
        private const string colRequestMethod = "RequestMethod";
        private const string colUserId = "UserId";
        private const string colUserProfileId = "UserProfileId";
        private const string colPersonId = "PersonId";
        private const string colSessionId = "SessionId";
        private const string colEducationalInstitutionId = "EducationalInstitutionId";
        private const string colSupervisorId = "SupervisorId";

        private static string connectionString;
        private static bool enabled => connectionString != null;

        public static void SetDatabase(string connectionString)
        {
            if (!enabled)
                MySqlLog4netAppender.connectionString = connectionString;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (!enabled) return;
            WriteEvent(loggingEvent);
        }

        private void WriteEvent(LoggingEvent loggingEvent)
        {
            using var conn = new MySqlConnection(connectionString);

            conn.Open();

            var query = @$"
                    insert into {tableName}
                    ({colDate},{colThread},{colLevel},{colLogger},{colTraceId},{colMessage},{colException},{colUserName},
                    {colIpAddress},{colUserAgent},{colRequestUrl},{colRequestMethod},{colUserId},{colUserProfileId},{colPersonId},{colSessionId},
                    {colEducationalInstitutionId},{colSupervisorId}) 
                    values (?date,?thread,?level,?logger,?traceid,?message,?exception,?username,?ipaddress,?useragent,?url,?method,?userid,?userprofileid,?personid,?sessionid,?educationalinstitutionid,?supervisorid);";

            using (var cmd = new MySqlCommand(query, conn))
            {
                AddParameter(cmd, "date", DbType.DateTime, loggingEvent.TimeStampUtc);
                AddParameter(cmd, "thread", DbType.String, loggingEvent.ThreadName, 255);
                AddParameter(cmd, "level", DbType.String, loggingEvent.Level.DisplayName, 50);
                AddParameter(cmd, "username", DbType.String, loggingEvent.LookupProperty("username")?.ToString(), 255);
                AddParameter(cmd, "ipaddress", DbType.String, loggingEvent.LookupProperty("ip")?.ToString(), 100);
                AddParameter(cmd, "useragent", DbType.String, loggingEvent.LookupProperty("useragent")?.ToString(), 150);
                AddParameter(cmd, "url", DbType.String, loggingEvent.LookupProperty("path")?.ToString(), 4000);
                AddParameter(cmd, "method", DbType.String, loggingEvent.LookupProperty("method")?.ToString(), 50);
                AddParameter(cmd, "logger", DbType.String, loggingEvent.LoggerName, 255);
                AddParameter(cmd, "traceid", DbType.String, loggingEvent.LookupProperty("traceid")?.ToString(), 50);
                AddParameter(cmd, "message", DbType.String, loggingEvent.RenderedMessage, 4000);
                AddParameter(cmd, "exception", DbType.String, loggingEvent.GetExceptionString(), 4000);
                AddParameter(cmd, "userid", DbType.String, loggingEvent.LookupProperty("userid")?.ToString(), 32);
                AddParameter(cmd, "userprofileid", DbType.String, loggingEvent.LookupProperty("userprofileid")?.ToString(), 32);
                AddParameter(cmd, "personid", DbType.String, loggingEvent.LookupProperty("personid")?.ToString(), 32);
                AddParameter(cmd, "sessionid", DbType.String, loggingEvent.LookupProperty("sessionid")?.ToString(), 32);
                AddParameter(cmd, "educationalinstitutionid", DbType.String, loggingEvent.LookupProperty("educationalinstitutionid")?.ToString(), 32);
                AddParameter(cmd, "supervisorid", DbType.Int32, (int?)loggingEvent.LookupProperty("supervisorid"));

                cmd.ExecuteNonQuery();
            }

            conn.Close();
        }

        private void AddParameter(MySqlCommand command, string name, DbType type, object value, int? size = null)
        {
            var para = command.CreateParameter();

            para.Direction = ParameterDirection.Input;
            para.DbType = type;
            para.ParameterName = name;

            para.Value = value ?? DBNull.Value;

            if (size != null)
                para.Size = size.Value;

            command.Parameters.Add(para);
        }
    }
}
