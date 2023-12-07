using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public class DocumentTemplateValidatorTests
    {
        public static IEnumerable<object[]> ValidateAsyncSucceedsData()
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);

            yield return new object[] { null, dateNow.AddDays(-1), dateNow, dateNow.AddDays(1) };
            yield return new object[] { dateNow.AddDays(-2), dateNow.AddDays(-1), dateNow, dateNow.AddDays(1) };
            yield return new object[] { null, dateNow.AddDays(-1), dateNow, null };
            yield return new object[] { null, dateNow.AddDays(-1), null, null };
            yield return new object[] { dateNow.AddDays(-2), dateNow.AddDays(-1), null, null };
            yield return new object[] { dateNow.AddDays(-2), dateNow.AddDays(-1), null, dateNow.AddDays(1) };
        }

        [Theory]
        [MemberData(nameof(ValidateAsyncSucceedsData))]
        public async Task ValidateAsync_Succeeds(DateOnly? validFrom1, DateOnly? validTo1, DateOnly? validFrom2, DateOnly? validTo2)
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

            var item = CreateValidEntity(resourceTypeId);
            item.ValidFrom = validFrom1;
            item.ValidTo = validTo1;

            var item2 = CreateValidEntity(resourceTypeId);
            item2.ValidFrom = validFrom2;
            item2.ValidTo = validTo2;

            db.DocumentTemplates.Add(item);
            db.SaveChanges();

            var validator = GetValidator(db);

            // Act
            await validator.ValidateAsync(item2);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task ValidateAsync_Hyperlink_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidHyperlinkEntity(Guid.NewGuid());

            var validator = GetValidator(db);

            // Act
            await validator.ValidateAsync(item);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task ValidateAsync_Throws_CodeRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidEntity(Guid.NewGuid());
            item.Code = null;

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(item));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.CodeRequired, result.Message);
        }

        [Fact]
        public async Task ValidateAsync_Throws_HyperlinkRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidHyperlinkEntity(Guid.NewGuid());
            item.Hyperlink = null;

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(item));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.HyperlinkRequired, result.Message);
        }

        [Fact]
        public async Task ValidateAsync_Throws_TitleRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidEntity(Guid.NewGuid());
            item.Title = null;

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(item));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.TitleRequired, result.Message);
        }

        [Fact]
        public async Task ValidateAsync_Throws_DateNowGreaterThanDateFrom()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);

            var item = CreateValidEntity(Guid.NewGuid());
            item.ValidFrom = dateNow.AddDays(-3);

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(item));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.DateNowGreaterThanDateFrom, result.Message);
        }

        [Fact]
        public async Task ValidateAsync_Throws_DateNowGreaterThanDateTo()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);

            var item = CreateValidEntity(Guid.NewGuid());
            item.ValidTo = dateNow.AddDays(-3);

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(item));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.DateNowGreaterThanDateTo, result.Message);
        }

        [Fact]
        public async Task ValidateAsync_Throws_DateFromGreaterThanDateTo()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);

            var item = CreateValidEntity(Guid.NewGuid());
            item.ValidFrom = dateNow.AddDays(3);
            item.ValidTo = dateNow.AddDays(1);

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(item));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.DateFromGreaterThanDateTo, result.Message);
        }

        [Fact]
        public async Task ValidateAsync_Throws_CountryRecordRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidEntity(Guid.NewGuid());
            item.PermissionType = UserProfileType.Supervisor;
            item.SupervisorId = 1;

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(item));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.CountryRecordRequired, result.Message);
        }

        public static IEnumerable<object[]> ValidateAsyncAlreadyExistsData()
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);

            yield return new object[] { null, null, dateNow, dateNow.AddDays(1) };
            yield return new object[] { dateNow.AddDays(-1), null, dateNow, dateNow.AddDays(1) };
            yield return new object[] { dateNow.AddDays(-1), dateNow, null, null };
            yield return new object[] { dateNow.AddDays(-1), null, null, null };
            yield return new object[] { dateNow.AddDays(-2), dateNow.AddDays(1), dateNow, dateNow.AddDays(2) };
            yield return new object[] { dateNow, dateNow, dateNow, dateNow };
            yield return new object[] { dateNow.AddDays(1), dateNow.AddDays(3), dateNow, dateNow.AddDays(2) };

        }

        [Theory]
        [MemberData(nameof(ValidateAsyncAlreadyExistsData))]
        public async Task ValidateAsync_Throws_AlreadyExists(DateOnly? validFrom1, DateOnly? validTo1, DateOnly? validFrom2, DateOnly? validTo2)
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

            var item = CreateValidEntity(resourceTypeId);
            item.ValidFrom = validFrom1;
            item.ValidTo = validTo1;

            var item2 = CreateValidEntity(resourceTypeId);
            item2.ValidFrom = validFrom2;
            item2.ValidTo = validTo2;

            db.DocumentTemplates.Add(item);
            db.SaveChanges();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(item2));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.AlreadyExists, result.Message);
        }

        [Fact]
        public async Task ValidateFileAsync_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            AddParameters(db);

            var file = CreateValidFileDto();

            var validator = GetValidator(db);

            // Act
            await validator.ValidateFileAsync(file);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task ValidateFileAsync_Throws_FileRequired_EmptyItem()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateFileAsync(null));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.FileRequired, result.Message);
        }

        [Fact]
        public async Task ValidateFileAsync_Throws_FileRequired_EmptyFileName()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var file = CreateValidFileDto();
            file.FileName = null;

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateFileAsync(file));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.FileRequired, result.Message);
        }

        [Fact]
        public async Task ValidateFileAsync_Throws_FileRequired_EmptyContent()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var file = CreateValidFileDto();
            file.Content = null;

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateFileAsync(file));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.FileRequired, result.Message);
        }

        [Fact]
        public async Task ValidateFileAsync_Throws_FileRequired_EmptyContentLength()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var file = CreateValidFileDto();
            file.Content = Encoding.UTF8.GetBytes("");

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateFileAsync(file));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.FileRequired, result.Message);
        }

        [Fact]
        public async Task ValidateFileAsync_Throws_MaxSizeExceeded()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            AddParameters(db, 0);

            var file = CreateValidFileDto();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateFileAsync(file));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.MaxSizeExceeded, result.Message);
        }

        [Fact]
        public async Task ValidateFileAsync_Throws_ExtensionNotAllowed()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            AddParameters(db);

            var file = CreateValidFileDto();
            file.FileName = "t.pdf";

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateFileAsync(file));

            // Assert
            Assert.Equal(DocumentTemplateValidator.Error.ExtensionNotAllowed, result.Message);
        }

        private DocumentTemplateValidator GetValidator(IAppDbContext db)
        {
            return new DocumentTemplateValidator(db);
        }

        private DocumentTemplate CreateValidEntity(Guid resourceTypeId)
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);

            return new DocumentTemplate
            {
                Code = "Code",
                Title = "Title",
                ValidFrom = dateNow,
                ValidTo = dateNow.AddDays(2),
                ResourceTypeId = resourceTypeId,
                PermissionType = UserProfileType.Country
            };
        }

        private DocumentTemplate CreateValidHyperlinkEntity(Guid resourceTypeId)
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);

            return new DocumentTemplate
            {
                Code = Domain.Constants.Classifiers.DocumentType.Hyperlink,
                Title = "Title",
                ValidFrom = dateNow,
                ValidTo = dateNow.AddDays(2),
                Hyperlink = "123",
                ResourceTypeId = resourceTypeId,
                PermissionType = UserProfileType.Country
            };
        }

        private FileDto CreateValidFileDto()
        {
            return new FileDto
            {
                FileName = "test.html",
                Content = Encoding.UTF8.GetBytes("test"),
            };
        }

        private void AddParameters(IAppDbContext db, int maxSize = 100000)
        {
            db.Parameters.AddRange(
                new Parameter { Code = ParameterCode.DocumentTemplateMaxSize, Value = maxSize.ToString() },
                new Parameter { Code = ParameterCode.DocumentTemplateExtensions, Value = ".html" });

            db.SaveChanges();
        }
    }
}
