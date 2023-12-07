using log4net.Appender;
using log4net.Core;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Izm.Rumis.Logging
{
    public class SqlServerLog4netAppender : AppenderSkeleton
    {
        private const string schema = "dbo";
        private const string tableName = "Log";
        private const string colId = "Id";
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

        private static string connectionString;
        private static bool enabled => connectionString != null;

        public static void SetDatabase(string connectionString)
        {
            if (!enabled)
                SqlServerLog4netAppender.connectionString = connectionString;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (!enabled) return;
            WriteEvent(loggingEvent);
        }

        private void WriteEvent(LoggingEvent loggingEvent)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var query = @$"
insert into {schema}.""{tableName}"" 
(""{colDate}"",""{colThread}"",""{colLevel}"",""{colLogger}"",""{colTraceId}"",""{colMessage}"",""{colException}"",""{colUserName}"",
""{colIpAddress}"",""{colUserAgent}"",""{colRequestUrl}"",""{colRequestMethod}"") 
values (@date,@thread,@level,@logger,@traceid,@message,@exception,@username,@ipaddress,@useragent,@url,@method)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    AddParameter(cmd, "date", DbType.DateTime, loggingEvent.TimeStamp);
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

                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        private void AddParameter(SqlCommand command, string name, DbType type, object value, int? size = null)
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
