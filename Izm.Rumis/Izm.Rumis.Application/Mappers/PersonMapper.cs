using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System.Linq;

namespace Izm.Rumis.Application.Mappers
{
    internal static class PersonMapper
    {
        public static Person Map(PersonCreateDto item, Person entity)
        {
            entity.FirstName = item.FirstName;
            entity.LastName = item.LastName;
            entity.PrivatePersonalIdentifier = item.PrivatePersonalIdentifier;
            entity.BirthDate = item.BirthDate;
            entity.PersonTechnical.PersonContacts = item.ContactInformation.Select(t => new PersonContact
            {
                ContactTypeId = t.TypeId,
                ContactValue = t.Value,
                IsActive = true
            }).ToArray();

            return entity;
        }

        public static Person Map(PersonUpdateDto item, Person entity)
        {
            entity.FirstName = item.FirstName;
            entity.LastName = item.LastName;
            entity.PrivatePersonalIdentifier = item.PrivatePersonalIdentifier;
            entity.BirthDate = item.BirthDate;

            return entity;
        }
    }
}
