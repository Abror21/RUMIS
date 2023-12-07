using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Validators;
using System;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class PersonDataReportValidatorTests
    {
        private PersonDataReportGenerateDto validPersonDataReportGenerateDto { get; } = new PersonDataReportGenerateDto()
        {
            Notes = "someNotes",
            DataHandlerPrivatePersonalIdentifier = "00000000002",
            DataOwnerPrivatePersonalIdentifier = "00000000001",
            ReasonId = Guid.NewGuid()
        };

        [Fact]
        public void Validate_Succeeds()
        {
            // Assign
            var validator = GetValidator();

            // Act
            validator.Validate(validPersonDataReportGenerateDto);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void Validate_Throws_DataOwnerOrHanlderPrivatePersonalIdentifierRequired()
        {
            // Assign
            var invalidDto = validPersonDataReportGenerateDto;
            invalidDto.DataHandlerPrivatePersonalIdentifier = null;
            invalidDto.DataOwnerPrivatePersonalIdentifier = null;

            var validator = GetValidator();

            // Act & Assert
            var result = Assert.Throws<ValidationException>(() => validator.Validate(invalidDto));

            // Assert
            Assert.Equal(PersonDataReportValidator.Error.DataOwnerOrHanlderPrivatePersonalIdentifierRequired, result.Message);
        }

        private PersonDataReportValidator GetValidator()
        {
            return new PersonDataReportValidator();
        }
    }
}
