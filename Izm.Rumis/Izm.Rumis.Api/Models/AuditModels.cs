using System;

namespace Izm.Rumis.Api.Models
{
    public class AuditUserModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
    }
}
