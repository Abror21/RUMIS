using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Entities;
using System;
using System.Threading.Tasks;
using Xunit;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using System.Text.Json;
using Izm.Rumis.Domain.Constants.Classifiers;

namespace Izm.Rumis.Application.Tests
{
    public class ClassifierValidatorTests
    {
        [Fact]
        public async Task ValidateAsync_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var entity = CreateValidClassifier();

            db.Classifiers.Add(new Classifier
            {
                Id = Guid.NewGuid(),
                Type = ClassifierTypes.ClassifierType,
                Code = entity.Type,
                Value = string.Empty,
                PermissionType = entity.PermissionType
            });

            db.SaveChanges();

            var validator = GetValidator(db);

            // Act
            await validator.ValidateAsync(entity);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task ValidateAsync_Throws_TypeForbidden()
        {
            using var db = ServiceFactory.ConnectDb();

            var entity = CreateValidClassifier();
            entity.Code = entity.Type;
            entity.Type = ClassifierTypes.ClassifierType;

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(entity));

            // Assert
            Assert.Equal(ClassifierValidator.Error.TypeForbidden, result.Message);
        }

        [Fact]
        public async Task ValidateAsync_SameCode_Throws_IncorrectType()
        {
            using var db = ServiceFactory.ConnectDb();

            var entity = CreateValidClassifier();

            db.Classifiers.Add(new Classifier
            {
                Id = Guid.NewGuid(),
                Type = ClassifierTypes.ClassifierType,
                Code = entity.Type,
                Value = string.Empty,
                PermissionType = UserProfileType.Supervisor
            });

            await db.SaveChangesAsync();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(entity));

            // Assert
            Assert.Equal(ClassifierValidator.Error.IncorrectPermissionType, result.Message);
        }

        [Fact]
        public async Task ValidateAsync_SameCode_Throws_AlreadyExists()
        {
            using var db = ServiceFactory.ConnectDb();

            var entity = CreateValidClassifier();

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = Guid.NewGuid(),
                    Type = ClassifierTypes.ClassifierType,
                    Code = entity.Type,
                    Value = string.Empty,
                    PermissionType = entity.PermissionType
                },
                new Classifier
                {
                    Id = Guid.NewGuid(),
                    Type = entity.Type,
                    Code = entity.Code,
                    Value = string.Empty
                });

            await db.SaveChangesAsync();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(entity));

            // Assert
            Assert.Equal(ClassifierValidator.Error.AlreadyExists, result.Message);
        }

        [Fact]
        public async Task ValidateAsync_SameValue_Throws_AlreadyExists()
        {
            using var db = ServiceFactory.ConnectDb();

            var entity = CreateValidClassifier();

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = Guid.NewGuid(),
                    Type = ClassifierTypes.ClassifierType,
                    Code = entity.Type,
                    Value = string.Empty,
                    PermissionType = entity.PermissionType
                },
                new Classifier
                {
                    Id = Guid.NewGuid(),
                    Type = entity.Type,
                    Code = string.Empty,
                    Value = entity.Value
                });

            db.SaveChanges();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(entity));

            // Assert
            Assert.Equal(ClassifierValidator.Error.AlreadyExists, result.Message);
        }
        
        [Fact]
        public async Task ValidateAsync_Throws_ValueRequired()
        {
            using var db = ServiceFactory.ConnectDb();

            var entity = CreateValidClassifier();
            entity.Value = string.Empty;

            db.Classifiers.Add(new Classifier
            {
                Id = Guid.NewGuid(),
                Type = ClassifierTypes.ClassifierType,
                Code = entity.Type,
                Value = string.Empty,
                PermissionType = entity.PermissionType
            });

            db.SaveChanges();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(entity));

            // Assert
            Assert.Equal(ClassifierValidator.Error.ValueRequired, result.Message);
        }

        [Fact]
        public async Task ValidateAsync_Throws_UnknownType()
        {
            using var db = ServiceFactory.ConnectDb();

            var entity = CreateValidClassifier();
            entity.Type = "x";

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(entity));

            // Assert
            Assert.Equal(ClassifierValidator.Error.UnknownType, result.Message);
        }

        [Fact]
        public async Task ValidateAsync_ResourceSubType_Throws_PayloadIncomplete()
        {
            using var db = ServiceFactory.ConnectDb();

            var entity = CreateValidClassifier();
            entity.Type = ClassifierTypes.ResourceSubType;
            entity.Payload = JsonSerializer.Serialize(new ResourceSubTypePayload());

            db.Classifiers.Add(new Classifier
            {
                Id = Guid.NewGuid(),
                Type = ClassifierTypes.ClassifierType,
                Code = ClassifierTypes.ResourceSubType,
                Value = string.Empty,
                PermissionType = entity.PermissionType
            });

            db.SaveChanges();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(entity));

            // Assert
            Assert.Equal(ClassifierValidator.Error.PayloadIncomplete, result.Message);
        }

        [Fact]
        public async Task ValidateAsync_Placeholder_Throws_PayloadIncomplete()
        {
            using var db = ServiceFactory.ConnectDb();

            var entity = CreateValidClassifier();
            entity.Type = ClassifierTypes.Placeholder;
            entity.Payload = JsonSerializer.Serialize(new PlaceholderPayload());

            db.Classifiers.Add(new Classifier
            {
                Id = Guid.NewGuid(),
                Type = ClassifierTypes.ClassifierType,
                Code = ClassifierTypes.Placeholder,
                Value = string.Empty,
                PermissionType = entity.PermissionType
            });

            db.SaveChanges();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(entity));

            // Assert
            Assert.Equal(ClassifierValidator.Error.PayloadIncomplete, result.Message);
        }

        [Theory]
        [InlineData(ClassifierTypes.Placeholder)]
        [InlineData(ClassifierTypes.ResourceSubType)]
        public async Task ValidateAsync_Throws_CannotDeserializePayload(string code)
        {
            using var db = ServiceFactory.ConnectDb();

            var entity = CreateValidClassifier();
            entity.Type = code;
            entity.Payload = JsonSerializer.Serialize("");

            db.Classifiers.Add(new Classifier
            {
                Id = Guid.NewGuid(),
                Type = ClassifierTypes.ClassifierType,
                Code = code,
                Value = string.Empty,
                PermissionType = entity.PermissionType
            });

            db.SaveChanges();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(entity));

            // Assert
            Assert.Equal(ClassifierValidator.Error.CannotDeserializePayload, result.Message);
        }

        private Classifier CreateValidClassifier()
        {
            return new Classifier
            {
                Id = Guid.NewGuid(),
                Type = ClassifierTypes.ApplicantRole,
                Code = ApplicantRole.ParentGuardian,
                Value = "v",
                PermissionType = UserProfileType.EducationalInstitution
            };
        }

        private ClassifierValidator GetValidator(IAppDbContext db)
        {
            return new ClassifierValidator(db);
        }
    }
}
