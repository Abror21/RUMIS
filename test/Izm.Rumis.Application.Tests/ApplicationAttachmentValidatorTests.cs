using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public class ApplicationAttachmentValidatorTests
    {
        [Fact]
        public void Validate_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidApplicationAttachment();

            var validator = GetValidator(db);

            // Act
            validator.Validate(item);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void Validate_Throws_NumberRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidApplicationAttachment();
            item.AttachmentNumber = string.Empty;

            var validator = GetValidator(db);

            // Act & Assert
            var result = Assert.Throws<ValidationException>(() => validator.Validate(item));

            // Assert
            Assert.Equal(ApplicationAttachmentValidator.Error.NumberRequired, result.Message);
        }

        [Fact]
        public void ValidateFile_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidFileDto();

            var validator = GetValidator(db);

            AddParameters(db);

            // Act
            validator.ValidateFile(item);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void ValidateFile_Throws_EmptyFileName_FileRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidFileDto();
            item.FileName = string.Empty;

            var validator = GetValidator(db);

            AddParameters(db);

            // Act & Assert
            var result = Assert.Throws<ValidationException>(() => validator.ValidateFile(item));

            // Assert
            Assert.Equal(ApplicationAttachmentValidator.Error.FileRequired, result.Message);
        }

        [Fact]
        public void ValidateFile_Throws_EmptyContent_FileRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidFileDto();
            item.Content = null;

            var validator = GetValidator(db);

            AddParameters(db);

            // Act & Assert
            var result = Assert.Throws<ValidationException>(() => validator.ValidateFile(item));

            // Assert
            Assert.Equal(ApplicationAttachmentValidator.Error.FileRequired, result.Message);
        }

        [Fact]
        public void ValidateFile_Throws_FileMaxSizeExceeded()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidFileDto();

            var validator = GetValidator(db);

            AddParameters(db, 0);

            // Act & Assert
            var result = Assert.Throws<ValidationException>(() => validator.ValidateFile(item));

            // Assert
            Assert.Equal(ApplicationAttachmentValidator.Error.FileMaxSizeExceeded, result.Message);
        }


        private ApplicationAttachment CreateValidApplicationAttachment()
        {
            return new ApplicationAttachment
            {
                Id = Guid.NewGuid(),
                AttachmentDate = new DateOnly(2020, 1, 1),
                AttachmentNumber = "a",
                ApplicationId = Guid.NewGuid(),
                FileId = Guid.NewGuid()
            };
        }

        private FileDto CreateValidFileDto()
        {
            return new FileDto
            {
                FileName = "test.docx",
                Content = Encoding.UTF8.GetBytes("test"),
            };
        }

        private ApplicationAttachmentValidator GetValidator(IAppDbContext db)
        {
            return new ApplicationAttachmentValidator(db);
        }

        private void AddParameters(AppDbContext db, int maxSize = 100000)
        {
            db.Parameters.AddRange(
                new Parameter { Code = ParameterCode.ApplicationAttachmentMaxSize, Value = maxSize.ToString() });

            db.SaveChanges();
        }
    }
}
