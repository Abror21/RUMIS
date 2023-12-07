using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class GdprAuditServiceTests
    {
        private readonly GdprAuditTraceDto dto = GetDto();

        private readonly IEnumerable<GdprAuditTraceDto> dtoRange = new GdprAuditTraceDto[]
        {
            GetDto(),
            GetDto()
        };

        [Fact]
        public async Task TraceAsync_CheckData()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act
            await service.TraceAsync(dto);

            var gdprAudit = db.GdprAudits.First();

            var dtoData = dto.Data.First();
            var gdprAuditData = gdprAudit.Data.First();

            // Assert
            Assert.Equal(dto.Action, gdprAudit.Action);
            Assert.Equal(dto.ActionData, gdprAudit.ActionData);
            Assert.Equal(dto.DataOwnerPrivatePersonalIdentifier, gdprAudit.DataOwnerPrivatePersonalIdentifier);
            Assert.Equal(dto.DataOwnerId, gdprAudit.DataOwnerId);
            Assert.Equal(dto.EducationalInstitutionId, gdprAudit.EducationalInstitutionId);

            Assert.Equal(dto.Data.Count(), gdprAudit.Data.Count());
            Assert.Equal(dtoData.Type, gdprAuditData.Type);
            Assert.Equal(dtoData.Value, gdprAuditData.Value);
        }

        [Fact]
        public async Task TraceAsync_DataHandlerIdSetFromCurrentUser()
        {
            // Assign
            dto.EducationalInstitutionId = null;

            using var db = ServiceFactory.ConnectDb();

            var currentUser = ServiceFactory.CreateCurrentUserService();
            currentUser.PersonId = Guid.NewGuid();

            db.Persons.Add(new Person
            {
                PrivatePersonalIdentifier = "00000000000",
                PersonTechnicalId = currentUser.PersonId.Value
            });

            await db.SaveChangesAsync();

            var service = GetService(
                db: db,
                currentUserService: currentUser);

            // Act
            await service.TraceAsync(dto);

            var gdprAudit = db.GdprAudits.First();

            // Assert
            Assert.Equal(currentUser.PersonId, gdprAudit.DataHandlerId);
        }

        [Fact]
        public async Task TraceAsync_DataOwnerPrivatePersonalIdnetifierSetFromDataOwnerId()
        {
            // Assign
            dto.DataOwnerPrivatePersonalIdentifier = null;

            using var db = ServiceFactory.ConnectDb();

            await SeedDataAsync(db);

            var service = GetService(db);

            // Act
            await service.TraceAsync(dto);

            var gdprAudit = db.GdprAudits.First();

            var dataOwnerPerson = db.Persons
                .Where(t => t.PersonTechnicalId == dto.DataOwnerId)
                .First();

            // Assert
            Assert.Equal(dataOwnerPerson.PrivatePersonalIdentifier, gdprAudit.DataOwnerPrivatePersonalIdentifier);
        }

        [Fact]
        public async Task TraceAsync_EducationalInstitutionIdSetFromCurrentUserProfile()
        {
            // Assign
            dto.EducationalInstitutionId = null;

            using var db = ServiceFactory.ConnectDb();

            var currentUserProfile = ServiceFactory.CreateCurrentUserProfileService();
            currentUserProfile.IsInitialized = true;
            currentUserProfile.EducationalInstitutionId = 1;

            var service = GetService(
                db: db,
                currentUserProfileService: currentUserProfile);

            // Act
            await service.TraceAsync(dto);

            var gdprAudit = db.GdprAudits.First();

            // Assert
            Assert.Equal(currentUserProfile.EducationalInstitutionId, gdprAudit.EducationalInstitutionId);
        }

        [Fact]
        public async Task TraceAsync_EntityCreated()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act
            await service.TraceAsync(dto);

            // Assert
            Assert.Single(db.GdprAudits);
        }

        [Fact]
        public async Task TraceAsync_SupervisorIdSetFromCurrentUserProfile()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var currentUserProfile = ServiceFactory.CreateCurrentUserProfileService();
            currentUserProfile.IsInitialized = true;
            currentUserProfile.SupervisorId = 1;

            var service = GetService(
                db: db,
                currentUserProfileService: currentUserProfile);

            // Act
            await service.TraceAsync(dto);

            var gdprAudit = db.GdprAudits.First();

            // Assert
            Assert.Equal(currentUserProfile.SupervisorId, gdprAudit.SupervisorId);
        }

        [Fact]
        public async Task TraceAsync_UserIdSetFromCurrentUser()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var currentUser = ServiceFactory.CreateCurrentUserService();

            var service = GetService(
                db: db,
                currentUserService: currentUser);

            // Act
            await service.TraceAsync(dto);

            var gdprAudit = db.GdprAudits.First();

            // Assert
            Assert.Equal(currentUser.Id, gdprAudit.UserId);
        }

        [Fact]
        public async Task TraceAsync_UserProfileIdSetFromCurrentUserProfile()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var currentUserProfile = ServiceFactory.CreateCurrentUserProfileService();
            currentUserProfile.IsInitialized = true;

            var service = GetService(
                db: db,
                currentUserProfileService: currentUserProfile);

            // Act
            await service.TraceAsync(dto);

            var gdprAudit = db.GdprAudits.First();

            // Assert
            Assert.Equal(currentUserProfile.Id, gdprAudit.UserProfileId);
        }

        [Fact]
        public async Task TraceAsync_ValidatorCalled()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var validator = ServiceFactory.CreateGdprAuditValidator();

            var service = GetService(
                db: db,
                validator: validator);

            // Act
            await service.TraceAsync(dto);

            // Assert
            Assert.Equal(dto, validator.ValidateCalledWith);
        }

        [Fact]
        public async Task TraceAsync_EntitiesCreated()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act
            await service.TraceRangeAsync(dtoRange);

            // Assert
            Assert.Equal(dtoRange.Count(), db.GdprAudits.Count());
        }

        private static GdprAuditTraceDto GetDto()
        {
            return new GdprAuditTraceDto()
            {
                Action = "some.test",
                ActionData = "seomData",
                DataOwnerPrivatePersonalIdentifier = "00000000000",
                DataOwnerId = Guid.NewGuid(),
                EducationalInstitutionId = 1,
                Data = new[]
                {
                    new PersonDataProperty { Type = PersonDataType.BirthDate, Value = DateTime.Now.ToString() }
                }
            };
        }

        private static GdprAuditService GetService(
            IAppDbContext db,
            IGdprAuditValidator validator = null,
            ICurrentUserService currentUserService = null,
            ICurrentUserProfileService currentUserProfileService = null,
            IDistributedCache distributedCache = null)
        {
            return new GdprAuditService(
                db: db,
                validator: validator ?? ServiceFactory.CreateGdprAuditValidator(),
                currentUser: currentUserService ?? ServiceFactory.CreateCurrentUserService(),
                currentUserProfile: currentUserProfileService ?? ServiceFactory.CreateCurrentUserProfileService(),
                distributedCache: distributedCache ?? ServiceFactory.CreateDistributedCache());
        }

        private async Task SeedDataAsync(IAppDbContext db)
        {
            var dataOwnerPerson = new Person
            {
                FirstName = "someName",
                LastName = "someLastName",
                PersonTechnical = new PersonTechnical
                {
                    Id = dto.DataOwnerId ?? Guid.NewGuid()
                },
                PrivatePersonalIdentifier = "00000000001"
            };

            db.Persons.Add(dataOwnerPerson);

            await db.SaveChangesAsync();
        }
    }
}
