using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Validators;

namespace Izm.Rumis.Application.Tests.Common
{
    internal sealed class GdprAuditValidatorFake : IGdprAuditValidator
    {
        public GdprAuditTraceDto ValidateCalledWith { get; set; }

        public void Validate(GdprAuditTraceDto item)
        {
            ValidateCalledWith = item;

            return;
        }
    }
}
