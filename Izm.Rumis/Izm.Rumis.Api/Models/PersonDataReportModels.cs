using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Api.Models
{
    public class PersonDataReportGenerateRequest
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public Guid ReasonId { get; set; }

        [MaxLength(11)]
        public string DataHandlerPrivatePersonalIdentifier { get; set; }

        [MaxLength(11)]
        public string DataOwnerPrivatePersonalIdentifier { get; set; }

        [Required]
        [MaxLength(200)]
        public string Notes { get; set; }
    }

    public class PersonDataReportListItemResponse
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
        public string ActionData { get; set; }
        public DateTime Created { get; set; }
        public PersonTechnicalData DataHandler { get; set; }
        public string DataHandlerPrivatePersonalIdentifier { get; set; }
        public PersonTechnicalData DataOwner { get; set; }
        public string DataOwnerPrivatePersonalIdentifier { get; set; }
        public IEnumerable<ProcessedDataEntry> ProcessedData { get; set; }

        public class PersonTechnicalData
        {
            public Guid PersonTechnicalId { get; set; }
            public IEnumerable<PersonData> Persons { get; set; }

            public class PersonData
            {
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string PrivatePersonalIdentifier { get; set; }
            }
        }

        public class ProcessedDataEntry
        {
            public PersonDataType Type { get; set; }
            public string Value { get; set; }
        }
    }
}
