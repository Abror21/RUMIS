using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using System;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public class DocumentTemplateServiceTests
    {
        [Fact]
        public void Get_ReturnsData()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var resourceTypeId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = resourceTypeId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.ResourceType
            });

            db.SaveChanges();

            db.DocumentTemplates.AddRange(
                new DocumentTemplate { Code = string.Empty, Title = string.Empty, ResourceTypeId = resourceTypeId },
                new DocumentTemplate { Code = string.Empty, Title = string.Empty, ResourceTypeId = resourceTypeId });

            db.SaveChanges();

            var service = CreateService(db);

            // Act
            var data = service.Get().List();

            // Assert
            Assert.Equal(2, data.Count());
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const string code = "a";
            var permissionType = UserProfileType.Supervisor;
            var supervisorId = 1;

            var authorizationService = ServiceFactory.CreateAuthorizationService();

            var validator = ServiceFactory.CreateDocumentTemplateValidator();

            var service = CreateService(
                db,
                validator: validator,
                authorization: authorizationService);

            var resourceTypeId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = resourceTypeId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.ResourceType
            });

            db.SaveChanges();

            // Act
            await service.CreateAsync(new DocumentTemplateEditDto
            {
                Code = code,
                Title = "x",
                ResourceTypeId = resourceTypeId,
                SupervisorId = supervisorId,
                PermissionType = permissionType,
                File = FakeFileDto("a.docx")
            });

            var item = db.DocumentTemplates.FirstOrDefault(t => t.Code == code);

            // Assert
            Assert.NotNull(item);
            Assert.Null(item.EducationalInstitutionId);
            Assert.Equal(supervisorId, item.SupervisorId);
            Assert.Equal(UserProfileType.Supervisor, item.PermissionType);
            Assert.NotNull(validator.ValidateAsyncCalledWith);
            Assert.NotNull(validator.ValidateFileCalledWith);
        }

        [Fact]
        public async Task Create_Hyperlink_NoFile_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const string code = Domain.Constants.Classifiers.DocumentType.Hyperlink;
            var permissionType = UserProfileType.Supervisor;
            var supervisorId = 1;

            var authorizationService = ServiceFactory.CreateAuthorizationService();

            var validator = ServiceFactory.CreateDocumentTemplateValidator();

            var service = CreateService(
                db,
                validator: validator,
                authorization: authorizationService);

            var resourceTypeId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = resourceTypeId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.ResourceType
            });

            db.SaveChanges();

            // Act
            await service.CreateAsync(new DocumentTemplateEditDto
            {
                Code = code,
                Title = "x",
                ResourceTypeId = resourceTypeId,
                SupervisorId = supervisorId,
                PermissionType = permissionType
            });

            var item = db.DocumentTemplates.FirstOrDefault(t => t.Code == code);

            // Assert
            Assert.NotNull(item);
            Assert.Null(item.EducationalInstitutionId);
            Assert.Equal(supervisorId, item.SupervisorId);
            Assert.Equal(UserProfileType.Supervisor, item.PermissionType);
            Assert.NotNull(validator.ValidateAsyncCalledWith);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int id = 1;
            const string code = "a";

            var permissionType = UserProfileType.Supervisor;
            var supervisorId = 1;

            var resourceTypeId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = resourceTypeId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.ResourceType
            });

            db.SaveChanges();

            db.DocumentTemplates.Add(new DocumentTemplate
            {
                Id = id,
                Code = string.Empty,
                Title = string.Empty,
                ResourceTypeId = resourceTypeId
            });

            db.SaveChanges();

            var validator = ServiceFactory.CreateDocumentTemplateValidator();
            var authorization = ServiceFactory.CreateAuthorizationService();

            var service = CreateService(
                db,
                validator: validator,
                authorization: authorization);

            // Act
            await service.UpdateAsync(id, new DocumentTemplateEditDto
            {
                Code = code,
                Title = "x",
                ResourceTypeId = resourceTypeId,
                PermissionType = permissionType,
                SupervisorId = supervisorId,
                File = FakeFileDto("a.docx")
            });

            var item = db.DocumentTemplates.FirstOrDefault(t => t.Id == id);

            // Assert
            Assert.Equal(code, item.Code);
            Assert.NotNull(validator.ValidateAsyncCalledWith);
            Assert.NotNull(validator.ValidateFileCalledWith);
            Assert.NotNull(authorization.AuthorizedDocumentTemplateEditDtoCalledWith);

        }

        [Fact]
        public async Task Update_NoFile_Hyperlink_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int id = 1;

            //Must have file for all codes expect Hyperlink. Otherwise for tests will throw null referecnec ex
            //due to fakeFileValidator. In real live, it will fall at validateFileAsync and return FileRequired.
            //Null condition for FileRequired is tested separetly.
            const string code = Domain.Constants.Classifiers.DocumentType.Hyperlink;
            const string title = "z";

            var permissionType = UserProfileType.Supervisor;
            var supervisorId = 1;

            var fileId = Guid.NewGuid();
            var fileEnId = Guid.NewGuid();

            var resourceTypeId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = resourceTypeId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.ResourceType
            });

            db.SaveChanges();

            db.DocumentTemplates.AddRange(new DocumentTemplate
            {
                Id = id,
                FileId = fileId,
                Code = string.Empty,
                Title = string.Empty,
                ResourceTypeId = resourceTypeId
            });

            db.SaveChanges();

            var validator = ServiceFactory.CreateDocumentTemplateValidator();
            var authorization = ServiceFactory.CreateAuthorizationService();

            var service = CreateService(
                db,
                validator: validator,
                authorization: authorization);

            // Act
            await service.UpdateAsync(id, new DocumentTemplateEditDto
            {
                Code = code,
                Title = title,
                PermissionType = permissionType,
                SupervisorId = supervisorId,
                ResourceTypeId = resourceTypeId
            });

            var item = db.DocumentTemplates.FirstOrDefault(t => t.Id == id);

            // Assert
            Assert.Equal(code, item.Code);
            Assert.Equal(title, item.Title);
            Assert.Equal(Guid.Empty, item.FileId);
            Assert.NotNull(validator.ValidateAsyncCalledWith);
            Assert.Null(validator.ValidateFileCalledWith);
            Assert.NotNull(authorization.AuthorizedDocumentTemplateEditDtoCalledWith);
        }

        [Fact]
        public async Task Update_ThrowsNotFound()
        {
            // Assign
            var db = ServiceFactory.ConnectDb();

            var service = CreateService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.UpdateAsync(1, new DocumentTemplateEditDto()));
        }

        [Fact]
        public async Task Delete_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int id = 1;

            var resourceTypeId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = resourceTypeId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.ResourceType
            });

            db.SaveChanges();

            db.DocumentTemplates.Add(new DocumentTemplate
            {
                Id = id,
                Code = string.Empty,
                Title = string.Empty,
                ResourceTypeId = resourceTypeId
            });

            db.SaveChanges();

            var service = CreateService(db);

            // Act
            await service.DeleteAsync(id);

            var exists = db.DocumentTemplates.Any(t => t.Id == id);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task Delete_ThrowsNotFound()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = CreateService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.DeleteAsync(1));
        }

        [Fact]
        public async Task GetSampleAsync_Succeeds()
        {
            // Assign
            const int id = 1;
            const string content = "content";

            var fileId = Guid.NewGuid();
            var resourceTypeId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceTypeId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.ResourceType
            });

            await db.SaveChangesAsync();

            await db.DocumentTemplates.AddAsync(new DocumentTemplate
            {
                Id = id,
                Code = DocumentType.PNA,
                Title = "Title",
                FileId = fileId,
                ResourceTypeId = resourceTypeId
            });

            var fileService = ServiceFactory.CreateFileService();

            await fileService.AddOrUpdateAsync(fileId, new FileDto
            {
                FileName = "template.html",
                ContentType = MediaTypeNames.Text.Html,
                Content = Encoding.UTF8.GetBytes(content)
            });

            await db.SaveChangesAsync();

            var service = CreateService(db, fileService: fileService);

            // Act
            var result = await service.GetSampleAsync(id);

            // Assert
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task GetSampleAsync_ThrowsNotFound()
        {
            // Assign
            var db = ServiceFactory.ConnectDb();

            var service = CreateService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetSampleAsync(1));
        }

        private DocumentTemplateService CreateService(
            IAppDbContext db,
            IFileService fileService = null,
            IDocumentTemplateValidator validator = null,
            ICurrentUserProfileService currentUserProfile = null,
            IAuthorizationService authorization = null)
        {
            return new DocumentTemplateService(
                db,
                fileService: fileService ?? ServiceFactory.CreateFileService(),
                validator: validator ?? ServiceFactory.CreateDocumentTemplateValidator(),
                currentUserProfile: currentUserProfile ?? ServiceFactory.CreateCurrentUserProfileService(),
                authorizationService: authorization ?? ServiceFactory.CreateAuthorizationService());
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
                Content = FakeContent()
            };
        }
    }
}
