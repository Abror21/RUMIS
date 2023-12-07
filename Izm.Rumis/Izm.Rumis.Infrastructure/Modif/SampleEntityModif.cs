using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Izm.Rumis.Infrastructure.Modif
{
    [Table("SampleEntities_Modif")]
    public class SampleEntityModif : ModifEntity
    {
        public int Id { get; set; }
        public Guid? SomeClassifierId { get; set; }
        public Guid ModifiedById { get; set; }
    }

    public class SampleEntityModifDto
    {
        public Guid? SomeClassifierId { get; set; }
        public ClassifierValue SomeClassifier { get; set; }
    }
}
