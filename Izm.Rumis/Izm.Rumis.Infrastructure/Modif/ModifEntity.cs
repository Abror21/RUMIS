using System;

namespace Izm.Rumis.Infrastructure.Modif
{
    public abstract class ModifEntity
    {
        // since modifs extend real entities, a prefix is used to prevent naming conflicts
        public long _Id { get; set; }
        public DateTime _Date { get; set; }
        public string _Author { get; set; }
        public string _Action { get; set; }
    }
}
