using Izm.Rumis.Infrastructure.EAddress.Enums;
using System;

namespace Izm.Rumis.Infrastructure.EAddress.Models
{
    public class EAddressSendMessageRequest
    {
        public string RecipientIdentifier { get; set; }
        public RecipientType RecipientType { get; set; }
        public string Content { get; set; }
        public string Subject { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime? DocumentDate { get; set; }
        public bool? IsLimitedAvailability { get; set; }
    }
}
