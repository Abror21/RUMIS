using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.UserProfileView)]
    public sealed class PersonsController : ApiController
    {
        private readonly IPersonService personService;
        private readonly IGdprAuditService gdprAuditService;

        public PersonsController(IPersonService personService, IGdprAuditService gdprAuditService)
        {
            this.personService = personService;
            this.gdprAuditService = gdprAuditService;
        }

        [HttpPost]
        [PermissionAuthorize(Permission.UserProfileEdit)]
        public async Task<ActionResult<PersonCreateResponse>> Create(PersonCreateRequest model, CancellationToken cancellationToken = default)
        {
            var personData = await personService.Get()
                .Where(t => t.PrivatePersonalIdentifier == model.PrivatePersonalIdentifier)
                .FirstAsync(map: t => new { t.Id, t.PersonTechnicalId }, cancellationToken: cancellationToken);

            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForCreateOperation(personData?.PersonTechnicalId, model.PrivatePersonalIdentifier), cancellationToken);

            if (personData == null)
                return PersonMapper.Map((await personService.CreateAsync(PersonMapper.Map(model, new PersonCreateDto()), cancellationToken)), new PersonCreateResponse());

            Guid? userId = null;

            if (model.IsUser)
                userId = await personService.EnsureUserAsync(personData.Id, cancellationToken);

            return new PersonCreateResponse { Id = personData.Id, UserId = userId };
        }


        [HttpGet("{privatePersonalIdentifier}")]
        public async Task<ActionResult<PersonResponse>> GetByPrivatePersonalIdentifier(string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            var data = await personService.Get()
                .Where(t => t.PrivatePersonalIdentifier == privatePersonalIdentifier)
                .OrderBy(t => t.ActiveFrom, SortDirection.Desc)
                .FirstAsync(PersonMapper.Project(), cancellationToken);

            if (data == null)
                return NotFound();

            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForGetByPrivatePersonalIdentifierOperation(data), cancellationToken);

            return data;
        }

        [HttpGet("getByUserId({userId})")]
        public async Task<ActionResult<PersonResponse>> GetByUserId(Guid userId, CancellationToken cancellationToken = default)
        {
            var data = await personService.Get()
                .Where(t => t.PersonTechnical.UserId == userId)
                .OrderBy(t => t.ActiveFrom, SortDirection.Desc)
                .FirstAsync(PersonMapper.Project(), cancellationToken);

            if (data == null)
                return NotFound();

            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForGetByUserIdOperation(userId, data), cancellationToken);

            return data;
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.ApplicationEdit)]
        public async Task<ActionResult> Update(Guid id, PersonUpdateRequest model, CancellationToken cancellationToken = default)
        {
            await personService.UpdateAsync(id, PersonMapper.Map(model, new PersonUpdateDto()), cancellationToken);

            return NoContent();
        }

        public static class GdprAuditHelper
        {
            public static GdprAuditTraceDto GenerateTraceForCreateOperation(Guid? dataOwnerId, string privatePersonalIdentifier)
            {
                return new GdprAuditTraceDto
                {
                    Action = "person.create",
                    ActionData = null,
                    DataOwnerId = dataOwnerId,
                    DataOwnerPrivatePersonalIdentifier = privatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = privatePersonalIdentifier }
                    }
                };
            }

            public static GdprAuditTraceDto GenerateTraceForGetByPrivatePersonalIdentifierOperation(PersonResponse personResponse)
            {
                var result = new GdprAuditTraceDto
                {
                    Action = "person.getByPrivatePersonalIdentifier",
                    ActionData = null,
                    DataOwnerId = personResponse.PersonTechnicalId,
                    DataOwnerPrivatePersonalIdentifier = null
                };

                var data = new List<PersonDataProperty>
                {
                    new PersonDataProperty { Type = PersonDataType.BirthDate, Value = personResponse.BirthDate.ToString() },
                    new PersonDataProperty { Type = PersonDataType.FirstName, Value = personResponse.FirstName },
                    new PersonDataProperty { Type = PersonDataType.LastName, Value = personResponse.LastName },
                    new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = personResponse.PrivatePersonalIdentifier }
                };

                foreach (var contact in personResponse.ContactInformation)
                    data.Add(new PersonDataProperty { Type = PersonDataType.Contact, Value = contact.Value });

                result.Data = data.Where(t => t.Value != null)
                    .ToArray();

                return result;
            }

            public static GdprAuditTraceDto GenerateTraceForGetByUserIdOperation(Guid userId, PersonResponse personResponse)
            {
                var result = new GdprAuditTraceDto
                {
                    Action = "person.getByUserId",
                    ActionData = JsonSerializer.Serialize(new { UserId = userId }),
                    DataOwnerId = personResponse.PersonTechnicalId,
                    DataOwnerPrivatePersonalIdentifier = null
                };

                var data = new List<PersonDataProperty>
                {
                    new PersonDataProperty { Type = PersonDataType.BirthDate, Value = personResponse.BirthDate.ToString() },
                    new PersonDataProperty { Type = PersonDataType.FirstName, Value = personResponse.FirstName },
                    new PersonDataProperty { Type = PersonDataType.LastName, Value = personResponse.LastName },
                    new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = personResponse.PrivatePersonalIdentifier }
                };

                foreach (var contact in personResponse.ContactInformation)
                    data.Add(new PersonDataProperty { Type = PersonDataType.Contact, Value = contact.Value });

                result.Data = data.Where(t => t.Value != null)
                    .ToArray();

                return result;
            }
        }
    }
}
