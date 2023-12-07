using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class PersonValidatorTests
    {
        [Fact]
        public async Task ValidateAsync_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var dto = CreateValidPersonEditDto();

            var validator = GetValidator(db);

            // Act
            await validator.ValidateAsync(dto);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task ValidateAsync_Throws_AlreadyExists()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var dto = CreateValidPersonEditDto();

            db.Persons.Add(new Person
            {
                PrivatePersonalIdentifier = dto.PrivatePersonalIdentifier
            });

            await db.SaveChangesAsync();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(dto));

            // Assert
            Assert.Equal(PersonValidator.Error.AlreadyExists, result.Message);
        }

        //[Fact]
        //public async Task ValidateAsync_Throws_InvalidPrivatePersonalIdentifier()
        //{
        //    // Assign
        //    using var db = ServiceFactory.ConnectDb();

        //    var dto = CreateValidPersonEditDto();
        //    dto.PrivatePersonalIdentifier = "00000000000";

        //    var validator = GetValidator(db);

        //    // Act & Assert
        //    var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(dto));

        //    // Assert
        //    Assert.Equal(PersonValidator.Error.InvalidPrivatePersonalIdentifier, result.Message);
        //}

        private PersonCreateDto CreateValidPersonEditDto()
        {
            return new PersonCreateDto
            {
                PrivatePersonalIdentifier = "00000000001"
            };
        }

        private PersonValidator GetValidator(IAppDbContext db)
        {
            return new PersonValidator(db);
        }
    }
}
