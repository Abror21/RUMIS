using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Izm.Rumis.Infrastructure.Viis.Models
{
    [Serializable]
    [XmlRoot("RUMIS_dati")]
    public class RelatedPersonData : List<RelatedPersonData.Student>
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

            [XmlAttribute("BirthDate")]
            public DateTime BirthDate { get; set; }

            [XmlAttribute("SubStatus")]
            public string SubStatus { get; set; }

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

            [XmlAttribute("FounderName")]
            public string FounderName { get; set; }

            [XmlAttribute("FounderATUCCode")]
            public string FounderATUCCode { get; set; }

            public Class Class { get; set; }

            public StudyParticipation StudyParticipation { get; set; }
        }

        [Serializable]
        public class StudyParticipation
        {
            [XmlAttribute("EducationProgramName")]
            public string EducationProgramName { get; set; }

            [XmlAttribute("EducationProgramCode")]
            public string EducationProgramCode { get; set; }
        }

        [Serializable]
        public class Class
        {
            [XmlAttribute("Type")]
            public string Type { get; set; }

            [XmlAttribute("ClassGrade")]
            public string ClassGrade { get; set; }

            [XmlAttribute("Parallel")]
            public string Parallel { get; set; }

            [XmlAttribute("GroupName")]
            public string GroupName { get; set; }

            [XmlAttribute("EducationProgramName")]
            public string EducationProgramName { get; set; }

            [XmlAttribute("EducationProgramCode")]
            public string EducationProgramCode { get; set; }
        }
    }
}
