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
using Izm.Rumis.Infrastructure;
using Izm.Rumis.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.UserView)]
    public class UsersController : ApiController
    {
        private readonly IUserService userService;
        private readonly IGdprAuditService gdprAuditService;

        public UsersController(IUserService userService, IGdprAuditService gdprAuditService)
        {
            this.userService = userService;
            this.gdprAuditService = gdprAuditService;
        }

        /// Development only.
        [HttpPost]
        [PermissionAuthorize(Permission.UserEdit)]
        public async Task<ActionResult<Guid>> CreateTestPersonAndUser(
            [FromBody] string userName,
            [FromServices] IHostEnvironment env,
            [FromServices] IConfiguration config,
            [FromServices] AppDbContext db,
            CancellationToken cancellationToken = default
            )
        {
            if (env.IsProduction())
                return NotFound();

            if (await db.Users.AnyAsync(t => t.Name == userName, cancellationToken))
                return BadRequest($"User:{userName} already exists!");

            var entity = new Person
            {
                ActiveFrom = DateTime.Now,
                FirstName = userName,
                LastName = userName,
                PrivatePersonalIdentifier = "00000000000",
                PersonTechnical = new PersonTechnical()
            };

            entity.PersonTechnical.CreateUser();
            entity.PersonTechnical.User.Name = userName;

            db.Persons.Add(entity);

            var passwordHasher = new PasswordHasher<IdentityUserLogin>();

            var login = new IdentityUserLogin
            {
                AuthType = UserAuthType.Forms,
                UserName = userName
            };

            login.PasswordHash = passwordHasher.HashPassword(
                user: login,
                password: Environment.GetEnvironmentVariable(EnvironmentVariable.AdminPassword)
                    ?? config.GetValue<string>("Auth:AdminPassword")
                );

            var identityUser = new Infrastructure.Identity.IdentityUser
            {
                User = entity.PersonTechnical.User,
                Logins = new List<IdentityUserLogin>
                {
                    login
                }
            };

            entity.PersonTechnical.User.Events.Clear();

            await db.SaveChangesAsync(cancellationToken);

            return entity.PersonTechnical.UserId;
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(Permission.UserView)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            await userService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }

        [HttpGet("persons")]
        [PermissionAuthorize(Permission.UserView)]
        public async Task<ActionResult<PagedListModel<UserPersonListItemResponse>>> GetPersons(
            [FromQuery] PagingRequest paging = null,
            [FromQuery] UserListItemFilterRequest filter = null,
            CancellationToken cancellationToken = default
            )
        {
            var pagingParams = new PagingParams<User>(paging)
                .AddDefaultSorting(t => t.PersonTechnical.Persons.OrderBy(t => t.Created).Last().LastName)
                .AddSorting("FirstName", t => t.PersonTechnical.Persons.OrderBy(t => t.Created).Last().FirstName)
                .AddSorting("LastName", t => t.PersonTechnical.Persons.OrderBy(t => t.Created).Last().LastName);

            var query = userService.GetPersons()
                .Filter(filter);

            var total = await query.CountAsync(cancellationToken);

            var data = await query.Paging(pagingParams)
                .ListAsync(map: UserMapper.ProjectPersonListItem(), cancellationToken: cancellationToken);

            await gdprAuditService.TraceRangeAsync(data.Select(GdprAuditHelper.ProjectTraces()).ToArray(), cancellationToken);

            var model = new PagedListModel<UserPersonListItemResponse>
            {
                Items = data,
                Page = pagingParams.Page,
                PageSize = pagingParams.PageSize,
                Total = total
            };

            return model;
        }

        public static class GdprAuditHelper
        {
            public static Func<UserPersonListItemResponse, GdprAuditTraceDto> ProjectTraces()
            {
                return response =>
                {
                    var result = new GdprAuditTraceDto
                    {
                        Action = "user.getPersons",
                        ActionData = JsonSerializer.Serialize(new { UserId = response.Id }),
                        DataOwnerId = response.PersonTechnicalId
                    };

                    var data = new List<PersonDataProperty>();

                    foreach (var person in response.Persons)
                    {
                        data.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                        data.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                        data.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                    }

                    result.Data = data.Where(t => t.Value != null)
                        .ToArray();

                    return result;
                };
            }
        }
    }
}
