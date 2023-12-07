using Izm.Rumis.Api.Models;
using Izm.Rumis.Domain.Entities;
using System;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    public static class EServicesEducationInstitutionMapper
    {
        public static Expression<Func<EducationalInstitution, EServiceEducationalInstitutionData>> Project()
        {
            return t => new EServiceEducationalInstitutionData
            {
                Id = t.Id,
                Code = t.Code,
                Status = new EServiceEducationalInstitutionData.ClassifierData
                {
                    Id = t.Status.Id,
                    Code = t.Status.Code,
                    Value = t.Status.Value
                }
            };
        }
    }
}
