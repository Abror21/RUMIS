using Izm.Rumis.Application.Dto;

namespace Izm.Rumis.Application.Mappers
{
    internal static class ApplicationMapper
    {
        public static Domain.Entities.Application Map(ApplicationCreateDto dto, Domain.Entities.Application entity)
        {
            entity.ApplicationStatusHistory = dto.ApplicationStatusHistory;
            entity.EducationalInstitutionId = dto.EducationalInstitutionId;
            entity.Notes = dto.Notes;
            entity.ResourceSubTypeId = dto.ResourceSubTypeId;
            entity.ResourceTargetPersonClassGrade = dto.ResourceTargetPersonClassGrade;
            entity.ResourceTargetPersonClassParallel = dto.ResourceTargetPersonClassParallel;
            entity.ResourceTargetPersonEducationalProgram = dto.ResourceTargetPersonEducationalProgram;
            entity.ResourceTargetPersonEducationalStatusId = dto.ResourceTargetPersonEducationalStatusId;
            entity.ResourceTargetPersonEducationalSubStatusId = dto.ResourceTargetPersonEducationalSubStatusId;
            entity.ResourceTargetPersonGroup = dto.ResourceTargetPersonGroup;
            entity.ResourceTargetPersonTypeId = dto.ResourceTargetPersonTypeId;
            entity.ResourceTargetPersonWorkStatusId = dto.ResourceTargetPersonWorkStatusId;
            entity.SocialStatus = dto.SocialStatus;
            entity.SocialStatusApproved = dto.SocialStatusApproved;
            entity.SubmitterTypeId = dto.SubmitterTypeId;

            return entity;
        }

        public static Domain.Entities.Application Map(ApplicationUpdateDto dto, Domain.Entities.Application entity)
        {
            entity.ApplicationStatusHistory = dto.ApplicationStatusHistory;
            entity.ContactPersonId = dto.ContactPersonId;
            entity.Notes = dto.Notes;
            entity.ResourceSubTypeId = dto.ResourceSubTypeId;
            entity.ResourceTargetPersonClassGrade = dto.ResourceTargetPersonClassGrade;
            entity.ResourceTargetPersonClassParallel = dto.ResourceTargetPersonClassParallel;
            entity.ResourceTargetPersonEducationalProgram = dto.ResourceTargetPersonEducationalProgram;
            entity.ResourceTargetPersonEducationalStatusId = dto.ResourceTargetPersonEducationalStatusId;
            entity.ResourceTargetPersonEducationalSubStatusId = dto.ResourceTargetPersonEducationalSubStatusId;
            entity.ResourceTargetPersonGroup = dto.ResourceTargetPersonGroup;
            entity.ResourceTargetPersonId = dto.ResourceTargetPersonId;
            entity.ResourceTargetPersonTypeId = dto.ResourceTargetPersonTypeId;
            entity.ResourceTargetPersonWorkStatusId = dto.ResourceTargetPersonWorkStatusId;
            entity.SocialStatus = dto.SocialStatus;
            entity.SocialStatusApproved = dto.SocialStatusApproved;
            entity.SubmitterTypeId = dto.SubmitterTypeId;

            return entity;
        }
    }
}
