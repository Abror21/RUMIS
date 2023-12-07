namespace Izm.Rumis.Application.Dto
{
    public class SupervisorCreateDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class SupervisorUpdateDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }
    }
}
