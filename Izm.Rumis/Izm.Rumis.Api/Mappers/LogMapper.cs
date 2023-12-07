using Izm.Rumis.Api.Models;
using Izm.Rumis.Infrastructure.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    internal static class LogMapper
    {
        public static Expression<Func<Log, LogListItemResponse>> ProjectListItem()
        {
            return log => new LogListItemResponse
            {
                Id = log.Id,
                Date = log.Date,
                EducationalInstitution = log.EducationalInstitutionId == null ? null : new LogListItemResponse.EducationalInstitutionData
                {
                    Id = log.EducationalInstitution.Id,
                    Code = log.EducationalInstitution.Code,
                    Name = log.EducationalInstitution.Name
                },
                Exception = log.Exception,
                IpAddress = log.IpAddress,
                Level = log.Level,
                Logger = log.Logger,
                Message = log.Message,
                PersonTechnical = log.PersonId == null ? null : new LogListItemResponse.PersonTechnicalData
                {
                    Id = log.Person.Id,
                    Persons = log.Person.Persons.Select(person => new LogListItemResponse.PersonTechnicalData.PersonData
                    {
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        PrivatePersonalIdentifier = person.PrivatePersonalIdentifier
                    })
                },
                RequestMethod = log.RequestMethod,
                RequestUrl = log.RequestUrl,
                SessionId = log.SessionId,
                Supervisor = log.SupervisorId == null ? null : new LogListItemResponse.SupervisorData
                {
                    Id = log.Supervisor.Id,
                    Code = log.Supervisor.Code,
                    Name = log.Supervisor.Name
                },
                Thread = log.Thread,
                TraceId = log.TraceId,
                UserAgent = log.UserAgent,
                UserId = log.UserId == null ? null : log.UserId.Value,
                UserName = log.UserName,
                UserProfile = log.UserProfileId == null ? null : new LogListItemResponse.UserProfileData
                {
                    Id = log.UserProfile.Id,
                    Roles = log.UserProfile.Roles.Select(t => t.Name)
                }
            };
        }
    }
}
