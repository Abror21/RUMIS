using System;

namespace Izm.Rumis.Application.Dto
{
    public class PersonDataReportGenerateDto
    {
        public string DataHandlerPrivatePersonalIdentifier { get; set; }
        public string DataOwnerPrivatePersonalIdentifier { get; set; }
        public Guid ReasonId { get; set; }
        public string Notes { get; set; }
    }
}
