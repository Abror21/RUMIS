using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class PersonServiceTests
    {
        [Fact]
        public async Task CreateAsync_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var model = new PersonCreateDto
            {
                FirstName = "firstName",
                LastName = "lastName",
                IsUser = false,
                PrivatePersonalIdentifier = "00000000000"
            };

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await service.CreateAsync(model);

            // Assert
            Assert.True(db.Persons.Any());
            Assert.True(db.PersonTechnicals.Any());
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateAsync_CheckData(bool isUser)
        {
            // Assign
            var now = DateTime.Now;
            using var db = ServiceFactory.ConnectDb();

            var model = new PersonCreateDto
            {
                FirstName = "firstName",
                LastName = "lastName",
                IsUser = isUser,
                PrivatePersonalIdentifier = "00000000000"
            };

            var service = GetService(db);

            // Act
            var result = await service.CreateAsync(model);

            var person = db.Persons
                .Include(t => t.PersonTechnical)
                .First();

            // Assert
            Assert.True(person.ActiveFrom > now);
            Assert.Equal(model.FirstName, person.FirstName);
            Assert.Equal(model.LastName, person.LastName);
            Assert.Equal(model.PrivatePersonalIdentifier, person.PrivatePersonalIdentifier);
            Assert.Equal(model.IsUser, person.PersonTechnical.User != null);
        }

        [Fact]
        public async Task CreateAsync_Throws_AlreadyExists()
        {
            // Assign
            const string privatePersonalIdentifier = "00000000000";

            using var db = ServiceFactory.ConnectDb();

            db.Persons.Add(new Person
            {
                PrivatePersonalIdentifier = privatePersonalIdentifier
            });

            await db.SaveChangesAsync();

            var dto = new PersonCreateDto
            {
                PrivatePersonalIdentifier = privatePersonalIdentifier
            };

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));

            // Assert
            Assert.Equal(PersonService.Error.AlreadyExists, result.Message);
        }

        [Fact]
        public async Task EnsureUserAsync_Succeeds_NoUser()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Persons.Add(new Person
            {
                Id = id,
                PersonTechnical = new PersonTechnical(),
                PrivatePersonalIdentifier = "00000000000"
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var userId = await service.EnsureUserAsync(id);

            // Assert
            Assert.True(true);
            Assert.NotNull(userId);
        }


        [Fact]
        public async Task EnsureUserAsync_Succeeds_HasUser()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Persons.Add(new Person
            {
                Id = id,
                PersonTechnical = new PersonTechnical
                {
                    User = User.Create()
                },
                PrivatePersonalIdentifier = "00000000000"
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var userId = await service.EnsureUserAsync(id);

            // Assert
            Assert.True(true);
            Assert.NotNull(userId);
        }

        [Fact]
        public async Task Get_Succeeds()
        {
            // Assign
            var personTechnicalId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            await db.PersonTechnicals.AddAsync(new PersonTechnical
            {
                Id = personTechnicalId
            });

            var persons = new List<Person>()
            {
                new Person
                {
                    PrivatePersonalIdentifier = "someValue",
                    PersonTechnicalId = personTechnicalId
                },
                new Person
                {
                    PrivatePersonalIdentifier = "someValue",
                    PersonTechnicalId = personTechnicalId
                }
            };

            await db.Persons.AddRangeAsync(persons);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var result = service.Get();

            // Assert
            Assert.Equal(persons.Count, result.Count());
        }

        [Fact]
        public async Task UpdateAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();
            var phoneNumberId = Guid.NewGuid();
            var emailId = Guid.NewGuid();
            var dto = new PersonUpdateDto
            {
                PrivatePersonalIdentifier = "TestIdentifier",
                ContactInformation = new List<PersonUpdateDto.ContactData>
                {
                    new PersonUpdateDto.ContactData
                    {
                        TypeId = phoneNumberId,
                        Value = "SomePhoneNumber"
                    },
                    new PersonUpdateDto.ContactData
                    {
                        TypeId = emailId,
                        Value = "SomeEmail"
                    }
                }
            };

            using var db = ServiceFactory.ConnectDb();

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = phoneNumberId,
                Type = ClassifierTypes.ContactType,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = emailId,
                Type = ClassifierTypes.ContactType,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.PersonTechnicals.AddAsync(new PersonTechnical
            {
                Id = id
            });

            await db.SaveChangesAsync();

            await db.Persons.AddAsync(new Person
            {
                Id = Guid.NewGuid(),
                PrivatePersonalIdentifier = string.Empty,
                ActiveFrom = DateTime.UtcNow,
                PersonTechnicalId = id
            });

            await db.PersonContacts.AddAsync(new PersonContact
            {
                Id = Guid.NewGuid(),
                ContactValue = string.Empty,
                IsActive = true,
                ContactTypeId = phoneNumberId,
                PersonTechnicalId = id
            });
            await db.PersonContacts.AddAsync(new PersonContact
            {
                Id = Guid.NewGuid(),
                ContactValue = string.Empty,
                IsActive = true,
                ContactTypeId = emailId,
                PersonTechnicalId = id
            });

            await db.SaveChangesAsync();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            // Act
            await service.UpdateAsync(id, dto);

            var persons = db.Persons;

            var personContacts = db.PersonContacts;

            // Assert
            Assert.Equal(2, persons.Count());
            Assert.Equal(4, personContacts.Count());
            Assert.Equal(persons.LastOrDefault().PrivatePersonalIdentifier, dto.PrivatePersonalIdentifier);
            Assert.Equal(personContacts.LastOrDefault().ContactValue, dto.ContactInformation.FirstOrDefault(t => t.TypeId == personContacts.LastOrDefault().ContactTypeId).Value);
            Assert.True(personContacts.LastOrDefault().IsActive);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task UpdateAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.UpdateAsync(Guid.NewGuid(), new PersonUpdateDto()));
        }

        private PersonService GetService(IAppDbContext db, IPersonValidator validator = null, IGdprAuditService gdprAuditService = null)
        {
            return new PersonService(
                db: db,
                validator: validator ?? ServiceFactory.CreatePersonValidator(),
                gdprAuditService: gdprAuditService ?? ServiceFactory.CreateGdprAuditService()
                );
        }
    }
}
