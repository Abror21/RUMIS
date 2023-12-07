namespace Izm.Rumis.Application
{
    public interface ITreeOrdered
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int Order { get; set; }
        public int Level { get; set; }
        public string Path { get; set; }
        public string NormalizedOrder { get; set; }
        public string ListOrder { get; set; }
        public int Index { get; set; }
    }
}
