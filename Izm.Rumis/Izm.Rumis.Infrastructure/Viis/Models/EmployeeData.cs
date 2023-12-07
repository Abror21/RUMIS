using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Izm.Rumis.Infrastructure.Viis.Models
{
    [Serializable]
    [XmlRoot("RUMIS_darbinieku_dati")]
    public class EmployeeData : List<EmployeeData.Employee>
    {
        [Serializable]
        [XmlRoot("Employee")]
        public class Employee
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("OtherNames")]
            public string OtherNames { get; set; }

            [XmlAttribute("Surname")]
            public string Surname { get; set; }

            [XmlAttribute("PersonCode")]
            public string PersonCode { get; set; }

            [XmlElement("Institution")]
            public List<Institution> Institution { get; set; }
        }

        [Serializable]
        public class Institution
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("RegNr")]
            public string RegNr { get; set; }

            [XmlAttribute("FounderUrCode")]
            public string FounderUrCode { get; set; }

            [XmlAttribute("PositionName")]
            public string PositionName { get; set; }

            [XmlAttribute("PositionCode")]
            public string PositionCode { get; set; }
        }
    }
}
