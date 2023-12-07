using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    public static class EducationalInstitutionMapper
    {
        public static Expression<Func<EducationalInstitution, EducationalInstitutionResponse>> Project()
        {
            return t => new EducationalInstitutionResponse
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Address = t.Address,
                City = t.City,
                District = t.District,
                Email = t.Email,
                PhoneNumber = t.PhoneNumber,
                Municipality = t.Municipality,
                Village = t.Village,
                Status = new EducationalInstitutionResponse.ClassifierData
                {
                    Id = t.Status.Id,
                    Code = t.Status.Code,
                    Value = t.Status.Value
                },
                Supervisor = new EducationalInstitutionResponse.SupervisorData
                {
                    Id = t.Supervisor.Id,
                    Code = t.Supervisor.Code,
                    Name = t.Supervisor.Name
                },
                EducationalInstitutionContactPersons = t.EducationalInstitutionContactPersons.Select(t => new EducationalInstitutionResponse.EducationalInstitutionContactPersonData
                {
                    Id = t.Id,
                    Name = t.Name,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    Address = t.Address,
                    JobPosition = new EducationalInstitutionResponse.ClassifierData
                    {
                        Id = t.JobPosition.Id,
                        Code = t.JobPosition.Code,
                        Value = t.JobPosition.Value
                    },
                    ContactPersonResourceSubTypes = t.ContactPersonResourceSubTypes.Select(t => new EducationalInstitutionResponse.EducationalInstitutionContactPersonData.ContactPersonResourceSubTypeData
                    {
                        Id = t.Id,
                        ResourceSubType = new EducationalInstitutionResponse.ClassifierData
                        {
                            Id = t.ResourceSubType.Id,
                            Code = t.ResourceSubType.Code,
                            Value = t.ResourceSubType.Value
                        }
                    })
                }),
                EducationalInstitutionResourceSubTypes = t.EducationalInstitutionResourceSubTypes.Select(t => new EducationalInstitutionResponse.EducationalInstitutionResourceSubTypeData
                {
                    Id = t.Id,
                    ResourceSubType = new EducationalInstitutionResponse.ClassifierData
                    {
                        Id = t.ResourceSubType.Id,
                        Code = t.ResourceSubType.Code,
                        Value = t.ResourceSubType.Value
                    },
                    TargetPersonGroupType = new EducationalInstitutionResponse.ClassifierData
                    {
                        Id = t.TargetPersonGroupType.Id,
                        Code = t.TargetPersonGroupType.Code,
                        Value = t.TargetPersonGroupType.Value
                    }
                })
            };
        }

        public static Expression<Func<EducationalInstitution, EducationalInstitutionListItemResponse>> ProjectEducationalInstitutionListItem()
        {
            return e => new EducationalInstitutionListItemResponse
            {
                Id = e.Id,
                Code = e.Code,
                Name = e.Name,
                Status = new EducationalInstitutionListItemResponse.ClassifierData
                {
                    Id = e.Status.Id,
                    Code = e.Status.Code,
                    Value = e.Status.Value
                },
                Supervisor = e.Supervisor == null ? null : new EducationalInstitutionListItemResponse.SupervisorData
                {
                    Id = e.Supervisor.Id,
                    Code = e.Supervisor.Code,
                    Name = e.Supervisor.Name
                }
            };
        }

        public static EducationalInstitutionCreateDto Map(EducationalInstitutionCreateRequest model, EducationalInstitutionCreateDto dto)
        {
            dto.Code = model.Code;
            dto.Name = model.Name;
            dto.Address = model.Address;
            dto.City = model.City;
            dto.District = model.District;
            dto.Email = model.Email;
            dto.PhoneNumber = model.PhoneNumber;
            dto.Municipality = model.Municipality;
            dto.Village = model.Village;
            dto.SupervisorId = model.SupervisorId;
            dto.StatusId = model.StatusId;

            return dto;
        }

        public static EducationalInstitutionUpdateDto Map(EducationalInstitutionUpdateRequest model, EducationalInstitutionUpdateDto dto)
        {
            dto.Code = model.Code;
            dto.Name = model.Name;
            dto.Address = model.Address;
            dto.City = model.City;
            dto.District = model.District;
            dto.Email = model.Email;
            dto.PhoneNumber = model.PhoneNumber;
            dto.Municipality = model.Municipality;
            dto.Village = model.Village;
            dto.SupervisorId = model.SupervisorId;
            dto.StatusId = model.StatusId;
            dto.EducationalInstitutionContactPersons = model.EducationalInstitutionContactPersons.Select(t => new EducationalInstitutionUpdateDto.EducationalInstitutionContactPersonData
            {
                Id = t.Id,
                Name = t.Name,
                Email = t.Email,
                PhoneNumber = t.PhoneNumber,
                Address = t.Address,
                JobPositionId = t.JobPositionId,
                ContactPersonResourceSubTypes = t.ContactPersonResourceSubTypes.Select(n => new EducationalInstitutionUpdateDto.EducationalInstitutionContactPersonData.ContactPersonResourceSubTypeData
                {
                    Id = n.Id,
                    ResourceSubTypeId = n.ResourceSubTypeId
                })
            });
            dto.EducationalInstitutionResourceSubTypes = model.EducationalInstitutionResourceSubTypes.Select(t => new EducationalInstitutionUpdateDto.EducationalInstitutionResourceSubTypeData
            {
                Id = t.Id,
                ResourceSubTypeId = t.ResourceSubTypeId,
                TargetPersonGroupTypeId = t.TargetPersonGroupTypeId,
                IsActive = t.IsActive
            });

            return dto;
        }
    }
}
