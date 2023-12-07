using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public class ApplicationAttachmentServiceTests
    {
        [Fact]
        public async Task Get_ReturnsData()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            await db.ApplicationAttachments.AddRangeAsync(
                new ApplicationAttachment { AttachmentNumber = string.Empty, AttachmentDate = new DateOnly(2020, 1, 1) },
                new ApplicationAttachment { AttachmentNumber = string.Empty, AttachmentDate = new DateOnly(2020, 1, 1) });

            await db.SaveChangesAsync();

            // Act
            var data = CreateService(db).Get().List();

            // Assert
            Assert.Equal(2, data.Count());
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var applicationId = Guid.NewGuid();

            await AddApplication(db, applicationId);

            AddParameters(db);

            // Act
            await CreateService(db).CreateAsync(new ApplicationAttachmentCreateDto
            {
                ApplicationId = applicationId,
                AttachmentNumber = "a",
                AttachmentDate = new DateOnly(2020, 1, 1),
                File = FakeFileDto("a.docx")
            });

            var exists = db.ApplicationAttachments.Any(t => t.ApplicationId == applicationId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task Create_Throws_NotFound()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            {
                return CreateService(db).CreateAsync(new ApplicationAttachmentCreateDto
                {
                    ApplicationId = Guid.NewGuid(),
                    AttachmentNumber = "a",
                    AttachmentDate = new DateOnly(2020, 1, 1),
                    File = FakeFileDto("a.docx")
                });
            });
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var applicationId = Guid.NewGuid();
            var applicationAttachmentId = Guid.NewGuid();
            var attachmentDate = new DateOnly(2023, 1, 1);
            const string attachmentNumber = "Number";

            await AddApplication(db, applicationId);

            AddParameters(db);

            await db.ApplicationAttachments.AddAsync(new ApplicationAttachment
            {
                Id = applicationAttachmentId,
                ApplicationId = applicationId,
                AttachmentDate = new DateOnly(2020, 1, 1),
                AttachmentNumber = string.Empty
            });

            await db.SaveChangesAsync();

            // Act
            await CreateService(db).UpdateAsync(applicationAttachmentId, new ApplicationAttachmentUpdateDto
            {
                AttachmentDate = attachmentDate,
                AttachmentNumber = attachmentNumber,
                File = FakeFileDto("a.docx")
            });

            var item = db.ApplicationAttachments.FirstOrDefault(t => t.Id == applicationAttachmentId);

            // Assert
            Assert.Equal(attachmentDate, item.AttachmentDate);
            Assert.Equal(attachmentNumber, item.AttachmentNumber);
        }

        [Fact]
        public async Task Update_NoFile_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var applicationId = Guid.NewGuid();
            var applicationAttachmentId = Guid.NewGuid();
            var attachmentDate = new DateOnly(2023, 1, 1);
            const string attachmentNumber = "Number";

            await AddApplication(db, applicationId);

            AddParameters(db);

            var fileId = Guid.NewGuid();
            var fileEnId = Guid.NewGuid();

            await db.ApplicationAttachments.AddAsync(new ApplicationAttachment
            {
                Id = applicationAttachmentId,
                ApplicationId = applicationId,
                AttachmentDate = new DateOnly(2020, 1, 1),
                AttachmentNumber = string.Empty,
                FileId = fileId
            });

            // Act
            await CreateService(db).UpdateAsync(applicationAttachmentId, new ApplicationAttachmentUpdateDto
            {
                AttachmentDate = attachmentDate,
                AttachmentNumber = attachmentNumber
            });

            var item = db.ApplicationAttachments.FirstOrDefault(t => t.Id == applicationAttachmentId);

            // Assert
            Assert.Equal(attachmentDate, item.AttachmentDate);
            Assert.Equal(attachmentNumber, item.AttachmentNumber);
            Assert.Equal(fileId, item.FileId);
        }

        [Fact]
        public async Task Update_Throws_NotFound()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            {
                return CreateService(db).UpdateAsync(Guid.NewGuid(), new ApplicationAttachmentUpdateDto());
            });
        }

        private ApplicationAttachmentService CreateService(AppDbContext db, IApplicationAttachmentValidator validator = null)
        {
            return new ApplicationAttachmentService(
                db,
                validator: validator ?? ServiceFactory.CreateApplicationAttachmentValidator(),
                new FileServiceFake());
        }

        private byte[] FakeContent()
        {
            return Encoding.UTF8.GetBytes("test");
        }

        private FileDto FakeFileDto(string filename)
        {
            return new FileDto
            {
                FileName = filename,
                Content = FakeContent(),
            };
        }

        private void AddParameters(AppDbContext db, int maxSize = 100000)
        {
            db.Parameters.AddRange(
                new Parameter { Code = ParameterCode.ApplicationAttachmentMaxSize, Value = maxSize.ToString() });

            db.SaveChanges();
        }

        private async Task AddApplication(AppDbContext db, Guid applicationId)
        {
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = applicationStatusId,
                Type = ClassifierTypes.ApplicationStatus,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = submitterTypeId,
                Type = ClassifierTypes.ApplicantRole,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceTargetPersonTypeId,
                Type = ClassifierTypes.ResourceTargetPersonType,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceSubTypeId,
                Type = ClassifierTypes.ResourceSubType,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            await db.Applications.AddAsync(new Domain.Entities.Application(applicationStatusId)
            {
                Id = applicationId,
                ApplicationNumber = Guid.NewGuid().ToString().Substring(0, 10),
                ApplicationDate = DateTime.UtcNow,
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonId = Guid.NewGuid()
            });

            await db.SaveChangesAsync();
        }
    }
}
