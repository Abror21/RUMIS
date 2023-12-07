using Izm.Rumis.Application;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Helpers;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Izm.Rumis.Infrastructure.Enums;
using Izm.Rumis.Infrastructure.Viis.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VIIS;

namespace Izm.Rumis.Infrastructure.Viis
{
    public class ViisServiceOptions
    {
        public int StudentPersonCodeCacheDuration { get; set; }
    }
    public interface IViisService
    {
        Task CheckPersonApplicationsAsync(CancellationToken cancellationToken = default);
        Task SyncEducationalInstitutionsAsync(CancellationToken cancellationToken = default);
        Task<List<RelatedPersonData.Student>> GetStudentsAsync(RequestParamType type, string privatePersonalIdentifier, CancellationToken cancellationToken = default);
        Task<List<EmployeeData.Employee>> GetEmployeesAsync(string privatePersonalIdentifier, CancellationToken cancellationToken = default);
        Task<List<SocialStatusData.Student>> CheckSocialStatusAsync(string privatePersonalIdentifier, string type, CancellationToken cancellationToken = default);
    }

    public class ViisService : IViisService, IApplicationSocialStatusCheckService, IPersonDataService
    {
        private readonly ViisServiceOptions options;
        private readonly IAppDbContext db;
        private readonly IHostEnvironment environment;
        private readonly IDistributedCache distributedCache;
        private readonly IUniversalDataSetService universalDataSetService;
        private readonly IGdprAuditService gdprAuditService;
        private readonly ILogger<ViisService> logger;

        public ViisService(
            IOptions<ViisServiceOptions> options,
            IAppDbContext db,
            IHostEnvironment environment,
            IDistributedCache distributedCache,
            IUniversalDataSetService universalDataSetService,
            IGdprAuditService gdprAuditService,
            ILogger<ViisService> logger)
        {
            this.options = options.Value;
            this.db = db;
            this.environment = environment;
            this.distributedCache = distributedCache;
            this.universalDataSetService = universalDataSetService;
            this.gdprAuditService = gdprAuditService;
            this.logger = logger;
        }

        public async Task CheckPersonApplicationsAsync(CancellationToken cancellationToken = default)
        {
            var validStatuses = new string[] { ApplicationStatus.Postponed, ApplicationStatus.Submitted };

            var personsApplications = await db.Applications
                .Where(t => validStatuses.Contains(t.ApplicationStatus.Code) || t.ApplicationResources.Any(t => PnaStatus.ActiveStatuses.Contains(t.PNAStatus.Code)))
                .Include(t => t.ResourceTargetPerson)
                    .ThenInclude(t => t.Persons)
                .Include(t => t.ResourceTargetPersonType)
                .Include(t => t.EducationalInstitution)
                .GroupBy(t => t.ResourceTargetPerson).Select(t => new
                {
                    Person = t.Key,
                    Applications = t.ToArray(),
                })
                .ToArrayAsync(cancellationToken);

            var classifierIds = await db.Classifiers
                .Where(t => (t.Type == ClassifierTypes.ResourceTargetPersonEducationalStatus && t.Code == ResourceTargetPersonEducationalStatus.NotStudying)
                            || (t.Type == ClassifierTypes.ResourceTargetPersonWorkStatus && t.Code == ResourceTargetPersonWorkStatus.NotWorking)
                            || (t.Type == ClassifierTypes.ApplicationStatus && t.Code == ApplicationStatus.Declined))
                .Select(t => new { t.Type, t.Id })
                .ToDictionaryAsync(t => t.Type, t => t.Id, cancellationToken);

            var declineReasons = await db.TextTemplates
                .Where(t => t.Code == TextTemplateCode.ApplicationAsEmployeeDeclinedReasonByMonitoring
                            || t.Code == TextTemplateCode.ApplicationAsLearnerDeclinedReasonByMonitoring)
                .Select(t => new { t.Code, t.Content })
                .ToDictionaryAsync(t => t.Code, t => t.Content, cancellationToken);

            foreach (var personApplications in personsApplications)
            {
                var viisPersons = await GetStudentsAsync(
                    type: RequestParamType.Student,
                    privatePersonalIdentifier: personApplications.Person.GetPerson().PrivatePersonalIdentifier,
                    cancellationToken: cancellationToken);

                var viisPerson = viisPersons.FirstOrDefault();

                if (viisPerson == null)
                {
                    logger.LogError("Person {person} has no data in VIIS.", personApplications.Person);
                    continue;
                }

                var institution = viisPerson.Institution.First();

                var educationalSubStatusId = await db.Classifiers
                    .Where(t => t.Type == ClassifierTypes.ResourceTargetPersonEducationalSubStatus && t.Value == viisPerson.SubStatus)
                    .Select(t => (Guid?)t.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                foreach (var application in personApplications.Applications)
                {
                    // Check institution
                    if (!viisPerson.Institution.Any(t => t.RegNr == application.EducationalInstitution.Code))
                    {
                        var declineReason = string.Empty;

                        switch (application.ResourceTargetPersonType.Code)
                        {
                            case ResourceTargetPersonType.Learner:
                                declineReason = declineReasons[TextTemplateCode.ApplicationAsLearnerDeclinedReasonByMonitoring];
                                application.SetMonitoringEducationalStatus(classifierIds[ClassifierTypes.ResourceTargetPersonEducationalStatus]);
                                break;

                            case ResourceTargetPersonType.Employee:
                                declineReason = declineReasons[TextTemplateCode.ApplicationAsEmployeeDeclinedReasonByMonitoring];
                                application.SetMonitoringWorkStatus(classifierIds[ClassifierTypes.ResourceTargetPersonWorkStatus]);
                                break;

                            default:
                                break;
                        }

                        if (validStatuses.Contains(application.ApplicationStatus.Code))
                        {
                            var propertyMap = ApplicationTextTemplateHelper.CreatePropertyMap(application);

                            application.DeclineReason = TextTemplateParser.Parse(declineReason, propertyMap);
                            application.SetApplicationStatus(classifierIds[ClassifierTypes.ApplicationStatus]);
                        }
                    }

                    if (application.ResourceTargetPersonType.Code == ResourceTargetPersonType.Learner)
                    {
                        // Check educational substatus
                        if (application.ResourceTargetPersonEducationalSubStatusId != educationalSubStatusId)
                            application.MonitoringEducationalSubStatusId = educationalSubStatusId;

                        // Check class
                        if (int.TryParse(institution.Class.ClassGrade, out var classGrade)
                                && application.ResourceTargetPersonClassGrade != classGrade)
                            application.MonitoringClassGrade = classGrade;

                        if ((application.ResourceTargetPersonClassParallel ?? string.Empty) != (institution.Class.Parallel ?? string.Empty))
                            application.MonitoringClassParallel = institution.Class.Parallel;

                        if ((application.ResourceTargetPersonGroup ?? string.Empty) != (institution.Class.GroupName ?? string.Empty))
                            application.MonitoringGroup = institution.Class.GroupName;
                    }
                }

                await db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task SyncEducationalInstitutionsAsync(CancellationToken cancellationToken = default)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var queryParams = new QueryParamBase[]
            {
                new QueryParamBase
                {
                    Name = "@varchar_MunicipalityATVKCode"
                }
            };

            var response = await universalDataSetService.ExecuteAsync("RUMIS_Izglitibas_iestazu_dati", queryParams);

            var result = new EducationalInstitutionData();

            var reader = new StringReader(response.OuterXml);

            var serializer = new XmlSerializer(typeof(EducationalInstitutionData));

            var institutions = (EducationalInstitutionData)serializer.Deserialize(reader);

            var distinctInstitutions = institutions.GroupBy(x => x.RegNr)
                .Select(group => group.First())
                .ToArray();

            if (distinctInstitutions.Count() == 0)
                return;

            var supervisors = await db.Supervisors.ToArrayAsync(cancellationToken);

            var data = await db.EducationalInstitutions.ToArrayAsync(cancellationToken);

            var statusId = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.EducationalInstitutionStatus && t.Code == EducationalInstitutionStatus.Disabled)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var update = false;

            var supervisorsInstitutions = distinctInstitutions
                .GroupBy(t => t.Founder.UrCode)
                .Select(t => new
                {
                    t.First().Founder,
                    t.First().Supervisor,
                    Institutions = t.ToArray(),
                })
                .ToArray();

            foreach (var supervisorInstitutions in supervisorsInstitutions)
            {
                if (string.IsNullOrEmpty(supervisorInstitutions.Founder.UrCode) || string.IsNullOrEmpty(supervisorInstitutions.Founder.Name))
                    continue;

                var supervisor = supervisors.FirstOrDefault(t => t.Code == supervisorInstitutions.Founder.UrCode);

                if (supervisor == null)
                {
                    supervisor = new Supervisor
                    {
                        Code = supervisorInstitutions.Founder.UrCode,
                        Name = supervisorInstitutions.Founder.Name.Trim(),
                        Type = supervisorInstitutions.Supervisor.Name.Trim()
                    };

                    db.Supervisors.Add(supervisor);

                    update = true;
                }
                else
                {
                    supervisor.Name = UpdateValue(supervisor.Name, supervisorInstitutions.Founder.Name);
                    supervisor.Type = UpdateValue(supervisor.Type, supervisorInstitutions.Supervisor.Name);
                }

                foreach (var institution in supervisorInstitutions.Institutions)
                {
                    if (string.IsNullOrEmpty(institution.RegNr) || string.IsNullOrEmpty(institution.Name))
                        continue;

                    var existingInstitution = data.FirstOrDefault(x => x.Code == institution.RegNr);

                    if (existingInstitution == null)
                    {
                        var educationalInstitution = new EducationalInstitution
                        {
                            Name = institution.Name,
                            Code = institution.RegNr,
                            Email = institution.Email,
                            PhoneNumber = institution.Phone,
                            Address = institution.Address.Adresse,
                            City = institution.Address.City,
                            District = institution.Address.RuralTerritory,
                            Municipality = institution.Address.Municipality,
                            Supervisor = supervisor,
                            StatusId = statusId
                        };

                        db.EducationalInstitutions.Add(educationalInstitution);

                        update = true;
                    }
                    else
                    {
                        existingInstitution.Name = UpdateValue(existingInstitution.Name, institution.Name);
                        existingInstitution.Email = UpdateValue(existingInstitution.Email, institution.Email);
                        existingInstitution.PhoneNumber = UpdateValue(existingInstitution.PhoneNumber, institution.Phone);
                        existingInstitution.Address = UpdateValue(existingInstitution.Address, institution.Address.Adresse);
                        existingInstitution.City = UpdateValue(existingInstitution.City, institution.Address.City);
                        existingInstitution.District = UpdateValue(existingInstitution.District, institution.Address.RuralTerritory);
                        existingInstitution.Municipality = UpdateValue(existingInstitution.Municipality, institution.Address.Municipality);
                        existingInstitution.Supervisor = UpdateValue(existingInstitution.Supervisor, supervisor);
                    }
                }
            }

            if (update)
                await db.SaveChangesAsync(cancellationToken);

            T UpdateValue<T>(T setValue, T newValue)
            {
                if (typeof(T) == typeof(string) && newValue is string)
                    newValue = (T)(object)(string.IsNullOrWhiteSpace((string)(object)newValue) ? null : ((string)(object)newValue).Trim());

                if (!EqualityComparer<T>.Default.Equals(setValue, newValue))
                {
                    setValue = newValue;
                    update = true;
                }

                return setValue;
            }
        }

        public async Task<List<RelatedPersonData.Student>> GetStudentsAsync(RequestParamType type, string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string paramType = "";

            switch (type)
            {
                case RequestParamType.Parent:
                    paramType = "@varchar_ParentPersonCode";
                    break;
                case RequestParamType.Student:
                    paramType = "@varchar_StudentPersonCode";
                    break;
                default:
                    break;
            }

            var queryParams = new QueryParamString[]
            {
                new QueryParamString
                {
                    Name = paramType,
                    Value = privatePersonalIdentifier
                }
            };

            var responseTask = universalDataSetService.ExecuteAsync("RUMIS_dati", queryParams);

            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForGetStudentsOperation(privatePersonalIdentifier), cancellationToken);

            var response = await responseTask;

            var data = new RelatedPersonData();

            var reader = new StringReader(response.OuterXml);

            var serializer = new XmlSerializer(typeof(RelatedPersonData));

            var students = (RelatedPersonData)serializer.Deserialize(reader);

            if (students.Count == 0 && !environment.IsProduction())
            {
                var xmlName = string.Empty;

                xmlName = type == 0
                    ? $"asParentOrGuardian_{privatePersonalIdentifier}.xml"
                    : $"asEducatee_{privatePersonalIdentifier}.xml";

                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = $"{assembly.GetName().Name}.Viis.Data.{xmlName}";

                using var stream = assembly.GetManifestResourceStream(resourceName);

                if (stream == null)
                {
                    logger.LogInformation("Fake VIIS students data not found.");

                    return students;
                }

                using var streamReader = new StreamReader(stream);

                students = (RelatedPersonData)serializer.Deserialize(streamReader);
            }

            await gdprAuditService.TraceRangeAsync(students.Select(GdprAuditHelper.ProjectStudentTraces(privatePersonalIdentifier)).ToArray(), cancellationToken);

            return students;
        }


        public async Task<bool> CheckEducationalInstitutionAsEmployee(string privatePersonalIdentifier, string educationalInstitutionCode, CancellationToken cancellationToken = default)
        {
            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForCheckEducationalInstitutionAsEmployee(privatePersonalIdentifier, educationalInstitutionCode), cancellationToken);

            var employeeData = await GetEmployeesAsync(privatePersonalIdentifier, cancellationToken);

            await gdprAuditService.TraceRangeAsync(employeeData.Select(GdprAuditHelper.ProjectInstitutionAsEmployeeTraces(privatePersonalIdentifier, educationalInstitutionCode)).ToArray(), cancellationToken);

            return employeeData.Any(e => e.Institution.Any(i => i.RegNr == educationalInstitutionCode));
        }


        public async Task<bool> CheckEducationalInstitutionAsStudent(string privatePersonalIdentifier, string educationalInstitutionCode, CancellationToken cancellationToken = default)
        {
            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForCheckEducationalInstitutionAsStudent(privatePersonalIdentifier, educationalInstitutionCode), cancellationToken);

            var studentData = await GetStudentsAsync(RequestParamType.Student, privatePersonalIdentifier, cancellationToken);

            await gdprAuditService.TraceRangeAsync(studentData.Select(GdprAuditHelper.ProjectInstitutionAsStudentTraces(privatePersonalIdentifier, educationalInstitutionCode)).ToArray(), cancellationToken);

            return studentData.Any(e => e.Institution.Any(i => i.RegNr == educationalInstitutionCode));
        }

        /// <inheritdoc/>
        public async Task<List<EmployeeData.Employee>> GetEmployeesAsync(string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var queryParams = new QueryParamString[]
            {
                new QueryParamString
                {
                    Name = "@varchar_EmployeePersonCode",
                    Value = privatePersonalIdentifier
                }
            };

            var responseTask = universalDataSetService.ExecuteAsync("RUMIS_darbinieku_dati", queryParams);

            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForGetEmployeeOperation(privatePersonalIdentifier), cancellationToken);

            var response = await responseTask;

            var reader = new StringReader(response.OuterXml);

            var serializer = new XmlSerializer(typeof(EmployeeData));

            var employee = (EmployeeData)serializer.Deserialize(reader);

            if (employee == null && !environment.IsProduction())
            {
                var xmlName = string.Empty;

                xmlName = $"asEmployee_{privatePersonalIdentifier}.xml";

                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = $"{assembly.GetName().Name}.Viis.Data.{xmlName}";

                using var stream = assembly.GetManifestResourceStream(resourceName);

                if (stream == null)
                {
                    logger.LogInformation("Fake VIIS students data not found.");

                    return employee;
                }

                using var streamReader = new StreamReader(stream);

                employee = (EmployeeData)serializer.Deserialize(streamReader);
            }

            await gdprAuditService.TraceRangeAsync(employee.Select(GdprAuditHelper.ProjectEmployeeTraces(privatePersonalIdentifier)).ToArray(), cancellationToken);

            return employee;
        }

        public async Task<Dictionary<string, bool>> CheckSocialStatusesAsync(string privatePersonalIdentifier, IEnumerable<string> statusTypes, CancellationToken cancellationToken = default)
        {
            var response = new Dictionary<string, bool>();

            foreach (var type in statusTypes)
            {
                var data = await CheckSocialStatusAsync(privatePersonalIdentifier, type, cancellationToken);
                response.Add(type, data.Any(d => d.SocialStatus.Any(s => s.StatusTypeCode == type)));
            }

            return response;
        }

        public async Task<List<SocialStatusData.Student>> CheckSocialStatusAsync(string privatePersonalIdentifier, string type, CancellationToken cancellationToken = default)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var queryParams = new QueryParamString[]
            {
                new QueryParamString
                {
                    Name = "@varchar_StudentPersonCode",
                    Value = privatePersonalIdentifier
                },
                 new QueryParamString
                {
                    Name = "@varchar_SocialStatusType",
                    Value = type
                }
            };

            var responseTask = universalDataSetService.ExecuteAsync("RUMIS_socialie_statusi", queryParams);

            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForCheckSocialStatusesOperation(privatePersonalIdentifier, type), cancellationToken);

            var response = await responseTask;

            var data = new SocialStatusData();

            var reader = new StringReader(response.OuterXml);

            var serializer = new XmlSerializer(typeof(SocialStatusData));

            var socialStatuses = (SocialStatusData)serializer.Deserialize(reader);

            if (socialStatuses.Count == 0 && !environment.IsProduction())
            {
                var xmlName = string.Empty;

                xmlName = $"socialStatus{type}_{privatePersonalIdentifier}.xml";

                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = $"{assembly.GetName().Name}.Viis.Data.SocialStatus.{type}.{xmlName}";

                using var stream = assembly.GetManifestResourceStream(resourceName);

                if (stream == null)
                {
                    logger.LogInformation("Fake VIIS students data not found.");

                    return socialStatuses;
                }

                using var streamReader = new StreamReader(stream);

                socialStatuses = (SocialStatusData)serializer.Deserialize(streamReader);
            }

            await gdprAuditService.TraceRangeAsync(socialStatuses.Select(GdprAuditHelper.ProjectCheckSocialStatusesTraces(privatePersonalIdentifier, type)).ToArray(), cancellationToken);

            return socialStatuses;
        }

        /// <inheritdoc/>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<DateTime> GetBirthDateAsync(string submitterPersonalIdentifier, string targetPrivatePersonalIdentifier, string type, CancellationToken cancellationToken = default)
        {
            RequestParamType requestParamType = type switch
            {
                ApplicantRole.Learner => RequestParamType.Student,
                ApplicantRole.EducationalInstitution => RequestParamType.Student,
                ApplicantRole.ParentGuardian => RequestParamType.Parent,
                _ => throw new NotImplementedException()
            };

            var data = new List<RelatedPersonData.Student>();

            data = type == ApplicantRole.EducationalInstitution
                ? await GetStudentsAsync(requestParamType, targetPrivatePersonalIdentifier, cancellationToken)
                : await GetStudentsAsync(requestParamType, submitterPersonalIdentifier, cancellationToken);

            var birthDate = data
                .Where(x => x.PersonCode == targetPrivatePersonalIdentifier)
                .Select(x => x.BirthDate)
                .First();

            return birthDate;
        }


        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetStudentsByParentOrGuardianAsync(string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            IEnumerable<string> result = null;

            var cacheKey = $"ViisService.GetStudentsByParentOrGuardianAsync/{privatePersonalIdentifier}";

            var cache = await distributedCache.GetStringAsync(cacheKey, cancellationToken);

            if (cache != null)
                result = JsonSerializer.Deserialize<IEnumerable<string>>(cache);
            else
            {
                var data = await GetStudentsAsync(RequestParamType.Parent, privatePersonalIdentifier, cancellationToken);

                result = data.Select(t => t.PersonCode);

                await distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(options.StudentPersonCodeCacheDuration)
                }, cancellationToken);
            }

            await gdprAuditService.TraceRangeAsync(result.Select(GdprAuditHelper.ProjectPrivatePersonalIdentifierTraces(privatePersonalIdentifier)).ToArray(), cancellationToken);

            return result;
        }

        public static class GdprAuditHelper
        {
            public static GdprAuditTraceDto GenerateTraceForGetStudentsOperation(string privatePersonalIdentifier)
            {
                return new GdprAuditTraceDto
                {
                    Action = "viis.getStudents",
                    DataOwnerPrivatePersonalIdentifier = privatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = privatePersonalIdentifier }
                    }
                };
            }

            public static GdprAuditTraceDto GenerateTraceForGetEmployeeOperation(string privatePersonalIdentifier)
            {
                return new GdprAuditTraceDto
                {
                    Action = "viis.getEmployees",
                    DataOwnerPrivatePersonalIdentifier = privatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = privatePersonalIdentifier }
                    }
                };
            }

            public static GdprAuditTraceDto GenerateTraceForCheckEducationalInstitutionAsEmployee(string privatePersonalIdentifier, string educationalInstitutionCode)
            {
                return new GdprAuditTraceDto
                {
                    Action = "viis.checkEducationalInstitutionAsEmployee",
                    ActionData = JsonSerializer.Serialize(new { RequestPrivatePersonalIdentifier = privatePersonalIdentifier, EducationalInstitutionCode = educationalInstitutionCode }),
                    DataOwnerPrivatePersonalIdentifier = privatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = privatePersonalIdentifier }
                    }
                };
            }

            public static GdprAuditTraceDto GenerateTraceForCheckEducationalInstitutionAsStudent(string privatePersonalIdentifier, string educationalInstitutionCode)
            {
                return new GdprAuditTraceDto
                {
                    Action = "viis.checkEducationalInstitutionAsStudent",
                    ActionData = JsonSerializer.Serialize(new { RequestPrivatePersonalIdentifier = privatePersonalIdentifier, EducationalInstitutionCode = educationalInstitutionCode }),
                    DataOwnerPrivatePersonalIdentifier = privatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = privatePersonalIdentifier }
                    }
                };
            }

            public static Func<string, GdprAuditTraceDto> ProjectPrivatePersonalIdentifierTraces(string privatePersonalIdentifier)
            {
                return privatePersonalIdentifier => new GdprAuditTraceDto
                {
                    Action = "viis.getStudentsByParentOrGuardian",
                    ActionData = JsonSerializer.Serialize(new { RequestPrivatePersonalIdentifier = privatePersonalIdentifier }),
                    DataOwnerPrivatePersonalIdentifier = privatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = privatePersonalIdentifier }
                    }
                };
            }

            public static GdprAuditTraceDto GenerateTraceForCheckSocialStatusesOperation(string privatePersonalIdentifier, string type)
            {
                return new GdprAuditTraceDto
                {
                    Action = "viis.checkSocialStatuses",
                    ActionData = JsonSerializer.Serialize(new { RequestPrivatePersonalIdentifier = privatePersonalIdentifier, Type = type }),
                    DataOwnerPrivatePersonalIdentifier = privatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = privatePersonalIdentifier }
                    }
                };
            }

            public static Func<RelatedPersonData.Student, GdprAuditTraceDto> ProjectStudentTraces(string privatePersonalIdentifier)
            {
                return student => new GdprAuditTraceDto
                {
                    Action = "viis.getStudents",
                    ActionData = JsonSerializer.Serialize(new { RequestPrivatePersonalIdentifier = privatePersonalIdentifier }),
                    DataOwnerPrivatePersonalIdentifier = student.PersonCode,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.BirthDate, Value = student.BirthDate.ToString() },
                        new PersonDataProperty { Type = PersonDataType.FirstName, Value = student.Name },
                        new PersonDataProperty { Type = PersonDataType.LastName, Value = student.Surname },
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = student.PersonCode }
                    }
                };
            }

            public static Func<EmployeeData.Employee, GdprAuditTraceDto> ProjectEmployeeTraces(string privatePersonalIdentifier)
            {
                return employee => new GdprAuditTraceDto
                {
                    Action = "viis.getEmployees",
                    ActionData = JsonSerializer.Serialize(new { RequestPrivatePersonalIdentifier = privatePersonalIdentifier }),
                    DataOwnerPrivatePersonalIdentifier = employee.PersonCode,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.FirstName, Value = employee.Name },
                        new PersonDataProperty { Type = PersonDataType.LastName, Value = employee.Surname },
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = employee.PersonCode }
                    }
                };
            }

            public static Func<SocialStatusData.Student, GdprAuditTraceDto> ProjectCheckSocialStatusesTraces(string privatePersonalIdentifier, string type)
            {
                return student => new GdprAuditTraceDto
                {
                    Action = "viis.checkSocialStatuses",
                    ActionData = JsonSerializer.Serialize(new { RequestPrivatePersonalIdentifier = privatePersonalIdentifier, Type = type }),
                    DataOwnerPrivatePersonalIdentifier = student.PersonCode,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.FirstName, Value = student.Name },
                        new PersonDataProperty { Type = PersonDataType.LastName, Value = student.Surname },
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = student.PersonCode }
                    }
                };
            }

            public static Func<RelatedPersonData.Student, GdprAuditTraceDto> ProjectInstitutionAsStudentTraces(string privatePersonalIdentifier, string educationalInstitution)
            {
                return student => new GdprAuditTraceDto
                {
                    Action = "viis.checkEducationalInstitutionAsStudent",
                    ActionData = JsonSerializer.Serialize(new { RequestPrivatePersonalIdentifier = privatePersonalIdentifier, EducationalInstitution = educationalInstitution }),
                    DataOwnerPrivatePersonalIdentifier = student.PersonCode,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.BirthDate, Value = student.BirthDate.ToString() },
                        new PersonDataProperty { Type = PersonDataType.FirstName, Value = student.Name },
                        new PersonDataProperty { Type = PersonDataType.LastName, Value = student.Surname },
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = student.PersonCode }
                    }
                };
            }

            public static Func<EmployeeData.Employee, GdprAuditTraceDto> ProjectInstitutionAsEmployeeTraces(string privatePersonalIdentifier, string educationalInstitution)
            {
                return employee => new GdprAuditTraceDto
                {
                    Action = "viis.checkEducationalInstitutionAsEmployee",
                    ActionData = JsonSerializer.Serialize(new { RequestPrivatePersonalIdentifier = privatePersonalIdentifier, EducationalInstitution = educationalInstitution }),
                    DataOwnerPrivatePersonalIdentifier = employee.PersonCode,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.FirstName, Value = employee.Name },
                        new PersonDataProperty { Type = PersonDataType.LastName, Value = employee.Surname },
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = employee.PersonCode }
                    }
                };
            }
        }
    }
}
