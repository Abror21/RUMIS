using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    public static class UserProfileMapper
    {
        public static UserProfileResponse Map(UserProfileIntermediateResponse intermediate, UserProfileResponse model)
        {
            model.ConfigurationInfo = intermediate.ConfigurationInfo;
            model.Disabled = intermediate.Disabled;
            model.EducationalInstitution = intermediate.EducationalInstitution == null ? null : new UserProfileResponse.EducationalInstitutionData
            {
                Code = intermediate.EducationalInstitution.Code,
                Id = intermediate.EducationalInstitution.Id,
                Name = intermediate.EducationalInstitution.Name
            };
            model.Email = intermediate.Email;
            model.Expires = intermediate.Expires;
            model.Id = intermediate.Id;
            model.InstitutionId = intermediate.InstitutionId == null ? null : new UserProfileResponse.ClassifierData
            {
                Code = intermediate.InstitutionId.Code,
                Id = intermediate.InstitutionId.Id,
                Value = intermediate.InstitutionId.Value
            };
            model.IsLoggedIn = intermediate.IsLoggedIn;
            model.Job = intermediate.Job;
            model.Notes = intermediate.Notes;
            model.PhoneNumber = intermediate.PhoneNumber;
            model.ProfileCreationDocumentDate = intermediate.ProfileCreationDocumentDate;
            model.ProfileCreationDocumentNumber = intermediate.ProfileCreationDocumentNumber;
            model.Roles = intermediate.Roles.Select(t => new UserProfileResponse.RoleData
            {
                Code = t.Code,
                Id = t.Id,
                Name = t.Name
            }).ToArray();
            model.Supervisor = intermediate.Supervisor == null ? null : new UserProfileResponse.SupervisorData
            {
                Code = intermediate.Supervisor.Code,
                Id = intermediate.Supervisor.Id,
                Name = intermediate.Supervisor.Name
            };
            model.Type = intermediate.Type;
            model.UserId = intermediate.UserId;

            return model;
        }

        public static Expression<Func<UserProfile, UserProfileIntermediateResponse>> ProjectIntermediate()
        {
            return t => new UserProfileIntermediateResponse
            {
                Id = t.Id,
                ConfigurationInfo = t.ConfigurationInfo,
                Disabled = t.Disabled,
                EducationalInstitution = t.EducationalInstitution == null ? null : new UserProfileIntermediateResponse.EducationalInstitutionData
                {
                    Id = t.EducationalInstitution.Id,
                    Code = t.EducationalInstitution.Code,
                    Name = t.EducationalInstitution.Name
                },
                Email = t.Email,
                Expires = t.Expires,
                Supervisor = t.Supervisor == null ? null : new UserProfileIntermediateResponse.SupervisorData
                {
                    Id = t.Supervisor.Id,
                    Code = t.Supervisor.Code,
                    Name = t.Supervisor.Name
                },
                IsLoggedIn = t.IsLoggedIn,
                InstitutionId = t.Institution == null ? null : new UserProfileIntermediateResponse.ClassifierData
                {
                    Id = t.Institution.Id,
                    Code = t.Institution.Code,
                    Value = t.Institution.Value
                },
                Job = t.Job,
                Notes = t.Notes,
                PersonTechnicalId = t.User.PersonTechnical == null ? null : t.User.PersonTechnical.Id,
                PhoneNumber = t.PhoneNumber,
                ProfileCreationDocumentDate = t.ProfileCreationDocumentDate,
                ProfileCreationDocumentNumber = t.ProfileCreationDocumentNumber,
                Roles = t.Roles.Select(t => new UserProfileIntermediateResponse.RoleData
                {
                    Id = t.Id,
                    Code = t.Code,
                    Name = t.Name
                }),
                Type = t.PermissionType,
                UserId = t.UserId
            };
        }

        public static Func<UserProfileIntermediateResponse, UserProfileResponse> ProjectFromIntermediate()
        {
            return t => new UserProfileResponse
            {
                Id = t.Id,
                ConfigurationInfo = t.ConfigurationInfo,
                Disabled = t.Disabled,
                EducationalInstitution = t.EducationalInstitution == null ? null : new UserProfileResponse.EducationalInstitutionData
                {
                    Id = t.EducationalInstitution.Id,
                    Code = t.EducationalInstitution.Code,
                    Name = t.EducationalInstitution.Name
                },
                Email = t.Email,
                Expires = t.Expires,
                Supervisor = t.Supervisor == null ? null : new UserProfileResponse.SupervisorData
                {
                    Id = t.Supervisor.Id,
                    Code = t.Supervisor.Code,
                    Name = t.Supervisor.Name
                },
                IsLoggedIn = t.IsLoggedIn,
                InstitutionId = t.InstitutionId == null ? null : new UserProfileResponse.ClassifierData
                {
                    Id = t.InstitutionId.Id,
                    Code = t.InstitutionId.Code,
                    Value = t.InstitutionId.Value
                },
                Job = t.Job,
                Notes = t.Notes,
                PhoneNumber = t.PhoneNumber,
                ProfileCreationDocumentDate = t.ProfileCreationDocumentDate,
                ProfileCreationDocumentNumber = t.ProfileCreationDocumentNumber,
                Roles = t.Roles.Select(t => new UserProfileResponse.RoleData
                {
                    Id = t.Id,
                    Code = t.Code,
                    Name = t.Name
                }),
                Type = t.Type,
                UserId = t.UserId
            };
        }

        public static Expression<Func<UserProfile, UserProfileListItemIntermediateResponse>> ProjectListItemIntermediate()
        {
            return t => new UserProfileListItemIntermediateResponse
            {
                Id = t.Id,
                Disabled = t.Disabled,
                EducationalInstitution = t.EducationalInstitution == null ? null : new UserProfileListItemIntermediateResponse.EducationalInstitutionData
                {
                    Id = t.EducationalInstitution.Id,
                    Code = t.EducationalInstitution.Code,
                    Name = t.EducationalInstitution.Name
                },
                Email = t.Email,
                Expires = t.Expires,
                Supervisor = t.Supervisor == null ? null : new UserProfileListItemIntermediateResponse.SupervisorData
                {
                    Id = t.Supervisor.Id,
                    Code = t.Supervisor.Code,
                    Name = t.Supervisor.Name
                },
                IsLoggedIn = t.IsLoggedIn,
                Persons = t.User.PersonTechnical == null ? null : t.User.PersonTechnical.Persons.Select(n => new UserProfileListItemIntermediateResponse.PersonData
                {
                    FirstName = n.FirstName,
                    LastName = n.LastName
                }),
                PersonTechnicalId = t.User.PersonTechnical == null ? null : t.User.PersonTechnical.Id,
                PhoneNumber = t.PhoneNumber,
                Roles = t.Roles.Select(t => new UserProfileListItemIntermediateResponse.RoleData
                {
                    Id = t.Id,
                    Code = t.Code,
                    Name = t.Name
                }),
                Type = t.PermissionType,
                UserId = t.UserId
            };
        }

        public static Func<UserProfileListItemIntermediateResponse, UserProfileListItemResponse> ProjectListItemFromIntermediate()
        {
            return t => new UserProfileListItemResponse
            {
                Id = t.Id,
                Disabled = t.Disabled,
                EducationalInstitution = t.EducationalInstitution == null ? null : new UserProfileListItemResponse.EducationalInstitutionData
                {
                    Id = t.EducationalInstitution.Id,
                    Code = t.EducationalInstitution.Code,
                    Name = t.EducationalInstitution.Name
                },
                Email = t.Email,
                Expires = t.Expires,
                Supervisor = t.Supervisor == null ? null : new UserProfileListItemResponse.SupervisorData
                {
                    Id = t.Supervisor.Id,
                    Code = t.Supervisor.Code,
                    Name = t.Supervisor.Name
                },
                IsLoggedIn = t.IsLoggedIn,
                Persons = t.Persons?.Select(n => new UserProfileListItemResponse.PersonData
                {
                    FirstName = n.FirstName,
                    LastName = n.LastName
                }),
                PhoneNumber = t.PhoneNumber,
                Roles = t.Roles.Select(t => new UserProfileListItemResponse.RoleData
                {
                    Id = t.Id,
                    Code = t.Code,
                    Name = t.Name
                }),
                Type = t.Type,
                UserId = t.UserId
            };
        }

        public static UserProfileEditDto Map(UserProfileEditRequest model, UserProfileEditDto dto)
        {
            dto.ConfigurationInfo = model.ConfigurationInfo;
            dto.EducationalInstitutionId = model.EducationalInstitutionId;
            dto.Email = model.Email;
            dto.Expires = model.Expires;
            dto.SupervisorId = model.SupervisorId;
            dto.InstitutionId = model.InstitutionId;
            dto.IsDisabled = model.IsDisabled;
            dto.Job = model.Job;
            dto.Notes = model.Notes;
            dto.PhoneNumber = model.PhoneNumber;
            dto.ProfileCreationDocumentDate = model.ProfileCreationDocumentDate;
            dto.ProfileCreationDocumentNumber = model.ProfileCreationDocumentNumber;
            dto.RoleIds = model.RoleIds;
            dto.PermissionType = model.Type;
            dto.UserId = model.UserId;

            return dto;
        }
    }
}
