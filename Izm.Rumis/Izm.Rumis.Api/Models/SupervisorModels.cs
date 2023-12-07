using Izm.Rumis.Api.Common;
using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Models
{
    public class SupervisorCreateRequest
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
    }

    public class SupervisorListItemFilterRequest : Filter<Supervisor>
    {
        public IEnumerable<int> SupervisorIds { get; set; }
        public bool? SupervisorIsActive { get; set; }

        protected override Expression<Func<Supervisor, bool>>[] GetFilters()
        {
            var result = new List<Expression<Func<Supervisor, bool>>>();

            if (SupervisorIds != null)
                result.Add(t => SupervisorIds.Contains(t.Id));

            if (SupervisorIsActive.HasValue)
                result.Add(t => t.IsActive == SupervisorIsActive);

            return result.ToArray();
        }
    }

    public class SupervisorByIdResponse
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public bool? IsActive { get; set; }
        public int ActiveResources { get; set; }
        public int ResourcesInUsePersonally { get; set; }
        public int ResourcesInUseEducationally { get; set; }
        public int Applications { get; set; }
        public int ApplicationsAccepted { get; set; }
        public int ApplicationsAwaitingResources { get; set; }
        public int ApplicationsPostponed { get; set; }
        public int EducationalInstitutions { get; set; }
        public int ActiveEducationalInstitutions { get; set; }
        public int CountryDocumentTemplates { get; set; }
        public int EducationalInstitutionsDocumentTemplates { get; set; }
        public int EducationalInstitutionsDocumentLinks { get; set; }
    }

    public class SupervisorListItemResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool? Status { get; set; }
        public int EducationalInstitutions { get; set; }
        public int ActiveEducationalInstitutions { get; set; }
    }

    public class SupervisorResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class SupervisorUpdateRequest
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        public bool? IsActive { get; set; }
    }
}
