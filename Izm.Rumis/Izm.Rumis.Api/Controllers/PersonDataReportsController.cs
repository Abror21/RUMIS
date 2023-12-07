using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.PersonDataReportView)]
    public sealed class PersonDataReportsController : ApiController
    {
        private readonly IPersonDataReportService personDataReportService;

        public PersonDataReportsController(IPersonDataReportService personDataReportService)
        {
            this.personDataReportService = personDataReportService;
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<PersonDataReportListItemResponse>>> Generate(PersonDataReportGenerateRequest model, CancellationToken cancellationToken = default)
        {
            if (model.DateFrom != null && model.DateTo != null && model.DateFrom >= model.DateTo)
                throw new InvalidOperationException(Error.InvalidDateRange);

            var query = await personDataReportService.GenerateAsync(PersonDataReportMapper.Map(model, new PersonDataReportGenerateDto()), cancellationToken);

            if (model.DateFrom != null)
                query = query.Where(t => t.Created >= model.DateFrom);

            if (model.DateTo != null)
                query = query.Where(t => t.Created <= model.DateTo);

            return await query
                .ListAsync(map: PersonDataReportMapper.Project(), cancellationToken: cancellationToken);
        }

        public static class Error
        {
            public const string InvalidDateRange = "personDataReport.invalidDateRange";
        }
    }
}
