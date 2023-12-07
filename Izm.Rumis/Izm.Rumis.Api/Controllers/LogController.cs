using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Infrastructure.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.LogView)]
    public sealed class LogController : ApiController
    {
        private readonly ILogService service;
        private readonly ILogger<LogController> logger;

        public LogController(
            ILogService service,
            ILogger<LogController> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedListModel<LogListItemResponse>>> GetAsync([FromQuery] LogFilter filter, [FromQuery] PagingRequest paging = null, CancellationToken cancellationToken = default)
        {
            var pagingParams = new PagingParams<Log>(paging)
                .SetMaxPageSize(100)
                .AddDefaultSorting(t => t.Id, SortDirection.Desc)
                .AddSorting("Id", t => t.Id)
                .AddSorting("Date", t => t.Date)
                .AddSorting("Thread", t => t.Thread)
                .AddSorting("Level", t => t.Level)
                .AddSorting("Logger", t => t.Logger)
                .AddSorting("TraceId", t => t.TraceId)
                .AddSorting("Message", t => t.Message)
                .AddSorting("Exception", t => t.Exception)
                .AddSorting("UserName", t => t.UserName)
                .AddSorting("IpAddress", t => t.IpAddress)
                .AddSorting("UserAgent", t => t.UserAgent)
                .AddSorting("RequestUrl", t => t.RequestUrl)
                .AddSorting("RequestMethod", t => t.RequestMethod)
                .AddSorting("PersonTechnical", t => t.Person.Persons.OrderBy(t => t.Created).Last().FirstName + " " + t.Person.Persons.OrderBy(t => t.Created).Last().LastName)
                .AddSorting("SessionId", t => t.SessionId)
                .AddSorting("EducationalInstitution", t => t.EducationalInstitution.Name)
                .AddSorting("Supervisor", t => t.Supervisor.Name);

            var query = service.Get()
                .Filter(filter);

            var total = await query.CountAsync(cancellationToken);

            var data = await query.Paging(pagingParams)
                .ListAsync(map: LogMapper.ProjectListItem(), cancellationToken: cancellationToken);

            var model = new PagedListModel<LogListItemResponse>
            {
                Items = data,
                Total = total,
                Page = pagingParams.Page,
                PageSize = pagingParams.PageSize
            };

            return model;
        }

        [HttpPost]
        [PermissionAuthorize(Permission.LogEdit)]
        public IActionResult Create(LogCreateRequest model)
        {
            logger.Log(model.Level, model.Message);

            return NoContent();
        }
    }
}
