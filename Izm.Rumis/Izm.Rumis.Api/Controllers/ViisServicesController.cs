using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Izm.Rumis.Infrastructure.Enums;
using Izm.Rumis.Infrastructure.Viis;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.ViisServicesView)]
    public class ViisServicesController : ApiController
    {
        private readonly IEducationalInstitutionService educationalInstitutionService;
        private readonly IViisService viisService;
        private readonly IGdprAuditService gdprAuditService;

        public ViisServicesController(IEducationalInstitutionService educationalInstitutionService, IViisService viisService, IGdprAuditService gdprAuditService)
        {
            this.educationalInstitutionService = educationalInstitutionService;
            this.viisService = viisService;
            this.gdprAuditService = gdprAuditService;
        }

        [HttpPost("checkPersonApplications")]
        public async Task<ActionResult> CheckPersonApplications(CancellationToken cancellationToken = default)
        {
            await viisService.CheckPersonApplicationsAsync(cancellationToken);

            return NoContent();
        }

        [HttpGet("asParentOrGuardian")]
        public async Task<ActionResult<IEnumerable<ViisRelatedPersonResponse>>> GetAsParentOrGuardian(string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            var data = await GetRelatedPersonData(privatePersonalIdentifier, RequestParamType.Parent, cancellationToken);

            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForGetAsParentOrGuardianOperation(privatePersonalIdentifier), cancellationToken);

            await gdprAuditService.TraceRangeAsync(data.Select(GdprAuditHelper.ProjectAsParentOrGuardianTraces(privatePersonalIdentifier)).ToArray(), cancellationToken);

            return data.ToArray();
        }

        [HttpGet("asEducatee")]
        public async Task<ActionResult<IEnumerable<ViisRelatedPersonResponse>>> GetAsEducatee(string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            var data = await GetRelatedPersonData(privatePersonalIdentifier, RequestParamType.Student, cancellationToken);

            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForGetAsEducateeOperation(privatePersonalIdentifier), cancellationToken);

            await gdprAuditService.TraceRangeAsync(data.Select(GdprAuditHelper.ProjectAsEducateeTraces(privatePersonalIdentifier)).ToArray(), cancellationToken);

            return data.ToArray();
        }

        [HttpGet("asEmployee")]
        public async Task<ActionResult<IEnumerable<ViisEmployeeDataResponse>>> GetAsEmployee(string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            var data = await GetEmployeesAsync(privatePersonalIdentifier, cancellationToken);

            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForGetAsEmployeeOperation(privatePersonalIdentifier), cancellationToken);

            await gdprAuditService.TraceRangeAsync(data.Select(GdprAuditHelper.ProjectAsEmployeeTraces(privatePersonalIdentifier)), cancellationToken);

            return data.ToArray();
        }

        private async Task<IEnumerable<ViisRelatedPersonResponse>> GetRelatedPersonData(string privatePersonalIdentifier, RequestParamType type, CancellationToken cancellationToken = default)
        {
            var data = await viisService.GetStudentsAsync(type, privatePersonalIdentifier, cancellationToken);

            var regNrs = data.SelectMany(student => student.Institution.Select(institution => institution.RegNr))
                .Distinct();

            var institutionData = await educationalInstitutionService.Get()
                .Where(institution => regNrs.Contains(institution.Code))
                .ListAsync(
                    map: institution => new
                    {
                        institution.Id,
                        institution.Code,
                        Status = new
                        {
                            institution.Status.Id,
                            institution.Status.Code,
                            institution.Status.Value
                        }
                    },
                    cancellationToken: cancellationToken
                );

            var institutionMapping = institutionData
                .GroupBy(intitution => intitution.Code)
                .Select(group => group.First())
                .ToDictionary(institution => institution.Code, institution => institution);

            var result = data.Select(ViisMapper.Project())
                .ToArray();

            foreach (var student in result)
                foreach (var institution in student.ActiveEducationData)
                    if (institutionMapping.TryGetValue(institution.EducationInstitutionCode, out var instData))
                    {
                        institution.EducationalInstitutionId = instData.Id;
                        institution.EducationInstitutionStatus = new ViisRelatedPersonResponse.ActiveEducationDataResponse.ClassifierData
                        {
                            Code = instData.Status.Code,
                            Id = instData.Status.Id,
                            Value = instData.Status.Value
                        };
                    }

            return result;
        }

        private async Task<IEnumerable<ViisEmployeeDataResponse>> GetEmployeesAsync(string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            var data = await viisService.GetEmployeesAsync(
                   privatePersonalIdentifier: privatePersonalIdentifier,
                   cancellationToken: cancellationToken);

            var regNrs = data.SelectMany(employee => employee.Institution.Select(institution => institution.RegNr))
                .Distinct();

            var institutionData = await educationalInstitutionService.Get()
                  .Where(institution => regNrs.Contains(institution.Code))
                  .ListAsync(
                      map: institution => new
                      {
                          institution.Id,
                          institution.Code,
                          Suppervisor = new
                          {
                              institution.Supervisor.Id,
                              institution.Supervisor.Name,
                              institution.Supervisor.Code
                          }

                      },
                      cancellationToken: cancellationToken
                  );

            var institutionMapping = institutionData
                .GroupBy(intitution => intitution.Code)
                .Select(group => group.First())
                .ToDictionary(institution => institution.Code, institution => institution);

            var result = data
                .Select(ViisMapper.ProjectEmployee())
                .ToArray();

            foreach (var employee in result)
                foreach (var institution in employee.ActiveWorkData)
                    if (institutionMapping.TryGetValue(institution.EducationInstitutionCode, out var instData))
                    {
                        institution.EducationalInstitutionId = instData.Id;
                        institution.SupervisorCode = instData.Suppervisor.Code;
                        institution.SupervisorName = instData.Suppervisor.Name;
                    }

            return result;
        }

        public static class GdprAuditHelper
        {
            private const string GetAsEducateeAction = "viis.getAsEducatee";
            private const string GetAsEmployeeAction = "viis.getAsEmployees";
            private const string GetAsParentOrGuardianAction = "viis.getAsParentOrGuardian";

            public static GdprAuditTraceDto GenerateTraceForGetAsEducateeOperation(string educateePrivatePersonalIdentifier)
                => GenerateTrace(educateePrivatePersonalIdentifier, GetAsEducateeAction);

            public static GdprAuditTraceDto GenerateTraceForGetAsEmployeeOperation(string educateePrivatePersonalIdentifier)
                => GenerateTrace(educateePrivatePersonalIdentifier, GetAsEmployeeAction);

            public static GdprAuditTraceDto GenerateTraceForGetAsParentOrGuardianOperation(string parentOrGuardianPrivatePersonalIdentifier)
                => GenerateTrace(parentOrGuardianPrivatePersonalIdentifier, GetAsParentOrGuardianAction);

            public static Func<ViisRelatedPersonResponse, GdprAuditTraceDto> ProjectAsEducateeTraces(string educateePrivatePersonalIdentifier)
                => ProjectTraces(educateePrivatePersonalIdentifier, GetAsEducateeAction);

            public static Func<ViisRelatedPersonResponse, GdprAuditTraceDto> ProjectAsParentOrGuardianTraces(string parentOrGuardianPrivatePersonalIdentifier)
                => ProjectTraces(parentOrGuardianPrivatePersonalIdentifier, GetAsParentOrGuardianAction);

            public static Func<ViisEmployeeDataResponse, GdprAuditTraceDto> ProjectAsEmployeeTraces(string educateePrivatePersonalIdentifier)
                => ProjectEmployeeTraces(educateePrivatePersonalIdentifier, GetAsEmployeeAction);

            private static GdprAuditTraceDto GenerateTrace(string privatePersonalIdentifier, string action)
            {
                return new GdprAuditTraceDto
                {
                    Action = action,
                    DataOwnerPrivatePersonalIdentifier = privatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = privatePersonalIdentifier }
                    }
                };
            }

            private static Func<ViisRelatedPersonResponse, GdprAuditTraceDto> ProjectTraces(string parentOrGuardianPrivatePersonalIdentifier, string action)
            {
                return t => new GdprAuditTraceDto
                {
                    Action = action,
                    ActionData = JsonSerializer.Serialize(new { ParentOrGuardianPrivatePersonalIdentifier = parentOrGuardianPrivatePersonalIdentifier }),
                    DataOwnerId = null,
                    DataOwnerPrivatePersonalIdentifier = t.PrivatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.BirthDate, Value = t.BirthDate },
                        new PersonDataProperty { Type = PersonDataType.FirstName, Value = t.FirstName },
                        new PersonDataProperty { Type = PersonDataType.LastName, Value = t.LastName },
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = t.PrivatePersonalIdentifier }
                    }
                };
            }

            private static Func<ViisEmployeeDataResponse, GdprAuditTraceDto> ProjectEmployeeTraces(string privatePersonalIdentifier, string action)
            {
                return t => new GdprAuditTraceDto
                {
                    Action = action,
                    ActionData = JsonSerializer.Serialize(new { PrivatePersonalIdentifier = privatePersonalIdentifier }),
                    DataOwnerId = null,
                    DataOwnerPrivatePersonalIdentifier = t.PrivatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.FirstName, Value = t.FirstName },
                        new PersonDataProperty { Type = PersonDataType.LastName, Value = t.LastName },
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = t.PrivatePersonalIdentifier }
                    }
                };
            }
        }
    }
}
