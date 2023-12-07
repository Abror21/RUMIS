using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Izm.Rumis.Infrastructure.Viis.Models
{
    [Serializable]
    [XmlRoot("RUMIS_Izglitibas_iestazu_dati")]
    public class EducationalInstitutionData : List<EducationalInstitutionData.Institution>
    {
        [Serializable]
        [XmlRoot("Institution")]
        public class Institution
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("RegNr")]
            public string RegNr { get; set; }

            [XmlAttribute("Phone")]
            public string Phone { get; set; }

            [XmlAttribute("Email")]
            public string Email { get; set; }

            [XmlElement("Adresse")]
            public Address Address { get; set; }

            [XmlElement("Founder")]
            public Founder Founder { get; set; }

            [XmlElement("Supervisor")]
            public Supervisor Supervisor { get; set; }
        }


        [Serializable]
        public class Address
        {
            [XmlAttribute("Adresse")]
            public string Adresse { get; set; }

            [XmlAttribute("City")]
            public string City { get; set; }

            [XmlAttribute("RuralTerritory")]
            public string RuralTerritory { get; set; }

            [XmlAttribute("Municipality")]
            public string Municipality { get; set; }

            [XmlAttribute("PostCode")]
            public string PostCode { get; set; }
        }

        [Serializable]
        public class Founder
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("UrCode")]
            public string UrCode { get; set; }
        }

        [Serializable]
        public class Supervisor
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("UrCode")]
            public string UrCode { get; set; }
        }
    }
}
