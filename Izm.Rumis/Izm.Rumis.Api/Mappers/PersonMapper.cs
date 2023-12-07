using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    internal static class PersonMapper
    {
        public static PersonCreateDto Map(PersonCreateRequest model, PersonCreateDto dto)
        {
            dto.FirstName = model.FirstName;
            dto.IsUser = model.IsUser;
            dto.LastName = model.LastName;
            dto.PrivatePersonalIdentifier = model.PrivatePersonalIdentifier;
            dto.BirthDate = model.BirthDate;
            dto.ContactInformation = model.ContactInformation.Select(t => new PersonCreateDto.ContactData
            {
                TypeId = t.TypeId,
                Value = t.Value
            });

            return dto;
        }

        public static PersonCreateResponse Map(PersonCreateResponseDto dto, PersonCreateResponse response)
        {
            response.Id = dto.Id;
            response.UserId = dto.UserId;

            return response;
        }

        public static PersonUpdateDto Map(PersonUpdateRequest model, PersonUpdateDto dto)
        {
            dto.FirstName = model.FirstName;
            dto.LastName = model.LastName;
            dto.PrivatePersonalIdentifier = model.PrivatePersonalIdentifier;
            dto.BirthDate = model.BirthDate;
            dto.ContactInformation = model.ContactInformation.Select(t => new PersonUpdateDto.ContactData
            {
                TypeId = t.TypeId,
                Value = t.Value
            });

            return dto;
        }

        public static Expression<Func<Person, PersonResponse>> Project()
        {
            return t => new PersonResponse
            {
                Id = t.Id,
                FirstName = t.FirstName,
                LastName = t.LastName,
                PrivatePersonalIdentifier = t.PrivatePersonalIdentifier,
                BirthDate = t.BirthDate,
                PersonTechnicalId = t.PersonTechnicalId,
                ContactInformation = t.PersonTechnical.PersonContacts
                                        .Where(t => t.IsActive == true)
                                        .Select(t => new PersonResponse.ContactData
                                        {
                                            Type = new PersonResponse.ContactData.ClassifierData
                                            {
                                                Id = t.ContactType.Id,
                                                Code = t.ContactType.Code,
                                                Value = t.ContactType.Value
                                            },
                                            Value = t.ContactValue
                                        })
            };
        }
    }
}
