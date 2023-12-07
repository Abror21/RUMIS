using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Izm.Rumis.Infrastructure.Viis.Models
{
    [Serializable]
    [XmlRoot("RUMIS_socialie_statusi")]
    public class SocialStatusData : List<SocialStatusData.Student>
    {
        [Serializable]
        [XmlRoot("Student")]
        public class Student
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("OtherNames")]
            public string OtherNames { get; set; }

            [XmlAttribute("Surname")]
            public string Surname { get; set; }

            [XmlAttribute("PersonCode")]
            public string PersonCode { get; set; }

            [XmlElement("SocialStatus")]
            public List<SocialStatus> SocialStatus { get; set; }
        }

        [Serializable]
        public class SocialStatus
        {
            [XmlAttribute("StatusTypeCode")]
            public string StatusTypeCode { get; set; }

            [XmlAttribute("StatusTypeName")]
            public string StatusTypeName { get; set; }

            [XmlAttribute("DateTo")]
            public DateTime DateTo { get; set; }
        }
    }
}
