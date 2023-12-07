using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;

namespace Izm.Rumis.Application.Validators
{
    public interface IPersonDataReportValidator
    {
        /// <summary>
        /// Validate person data report generate data.
        /// </summary>
        /// <param name="item">Data to validate.</param>
        void Validate(PersonDataReportGenerateDto item);
    }

    public sealed class PersonDataReportValidator : IPersonDataReportValidator
    {
        /// <inheritdoc/>
        public void Validate(PersonDataReportGenerateDto item)
        {
            if (string.IsNullOrEmpty(item.DataHandlerPrivatePersonalIdentifier) && string.IsNullOrEmpty(item.DataOwnerPrivatePersonalIdentifier))
                throw new ValidationException(Error.DataOwnerOrHanlderPrivatePersonalIdentifierRequired);
        }

        public static class Error
        {
            public const string DataOwnerOrHanlderPrivatePersonalIdentifierRequired = "personDataReport.dataOwnerOrHanlderPrivatePersonalIdentifierRequired";
        }
    }
}
