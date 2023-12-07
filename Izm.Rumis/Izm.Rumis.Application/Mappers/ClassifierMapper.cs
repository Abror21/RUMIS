using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Entities;

namespace Izm.Rumis.Application.Mappers
{
    internal static class ClassifierMapper
    {
        public static Classifier Map(ClassifierEditDto item, Classifier entity)
        {
            entity.Value = item.Value;
            entity.Payload = item.Payload;
            entity.SortOrder = item.SortOrder;

            return entity;
        }
    }
}
