using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.EServices.Dtos;
using Izm.Rumis.Infrastructure.Viis.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using static Izm.Rumis.Infrastructure.EServices.Dtos.EServiceEmployeeResponseDto;
using static Izm.Rumis.Infrastructure.EServices.Dtos.EServicesRelatedPersonResponseDto;

namespace Izm.Rumis.Infrastructure.EServices
{
    internal static class EServiceMapper
    {
        public static ApplicationCreateDto Map(EServiceApplicationCreateDto eserviceDto, ApplicationCreateDto dto)
        {
            dto.ApplicationSocialStatuses = eserviceDto.ApplicationSocialStatuses;
            dto.ApplicationStatusHistory = eserviceDto.ApplicationStatusHistory;
            dto.EducationalInstitutionId = eserviceDto.EducationalInstitutionId;
            dto.Notes = eserviceDto.Notes;
            dto.ResourceSubTypeId = eserviceDto.ResourceSubTypeId;
            dto.ResourceTargetPerson = eserviceDto.ResourceTargetPerson == null ? null : new PersonData
            {
                ContactInformation = eserviceDto.ResourceTargetPerson.ContactInformation.Select(t => new PersonData.ContactData
                {
                    TypeId = t.TypeId,
                    Value = t.Value
                }).ToArray(),
                FirstName = eserviceDto.ResourceTargetPerson.FirstName,
                LastName = eserviceDto.ResourceTargetPerson.LastName,
                PrivatePersonalIdentifier = eserviceDto.ResourceTargetPerson.PrivatePersonalIdentifier
            };
            dto.ResourceTargetPersonClassGrade = eserviceDto.ResourceTargetPersonClassGrade;
            dto.ResourceTargetPersonClassParallel = eserviceDto.ResourceTargetPersonClassParallel;
            dto.ResourceTargetPersonEducationalProgram = eserviceDto.ResourceTargetPersonEducationalProgram;
            dto.ResourceTargetPersonEducationalStatusId = eserviceDto.ResourceTargetPersonEducationalStatusId;
            dto.ResourceTargetPersonEducationalSubStatusId = eserviceDto.ResourceTargetPersonEducationalSubStatusId;
            dto.ResourceTargetPersonGroup = eserviceDto.ResourceTargetPersonGroup;
            dto.ResourceTargetPersonTypeId = eserviceDto.ResourceTargetPersonTypeId;
            dto.ResourceTargetPersonWorkStatusId = eserviceDto.ResourceTargetPersonWorkStatusId;
            dto.SocialStatus = eserviceDto.SocialStatus;
            dto.SocialStatusApproved = eserviceDto.SocialStatusApproved;
            dto.SubmitterTypeId = eserviceDto.SubmitterTypeId;

            return dto;
        }

        public static ApplicationCheckDuplicateDto Map(EServiceApplicationCheckDuplicateDto eserviceDto, ApplicationCheckDuplicateDto dto)
        {
            dto.PrivatePersonalIdentifier = eserviceDto.PrivatePersonalIdentifier;
            dto.ResourceSubTypeId = eserviceDto.ResourceSubTypeId;

            return dto;
        }

        public static ActiveEducationDataResponse Map(EServicesEducationalInstitutionDto data, ActiveEducationDataResponse response)
        {
            response.EducationalInstitutionId = data.Id;
            response.EducationInstitutionStatus = new ActiveEducationDataResponse.ClassifierData
            {
                Id = data.Status.Id,
                Code = data.Status.Code,
                Value = data.Status.Value
            };

            return response;
        }

        public static ActiveWorkDataResponse Map(EServicesEducationalInstitutionDto data, ActiveWorkDataResponse response)
        {
            response.EducationalInstitutionId = data.Id;
            response.SupervisorName = data.Supervisor.Name;
            response.SupervisorCode = data.Supervisor.Code;

            return response;
        }

        public static ApplicationResourceChangeStatusDto Map(EServiceChangeStatusDto data, ApplicationResourceChangeStatusDto dto)
        {
            dto.FileToDeleteIds = data.FileToDeleteIds;
            dto.Files = data.Files;
            dto.Notes = data.Notes;

            return dto;
        }

        public static Expression<Func<EducationalInstitution, EServicesEducationalInstitutionDto>> ProjectEducationalInstitution()
        {
            return t => new EServicesEducationalInstitutionDto
            {
                Id = t.Id,
                Code = t.Code,
                Status = new EServicesEducationalInstitutionDto.ClassifierData
                {
                    Id = t.Status.Id,
                    Code = t.Status.Code,
                    Value = t.Status.Value
                },
                Supervisor = new EServicesEducationalInstitutionDto.SupervisorData
                {
                    Id = t.Supervisor.Id,
                    Code = t.Supervisor.Code,
                    Name = t.Supervisor.Name
                }
            };
        }

        public static Func<RelatedPersonData.Student, EServicesRelatedPersonResponseDto> ProjectRelatedPerson()
        {
            return t => new EServicesRelatedPersonResponseDto
            {
                BirthDate = t.BirthDate.ToString(),
                FirstName = t.Name,
                LastName = t.Surname,
                PrivatePersonalIdentifier = t.PersonCode,
                ActiveEducationData = t.Institution == null ? null : t.Institution.Select(inst => new ActiveEducationDataResponse
                {
                    ClassGroup = inst.Class.ClassGrade,
                    ClassGroupLevel = inst.Class.Type,
                    EducationProgram = inst.Class.EducationProgramName,
                    EducationProgramCode = inst.Class.EducationProgramCode,
                    EducationInstitutionCode = inst.RegNr,
                    EducationInstitutionName = inst.Name,
                    SupervisorName = inst.FounderName,
                    SupervisorCode = inst.FounderATUCCode
                }).ToArray()
            };
        }

        public static Func<EmployeeData.Employee, EServiceEmployeeResponseDto> ProjectEmployee()
        {
            return t => new EServiceEmployeeResponseDto
            {
                FirstName = t.Name,
                LastName = t.Surname,
                PrivatePersonalIdentifier = t.PersonCode,
                ActiveWorkData = t.Institution == null ? null : t.Institution.Select(inst => new ActiveWorkDataResponse
                {
                    EducationInstitutionCode = inst.RegNr,
                    EducationInstitutionName = inst.Name,
                    PositionName = inst.PositionName,
                    PositionCode = inst.PositionCode
                }).ToArray()
            };
        }
    }
}
