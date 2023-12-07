using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class PersonDataReportServiceTests
    {
        private PersonDataReportGenerateDto personDataReportGenerateDto { get; } = new PersonDataReportGenerateDto()
        {
            Notes = "someNotes",
            DataHandlerPrivatePersonalIdentifier = "00000000002",
            DataOwnerPrivatePersonalIdentifier = "00000000001",
            ReasonId = Guid.NewGuid()
        };

        [Fact]
        public async Task GenerateAsync_CreateEntityAndAudit_DataOwnerAndDataHandlerFilter()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            await SeedClassifiersAsync(db, personDataReportGenerateDto.ReasonId, Guid.NewGuid());

            var currentUserProfileService = ServiceFactory.CreateCurrentUserProfileService();
            currentUserProfileService.Id = Guid.NewGuid();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                currentUserProfileService: currentUserProfileService,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await service.GenerateAsync(personDataReportGenerateDto);

            var personDataReport = db.PersonDataReports.First();

            // Assert
            Assert.Single(db.PersonDataReports);
            Assert.Equal(currentUserProfileService.Id, personDataReport.UserProfileId);
            Assert.Equal(personDataReportGenerateDto.Notes, personDataReport.Notes);
            Assert.Equal(personDataReportGenerateDto.DataHandlerPrivatePersonalIdentifier, personDataReport.DataHandlerPrivatePersonalIdentifier);
            Assert.Equal(personDataReportGenerateDto.DataOwnerPrivatePersonalIdentifier, personDataReport.DataOwnerPrivatePersonalIdentifier);
            Assert.Equal(personDataReportGenerateDto.ReasonId, personDataReport.ReasonId);
            Assert.NotNull(gdprAuditService.TraceRangeAsyncCalledWith);
        }

        [Fact]
        public async Task GenerateAsync_CreateEntityAndAudit_DataHandlerFilter()
        {
            // Assign
            personDataReportGenerateDto.DataOwnerPrivatePersonalIdentifier = null;

            using var db = ServiceFactory.ConnectDb();

            await SeedClassifiersAsync(db, personDataReportGenerateDto.ReasonId, Guid.NewGuid());

            var currentUserProfileService = ServiceFactory.CreateCurrentUserProfileService();
            currentUserProfileService.Id = Guid.NewGuid();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                currentUserProfileService: currentUserProfileService,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await service.GenerateAsync(personDataReportGenerateDto);

            var personDataReport = db.PersonDataReports.First();

            // Assert
            Assert.Single(db.PersonDataReports);
            Assert.Equal(currentUserProfileService.Id, personDataReport.UserProfileId);
            Assert.Equal(personDataReportGenerateDto.Notes, personDataReport.Notes);
            Assert.Equal(personDataReportGenerateDto.DataHandlerPrivatePersonalIdentifier, personDataReport.DataHandlerPrivatePersonalIdentifier);
            Assert.Null(personDataReport.DataOwnerPrivatePersonalIdentifier);
            Assert.Equal(personDataReportGenerateDto.ReasonId, personDataReport.ReasonId);
            Assert.NotNull(gdprAuditService.TraceRangeAsyncCalledWith);
        }

        [Fact]
        public async Task GenerateAsync_CreateEntityAndAudit_DataOwnerFilter()
        {
            // Assign
            personDataReportGenerateDto.DataHandlerPrivatePersonalIdentifier = null;

            using var db = ServiceFactory.ConnectDb();

            await SeedClassifiersAsync(db, personDataReportGenerateDto.ReasonId, Guid.NewGuid());

            var currentUserProfileService = ServiceFactory.CreateCurrentUserProfileService();
            currentUserProfileService.Id = Guid.NewGuid();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                currentUserProfileService: currentUserProfileService,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await service.GenerateAsync(personDataReportGenerateDto);

            var personDataReport = db.PersonDataReports.First();

            // Assert
            Assert.Single(db.PersonDataReports);
            Assert.Equal(currentUserProfileService.Id, personDataReport.UserProfileId);
            Assert.Equal(personDataReportGenerateDto.Notes, personDataReport.Notes);
            Assert.Equal(personDataReportGenerateDto.DataOwnerPrivatePersonalIdentifier, personDataReport.DataOwnerPrivatePersonalIdentifier);
            Assert.Equal(personDataReportGenerateDto.ReasonId, personDataReport.ReasonId);
            Assert.NotNull(gdprAuditService.TraceRangeAsyncCalledWith);
        }

        [Theory]
        [InlineData(UserProfileType.Country, null, null, 3)]
        [InlineData(UserProfileType.Supervisor, 1, null, 2)]
        [InlineData(UserProfileType.EducationalInstitution, null, 1, 1)]
        public async Task GenerateAsync_ReturnsData_CountryProfile(UserProfileType profileType, int? supervisorId, int? educationalInstitutionId, int expectedCount)
        {
            // Assign
            var educationalInstitutionStatusId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            await SeedClassifiersAsync(db, personDataReportGenerateDto.ReasonId, educationalInstitutionStatusId);


            db.Supervisors.AddRange(new Supervisor[]
            {
                new Supervisor { Id = 1, Code = "1", Name = "1" },
                new Supervisor { Id = 2, Code = "2", Name = "2" }
            });

            db.EducationalInstitutions.AddRange(new EducationalInstitution[]
            {
                new EducationalInstitution { Id = 1, Code = "1", Name = "1", SupervisorId = 1, StatusId = educationalInstitutionStatusId },
                new EducationalInstitution { Id = 2, Code = "2", Name = "2", SupervisorId = 1, StatusId = educationalInstitutionStatusId },
                new EducationalInstitution { Id = 3, Code = "3", Name = "3", SupervisorId = 2, StatusId = educationalInstitutionStatusId }
            });

            db.GdprAudits.Add(new GdprAudit
            {
                Action = "some.action",
                DataHandlerPrivatePersonalIdentifier = personDataReportGenerateDto.DataHandlerPrivatePersonalIdentifier,
                DataOwnerPrivatePersonalIdentifier = personDataReportGenerateDto.DataOwnerPrivatePersonalIdentifier,
                User = User.Create(),
                SupervisorId = 2
            });

            db.GdprAudits.Add(new GdprAudit
            {
                Action = "some.action",
                DataHandlerPrivatePersonalIdentifier = personDataReportGenerateDto.DataHandlerPrivatePersonalIdentifier,
                DataOwnerPrivatePersonalIdentifier = personDataReportGenerateDto.DataOwnerPrivatePersonalIdentifier,
                User = User.Create(),
                EducationalInstitutionId = 1
            });

            db.GdprAudits.Add(new GdprAudit
            {
                Action = "some.action",
                DataHandlerPrivatePersonalIdentifier = personDataReportGenerateDto.DataHandlerPrivatePersonalIdentifier,
                DataOwnerPrivatePersonalIdentifier = personDataReportGenerateDto.DataOwnerPrivatePersonalIdentifier,
                User = User.Create(),
                EducationalInstitutionId = 2
            });


            var currentUserProfileService = ServiceFactory.CreateCurrentUserProfileService();
            currentUserProfileService.Id = Guid.NewGuid();
            currentUserProfileService.EducationalInstitutionId = educationalInstitutionId;
            currentUserProfileService.SupervisorId = supervisorId;
            currentUserProfileService.Type = profileType;

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                currentUserProfileService: currentUserProfileService,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await service.GenerateAsync(personDataReportGenerateDto);

            // Assert
            Assert.Equal(expectedCount, result.Count());
        }

        [Fact]
        public async Task GenerateAsync_ValidatorCalled()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            await SeedClassifiersAsync(db, personDataReportGenerateDto.ReasonId, Guid.NewGuid());

            var validator = ServiceFactory.CreatePersonDataReportValidator();

            var service = GetService(
                db: db,
                validator: validator
                );

            // Act
            var result = await service.GenerateAsync(personDataReportGenerateDto);

            var personDataReport = db.PersonDataReports.First();

            // Assert
            Assert.Equal(validator.ValidateCalledWith, personDataReportGenerateDto);
        }

        private PersonDataReportService GetService(
            IAppDbContext db,
            ICurrentUserProfileService currentUserProfileService = null,
            IGdprAuditService gdprAuditService = null,
            IPersonDataReportValidator validator = null)
        {
            return new PersonDataReportService(
                db: db,
                currentUserProfileService: currentUserProfileService ?? ServiceFactory.CreateCurrentUserProfileService(),
                gdprAuditService: gdprAuditService ?? ServiceFactory.CreateGdprAuditService(),
                validator: validator ?? ServiceFactory.CreatePersonDataReportValidator()
                );
        }

        private Task SeedClassifiersAsync(IAppDbContext db, Guid reasonId, Guid educationalInstitutionStatusId)
        {
            db.Classifiers.Add(new Classifier
            {
                Id = reasonId,
                Code = PersonalDataSpecialistReasonForRequest.DataBreachIncidents,
                Type = ClassifierTypes.PersonalDataSpecialistReasonForRequest,
                Value = "someValue"
            });

            db.Classifiers.Add(new Classifier
            {
                Id = educationalInstitutionStatusId,
                Code = EducationalInstitutionStatus.Disabled,
                Type = ClassifierTypes.EducationalInstitutionStatus,
                Value = "someValue"
            });

            return db.SaveChangesAsync();
        }
    }
}
