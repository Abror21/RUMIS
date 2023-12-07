using System;

namespace Izm.Rumis.Infrastructure.Modif
{
    public class ModifDto<T>
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string Author { get; set; }
        public string Action { get; set; }

        public T Data { get; set; }
    }
}
