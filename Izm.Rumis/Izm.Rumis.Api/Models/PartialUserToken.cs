using System;
using System.Collections.Generic;

namespace Izm.Rumis.Api.Models
{
    public class PartialUserToken
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public IEnumerable<string> Permissions { get; set; }
    }
}
