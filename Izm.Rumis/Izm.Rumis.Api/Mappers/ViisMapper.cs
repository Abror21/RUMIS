using Izm.Rumis.Api.Models;
using Izm.Rumis.Infrastructure.Viis.Models;
using System;
using System.Linq;

namespace Izm.Rumis.Api.Mappers
{
    internal static class ViisMapper
    {
        public static Func<RelatedPersonData.Student, ViisRelatedPersonResponse> Project()
        {
            return t => new ViisRelatedPersonResponse
            {
                BirthDate = t.BirthDate.ToString(),
                FirstName = t.Name,
                LastName = t.Surname,
                PrivatePersonalIdentifier = t.PersonCode,
                ActiveEducationData = t.Institution == null ? null : t.Institution.Select(inst => new ViisRelatedPersonResponse.ActiveEducationDataResponse
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

        public static Func<EmployeeData.Employee, ViisEmployeeDataResponse> ProjectEmployee()
        {
            return t => new ViisEmployeeDataResponse
            {
                FirstName = t.Name,
                LastName = t.Surname,
                PrivatePersonalIdentifier = t.PersonCode,
                ActiveWorkData = t.Institution == null ? null : t.Institution.Select(inst => new ViisEmployeeDataResponse.ActiveWorkDataResponse
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
