using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;

namespace Izm.Rumis.Application.Validators
{
    public interface IGdprAuditValidator
    {
        /// <summary>
        /// Validate trace data.
        /// </summary>
        /// <param name="item"></param>
        void Validate(GdprAuditTraceDto item);
    }

    public sealed class GdprAuditValidator : IGdprAuditValidator
    {
        public void Validate(GdprAuditTraceDto item)
        {
            if (string.IsNullOrEmpty(item.Action))
                throw new ValidationException(Error.MissingAction);

            //if (item.DataOwnerId == null && string.IsNullOrEmpty(item.PrivatePersonalIdentifier))
            //    throw new ValidationException(Error.MissingDataOwner);
        }

        public static class Error
        {
            public const string MissingDataOwner = "gdprAudit.missingDataOwner";
            public const string MissingAction = "gdprAudit.missingAction";
        }
    }
}
