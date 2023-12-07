using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Validators;

namespace Izm.Rumis.Application.Tests.Common
{
    internal sealed class PersonDataValidatorFake : IPersonDataReportValidator
    {
        public PersonDataReportGenerateDto ValidateCalledWith { get; set; }

        public void Validate(PersonDataReportGenerateDto item)
        {
            ValidateCalledWith = item;
        }
    }
}
