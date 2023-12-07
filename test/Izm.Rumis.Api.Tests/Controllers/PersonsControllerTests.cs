using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public sealed class PersonsControllerTests
    {
        [Fact]
        public async Task Create_Succeeds_PersonDoesNotExist()
        {
            // Assign
            var requestModel = new PersonCreateRequest
            {
                FirstName = "FirstName",
                IsUser = true,
                LastName = "LastName",
                PrivatePersonalIdentifier = "00000000000"
            };

            using var db = ServiceFactory.ConnectDb();

            var personService = new PersonServiceFake();

            personService.Persons = db.Persons.AsQueryable();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var controller = GetController(
                personService: personService,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await controller.Create(requestModel);

            // Assert
            Assert.Equal(requestModel.FirstName, personService.CreateCalledWith.FirstName);
            Assert.Equal(requestModel.IsUser, personService.CreateCalledWith.IsUser);
            Assert.Equal(requestModel.LastName, personService.CreateCalledWith.LastName);
            Assert.Equal(requestModel.PrivatePersonalIdentifier, personService.CreateCalledWith.PrivatePersonalIdentifier);

            Assert.NotNull(result);

            Assert.NotEqual(result.Value.Id, Guid.Empty);
            Assert.NotNull(result.Value.UserId);

            Assert.Null(personService.EnsureUserCalledWith);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task Create_Succeeds_IsUser_PersonExists()
        {
            // Assign
            const string privatePersonalIdentifier = "00000000000";

            var requestModel = new PersonCreateRequest
            {
                FirstName = "FirstName",
                IsUser = true,
                LastName = "LastName",
                PrivatePersonalIdentifier = privatePersonalIdentifier
            };

            var personService = new PersonServiceFake();

            var person = new Person
            {
                Id = Guid.NewGuid(),
                PrivatePersonalIdentifier = privatePersonalIdentifier
            };

            using var db = ServiceFactory.ConnectDb();

            db.Persons.Add(person);

            await db.SaveChangesAsync();

            personService.Persons = db.Persons.AsQueryable();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var controller = GetController(
                personService: personService,
                gdprAuditService: gdprAuditService
                );

            // Act
            await controller.Create(requestModel);

            // Assert
            Assert.Equal(person.Id, personService.EnsureUserCalledWith);

            Assert.Null(personService.CreateCalledWith);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task Create_Succeeds_IsNotUser_PersonExists()
        {
            // Assign
            const string privatePersonalIdentifier = "00000000000";

            var requestModel = new PersonCreateRequest
            {
                FirstName = "FirstName",
                IsUser = false,
                LastName = "LastName",
                PrivatePersonalIdentifier = privatePersonalIdentifier
            };

            var personService = ServiceFactory.CreatePersonService();

            using var db = ServiceFactory.ConnectDb();

            db.Persons.Add(new Person
            {
                Id = Guid.NewGuid(),
                PrivatePersonalIdentifier = privatePersonalIdentifier
            });

            await db.SaveChangesAsync();

            personService.Persons = db.Persons.AsQueryable();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var controller = GetController(
                personService: personService,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await controller.Create(requestModel);

            // Assert
            Assert.Null(personService.EnsureUserCalledWith);

            Assert.NotNull(result);

            Assert.NotEqual(result.Value.Id, Guid.Empty);
            Assert.Null(result.Value.UserId);

            Assert.Null(personService.CreateCalledWith);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task GetByPrivatePersonalIdentifier_NoData()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var personService = new PersonServiceFake();

            var controller = GetController(personService);

            personService.Persons = db.Persons.AsQueryable();

            // Act
            var result = await controller.GetByPrivatePersonalIdentifier(string.Empty);

            // Assert
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetByPrivatePersonalIdentifier_Succeeds()
        {
            // Assign
            var personTechnicalId = Guid.NewGuid();
            var privatePersonalIdentifier = "SomeIdentifier";

            using var db = ServiceFactory.ConnectDb();

            var personService = new PersonServiceFake();


            await db.PersonTechnicals.AddAsync(new PersonTechnical
            {
                Id = personTechnicalId
            });

            await db.Persons.AddAsync(new Person
            {
                PrivatePersonalIdentifier = privatePersonalIdentifier,
                PersonTechnicalId = personTechnicalId
            });

            await db.SaveChangesAsync();

            personService.Persons = db.Persons.AsQueryable();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var controller = GetController(
                personService: personService,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await controller.GetByPrivatePersonalIdentifier(privatePersonalIdentifier);

            // Assert
            Assert.NotNull(result.Value);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task GetByUserId_NoData()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var personService = new PersonServiceFake();

            var controller = GetController(personService);

            personService.Persons = db.Persons.AsQueryable();

            // Act
            var result = await controller.GetByUserId(Guid.NewGuid());

            // Assert
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetByUserId_Succeeds()
        {
            // Assign
            var personTechnicalId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            var personService = new PersonServiceFake();


            await db.PersonTechnicals.AddAsync(new PersonTechnical
            {
                Id = personTechnicalId,
                UserId = userId
            });

            await db.Persons.AddAsync(new Person
            {
                PrivatePersonalIdentifier = string.Empty,
                PersonTechnicalId = personTechnicalId
            });

            await db.SaveChangesAsync();

            personService.Persons = db.Persons.AsQueryable();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var controller = GetController(
                personService: personService,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await controller.GetByUserId(userId);

            // Assert
            Assert.NotNull(result.Value);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            // Assign
            var personService = ServiceFactory.CreatePersonService();

            var controller = GetController(personService);

            // Act
            var result = await controller.Update(Guid.NewGuid(), new PersonUpdateRequest());

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        public PersonsController GetController(IPersonService personService = null, IGdprAuditService gdprAuditService = null)
        {
            return new PersonsController(
                personService: personService ?? ServiceFactory.CreatePersonService(),
                gdprAuditService: gdprAuditService ?? ServiceFactory.CreateGdprAuditService()
                );
        }
    }
}
