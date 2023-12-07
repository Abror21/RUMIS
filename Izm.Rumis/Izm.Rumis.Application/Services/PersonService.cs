using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Mappers;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public sealed class PersonService : IPersonService
    {
        private readonly IAppDbContext db;
        private readonly IPersonValidator validator;
        private readonly IGdprAuditService gdprAuditService;

        public PersonService(IAppDbContext db, IPersonValidator validator, IGdprAuditService gdprAuditService)
        {
            this.db = db;
            this.validator = validator;
            this.gdprAuditService = gdprAuditService;
        }

        /// <inheritdoc/>
        public async Task<PersonCreateResponseDto> CreateAsync(PersonCreateDto item, CancellationToken cancellationToken = default)
        {
            await validator.ValidateAsync(item, cancellationToken);

            if (await db.Persons.AnyAsync(t => t.PrivatePersonalIdentifier == item.PrivatePersonalIdentifier, cancellationToken))
                throw new InvalidOperationException(Error.AlreadyExists);

            var entity = new Person
            {
                ActiveFrom = DateTime.Now,
                PersonTechnical = new PersonTechnical
                {
                    Id = Guid.NewGuid()
                }
            };

            Guid? userId = null;

            if (item.IsUser)
            {
                entity.PersonTechnical.CreateUser();
                userId = entity.PersonTechnical == null ? null : entity.PersonTechnical.UserId;
            }

            db.Persons.Add(PersonMapper.Map(item, entity));

            await gdprAuditService.TraceAsync(
                GdprAuditHelper.GenerateTraceForCreateOperation(entity.PersonTechnical.Id, item),
                cancellationToken
                );

            await db.SaveChangesAsync(cancellationToken);

            return new PersonCreateResponseDto { Id = entity.Id, UserId = userId };
        }

        /// <inheritdoc/>
        public async Task<Guid?> EnsureUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.Persons
                .Where(t => t.Id == id)
                .Select(t => t.PersonTechnical)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            if (entity.UserId != null)
                return entity.UserId;

            entity.CreateUser();

            await db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        /// <inheritdoc/>
        public SetQuery<Person> Get()
        {
            return new SetQuery<Person>(db.Persons.AsNoTracking());
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task UpdateAsync(Guid id, PersonUpdateDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.PersonTechnicals
                .Include(t => t.Persons)
                .Include(t => t.PersonContacts)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            var personEntity = entity.Persons.OrderByDescending(t => t.ActiveFrom)
                .FirstOrDefault();

            if (item.FirstName != personEntity.FirstName
                || item.LastName != personEntity.LastName
                || item.PrivatePersonalIdentifier != personEntity.PrivatePersonalIdentifier
                || item.BirthDate != personEntity.BirthDate)
            {
                var newPersonEntity = new Person
                {
                    ActiveFrom = DateTime.Now,
                    PersonTechnicalId = id
                };

                db.Persons.Add(PersonMapper.Map(item, newPersonEntity));
            }

            var personContactToBeNotActive = entity.PersonContacts.Where(t => item.ContactInformation.Any(n => n.Value != t.ContactValue
                                                                                                            && n.TypeId == t.ContactTypeId));

            foreach (var personContact in personContactToBeNotActive)
                personContact.IsActive = false;

            var personContactToUpdate = entity.PersonContacts.Where(t => item.ContactInformation.Any(n => n.TypeId == t.ContactTypeId
                                                                                                       && n.Value == t.ContactValue));

            foreach (var personContact in personContactToUpdate)
                personContact.IsActive = true;

            var personContactToAdd = item.ContactInformation.Where(t => !entity.PersonContacts.Any(n => n.ContactValue == t.Value
                                                                                                    && n.ContactTypeId == t.TypeId));

            foreach (var personContact in personContactToAdd)
                entity.PersonContacts.Add(new PersonContact
                {
                    ContactTypeId = personContact.TypeId,
                    ContactValue = personContact.Value,
                    IsActive = true
                });

            await gdprAuditService.TraceAsync(
                GdprAuditHelper.GenerateTraceForUpdateOperation(entity),
                cancellationToken
                );

            await db.SaveChangesAsync(cancellationToken);
        }

        public class Error
        {
            public const string AlreadyExists = "person.alreadyExists";
        }

        public static class GdprAuditHelper
        {
            public static GdprAuditTraceDto GenerateTraceForCreateOperation(Guid dataOwnerId, PersonCreateDto item)
            {
                return new GdprAuditTraceDto
                {
                    Action = "person.create",
                    ActionData = null,
                    DataOwnerId = dataOwnerId,
                    DataOwnerPrivatePersonalIdentifier = item.PrivatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.FirstName, Value = item.FirstName },
                        new PersonDataProperty { Type = PersonDataType.LastName, Value = item.LastName },
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = item.PrivatePersonalIdentifier }
                    }
                };
            }

            public static GdprAuditTraceDto GenerateTraceForUpdateOperation(PersonTechnical personTechnical)
            {
                var result = new GdprAuditTraceDto
                {
                    Action = "person.update",
                    ActionData = null,
                    DataOwnerId = personTechnical.Id,
                    DataOwnerPrivatePersonalIdentifier = null
                };

                var data = new List<PersonDataProperty>();

                foreach (var person in personTechnical.Persons)
                {
                    data.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                    data.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                    data.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                }

                foreach (var contact in personTechnical.PersonContacts)
                    data.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = contact.ContactValue });

                result.Data = data.Where(t => t.Value != null)
                    .Distinct()
                    .ToArray();

                return result;
            }
        }
    }
}
