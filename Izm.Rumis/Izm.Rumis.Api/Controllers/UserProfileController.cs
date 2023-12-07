using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.UserProfileView)]
    public class UserProfileController : ApiController
    {
        private readonly IUserProfileService userProfileService;
        private readonly IGdprAuditService gdprAuditService;

        public UserProfileController(IUserProfileService userProfileService, IGdprAuditService gdprAuditService)
        {
            this.userProfileService = userProfileService;
            this.gdprAuditService = gdprAuditService;
        }

        [HttpPost]
        [PermissionAuthorize(Permission.UserProfileEdit)]
        public async Task<ActionResult<Guid>> Create(UserProfileEditRequest model, CancellationToken cancellationToken = default)
        {
            return await userProfileService.CreateAsync(UserProfileMapper.Map(model, new UserProfileEditDto()), cancellationToken);
        }

        [HttpGet]
        public async Task<ActionResult<PagedListModel<UserProfileListItemResponse>>> Get([FromQuery] PagingRequest paging = null, [FromQuery] UserProfileFilterRequest filter = null, CancellationToken cancellationToken = default)
        {
            var pagingParams = new PagingParams<UserProfile>(paging)
                .AddDefaultSorting(t => t.Created, SortDirection.Desc);

            var query = userProfileService.Get()
                .Filter(filter);

            var total = await query.CountAsync(cancellationToken);

            var data = await query.Paging(pagingParams)
                .ListAsync(map: UserProfileMapper.ProjectListItemIntermediate(), cancellationToken: cancellationToken);

            await gdprAuditService.TraceRangeAsync(data.Where(t => t.PersonTechnicalId != null).Select(GdprAuditHelper.ProjectTraces()).ToArray(), cancellationToken);

            var model = new PagedListModel<UserProfileListItemResponse>
            {
                Items = data.Select(UserProfileMapper.ProjectListItemFromIntermediate()),
                Total = total,
                Page = pagingParams.Page,
                PageSize = pagingParams.PageSize
            };

            return model;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfileResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var intermediate = await userProfileService.Get()
                .Where(t => t.Id == id)
                .FirstAsync(map: UserProfileMapper.ProjectIntermediate(), cancellationToken: cancellationToken);

            if (intermediate == null)
                return NotFound();

            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForGetByIdMethod(intermediate), cancellationToken);

            return UserProfileMapper.Map(intermediate, new UserProfileResponse());
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(Permission.UserProfileEdit)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            await userProfileService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.UserProfileEdit)]
        public async Task<ActionResult> Update(Guid id, UserProfileEditRequest model, CancellationToken cancellationToken = default)
        {
            await userProfileService.UpdateAsync(id, UserProfileMapper.Map(model, new UserProfileEditDto()), cancellationToken);

            return NoContent();
        }

        public static class GdprAuditHelper
        {
            public static GdprAuditTraceDto GenerateTraceForGetByIdMethod(UserProfileIntermediateResponse intermediate)
            {
                return new GdprAuditTraceDto
                {
                    Action = "userProfile.getById",
                    ActionData = JsonSerializer.Serialize(new { UserProfileId = intermediate.Id, UserId = intermediate.Id }),
                    DataOwnerId = intermediate.PersonTechnicalId,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.Contact, Value = intermediate.Email },
                        new PersonDataProperty { Type = PersonDataType.Contact, Value = intermediate.PhoneNumber }
                    }.Where(t => t.Value != null).ToArray(),
                    EducationalInstitutionId = intermediate.EducationalInstitution?.Id
                };
            }

            public static Func<UserProfileListItemIntermediateResponse, GdprAuditTraceDto> ProjectTraces()
            {
                return intermediate => new GdprAuditTraceDto
                {
                    Action = "userProfile.getList",
                    ActionData = JsonSerializer.Serialize(new { UserProfileId = intermediate.Id, UserId = intermediate.Id }),
                    DataOwnerId = intermediate.PersonTechnicalId,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.Contact, Value = intermediate.Email },
                        new PersonDataProperty { Type = PersonDataType.Contact, Value = intermediate.PhoneNumber }
                    }.Where(t => t.Value != null).ToArray(),
                    EducationalInstitutionId = intermediate.EducationalInstitution?.Id
                };
            }
        }
    }
}
