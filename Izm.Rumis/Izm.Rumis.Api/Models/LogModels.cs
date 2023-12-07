using Izm.Rumis.Api.Common;
using Izm.Rumis.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Models
{
    public class LogCreateRequest
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; }
    }

    public class LogListItemResponse
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Thread { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string TraceId { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string RequestUrl { get; set; }
        public string RequestMethod { get; set; }
        public Guid? UserId { get; set; }
        public UserProfileData UserProfile { get; set; }
        public PersonTechnicalData PersonTechnical { get; set; }
        public string SessionId { get; set; }
        public EducationalInstitutionData EducationalInstitution { get; set; }
        public SupervisorData Supervisor { get; set; }

        public class EducationalInstitutionData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class SupervisorData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class PersonTechnicalData
        {
            public Guid Id { get; set; }
            public IEnumerable<PersonData> Persons { get; set; }

            public class PersonData
            {
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string PrivatePersonalIdentifier { get; set; }
            }
        }

        public class UserProfileData
        {
            public Guid Id { get; set; }
            public IEnumerable<string> Roles { get; set; }
        }
    }

    public class LogFilter : Filter<Log>
    {
        private const char splitSymbol = ',';

        public string Thread { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string TraceId { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string RequestUrl { get; set; }
        public string RequestMethods { get; set; }
        public string EducationalInstitutionCode { get; set; }
        public string EducationalInstitutionName { get; set; }
        public string SupervisorCode { get; set; }
        public string SupervisorName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrivatePersonalIdentifier { get; set; }
        public string Roles { get; set; }
        public string Levels { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan TimeFrom { get; set; }
        public TimeSpan TimeTo { get; set; }

        protected override Expression<Func<Log, bool>>[] GetFilters()
        {
            var result = new List<Expression<Func<Log, bool>>>();

            if (!string.IsNullOrEmpty(Thread))
                result.Add(t => t.Thread == Thread);

            if (!string.IsNullOrEmpty(Level))
                result.Add(t => t.Level.Contains(Level));

            if (!string.IsNullOrEmpty(Logger))
                result.Add(t => t.Logger.Contains(Logger));

            if (!string.IsNullOrEmpty(TraceId))
                result.Add(t => t.TraceId.Contains(TraceId));

            if (!string.IsNullOrEmpty(Message))
                result.Add(t => t.Message.Contains(Message));

            if (!string.IsNullOrEmpty(Exception))
                result.Add(t => t.Exception.Contains(Exception));

            if (!string.IsNullOrEmpty(UserName))
                result.Add(t => t.UserName.Contains(UserName));

            if (!string.IsNullOrEmpty(IpAddress))
                result.Add(t => t.IpAddress.Contains(IpAddress));

            if (!string.IsNullOrEmpty(UserAgent))
                result.Add(t => t.UserAgent.Contains(UserAgent));

            if (!string.IsNullOrEmpty(RequestUrl))
                result.Add(t => t.RequestUrl.Contains(RequestUrl));

            if (!string.IsNullOrEmpty(EducationalInstitutionCode))
                result.Add(t => t.EducationalInstitution.Code.Contains(EducationalInstitutionCode));

            if (!string.IsNullOrEmpty(EducationalInstitutionName))
                result.Add(t => t.EducationalInstitution.Name.Contains(EducationalInstitutionName));

            if (!string.IsNullOrEmpty(SupervisorCode))
                result.Add(t => t.Supervisor.Code.Contains(SupervisorCode));

            if (!string.IsNullOrEmpty(SupervisorName))
                result.Add(t => t.Supervisor.Name.Contains(SupervisorName));

            if (!string.IsNullOrEmpty(FirstName))
                result.Add(t => t.Person.Persons.Any(person => person.FirstName.Contains(SupervisorName)));

            if (!string.IsNullOrEmpty(LastName))
                result.Add(t => t.Person.Persons.Any(person => person.LastName.Contains(LastName)));

            if (!string.IsNullOrEmpty(PrivatePersonalIdentifier))
                result.Add(t => t.Person.Persons.Any(person => person.PrivatePersonalIdentifier.Contains(PrivatePersonalIdentifier)));

            var roles = (Roles ?? string.Empty).Split(splitSymbol)
               .Where(t => !string.IsNullOrEmpty(t));

            if (roles.Any())
                result.Add(t => t.UserProfile.Roles.Any(role => roles.Contains(role.Name)));

            var levels = (Levels ?? string.Empty).Split(splitSymbol)
                .Where(t => !string.IsNullOrEmpty(t));

            if (levels.Any())
                result.Add(t => levels.Contains(t.Level));

            var requestMethods = (RequestMethods ?? string.Empty)
                .Split(splitSymbol)
                .Where(t => !string.IsNullOrEmpty(t));

            if (requestMethods.Any())
                result.Add(t => requestMethods.Contains(t.RequestMethod));

            var dateFrom = Date.Date.Add(TimeFrom);
            var dateTo = Date.Date.Add(TimeTo);

            result.Add(t => t.Date >= dateFrom && t.Date <= dateTo);

            return result.ToArray();
        }
    }
}
