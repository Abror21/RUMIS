namespace Izm.Rumis.Infrastructure.Identity.Dto
{
    public class UserCreateDto
    {
        public UserAuthType AuthType { get; set; }
        public string UserName { get; set; }
        public bool IsDisabled { get; set; }
    }
}
