namespace Izm.Rumis.Domain.Entities
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
    }
}
