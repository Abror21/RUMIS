using System.Collections.Generic;

namespace Izm.Rumis.Application.Dto
{
    public class RoleEditDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Permissions { get; set; } = new string[0];
    }
}
