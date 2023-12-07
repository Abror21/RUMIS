namespace Izm.Rumis.Application.Common
{
    public interface ISequenceService
    {
        long GetByKey(string key = "default");
    }
}
