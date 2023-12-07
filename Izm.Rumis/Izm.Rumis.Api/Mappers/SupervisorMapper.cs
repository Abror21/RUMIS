using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    public static class SupervisorMapper
    {
        public static Expression<Func<Supervisor, SupervisorResponse>> Project()
        {
            return t => new SupervisorResponse
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name
            };
        }

        public static Expression<Func<Supervisor, SupervisorByIdResponse>> ProjectById()
        {
            return t => new SupervisorByIdResponse
            {
                Name = t.Name,
                Code = t.Code,
                IsActive = t.IsActive,
                ActiveResources = t.EducationalInstitutions
                    .SelectMany(e => e.Resources)
                    .Where(r => ResourceStatus.ActiveStatuses.Contains(r.ResourceStatus.Code))
                    .Count(),
                ResourcesInUsePersonally = t.EducationalInstitutions
                    .SelectMany(e => e.Resources)
                    .Where(r => r.ResourceStatus.Code == ResourceStatus.InUse && r.UsagePurposeType.Code == ResourceUsingPurpose.IssuedIndividually)
                    .Count(),
                ResourcesInUseEducationally = t.EducationalInstitutions
                    .SelectMany(e => e.Resources)
                    .Where(r => r.ResourceStatus.Code == ResourceStatus.InUse && r.UsagePurposeType.Code == ResourceUsingPurpose.InstitutionLearningProcess)
                    .Count(),
                Applications = t.EducationalInstitutions
                    .SelectMany(e => e.Applications)
                    .Count(),
                ApplicationsAccepted = t.EducationalInstitutions
                    .SelectMany(e => e.Applications)
                    .Where(a => a.ApplicationStatus.Code == ApplicationStatus.Confirmed)
                    .Count(),
                ApplicationsAwaitingResources = t.EducationalInstitutions
                    .SelectMany(e => e.Applications)
                    .Where(a => a.ApplicationStatus.Code == ApplicationStatus.Submitted)
                    .Count(),
                ApplicationsPostponed = t.EducationalInstitutions
                    .SelectMany(e => e.Applications)
                    .Where(a => a.ApplicationStatus.Code == ApplicationStatus.Postponed)
                    .Count(),
                EducationalInstitutions = t.EducationalInstitutions.Count(),
                ActiveEducationalInstitutions = t.EducationalInstitutions
                    .Where(e => e.Status.Code == EducationalInstitutionStatus.Active)
                    .Count(),
                CountryDocumentTemplates = t.DocumentTemplates
                    .Where(d => d.PermissionType == UserProfileType.Country && d.Code != DocumentType.Hyperlink)
                    .Count(),
                EducationalInstitutionsDocumentTemplates = t.DocumentTemplates
                    .Where(d => d.PermissionType == UserProfileType.Supervisor && d.Code != DocumentType.Hyperlink)
                    .Count(),
                EducationalInstitutionsDocumentLinks = t.DocumentTemplates
                    .Where(d => d.PermissionType == UserProfileType.Supervisor && d.Code == DocumentType.Hyperlink)
                    .Count()
            };
        }

        public static Expression<Func<Supervisor, SupervisorListItemResponse>> ProjectSupervisorListItem()
        {
            return t => new SupervisorListItemResponse
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Status = t.IsActive,
                EducationalInstitutions = t.EducationalInstitutions.Count(),
                ActiveEducationalInstitutions = t.EducationalInstitutions
                .Where(e => e.Status.Code == EducationalInstitutionStatus.Active)
                .Count(),
            };
        }

        public static SupervisorCreateDto Map(SupervisorCreateRequest model, SupervisorCreateDto dto)
        {
            dto.Code = model.Code;
            dto.Name = model.Name;

            return dto;
        }

        public static SupervisorUpdateDto Map(SupervisorUpdateRequest model, SupervisorUpdateDto dto)
        {
            dto.Code = model.Code;
            dto.Name = model.Name;
            dto.IsActive = model.IsActive;

            return dto;
        }
    }
}
