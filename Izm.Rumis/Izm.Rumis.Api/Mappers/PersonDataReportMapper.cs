using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    internal static class PersonDataReportMapper
    {
        public static PersonDataReportGenerateDto Map(PersonDataReportGenerateRequest model, PersonDataReportGenerateDto dto)
        {
            dto.DataHandlerPrivatePersonalIdentifier = model.DataHandlerPrivatePersonalIdentifier;
            dto.DataOwnerPrivatePersonalIdentifier = model.DataOwnerPrivatePersonalIdentifier;
            dto.Notes = model.Notes;
            dto.ReasonId = model.ReasonId;

            return dto;
        }

        public static Expression<Func<GdprAudit, PersonDataReportListItemResponse>> Project()
        {
            return gdprAudit => new PersonDataReportListItemResponse
            {
                Id = gdprAudit.Id,
                Action = gdprAudit.Action,
                ActionData = gdprAudit.ActionData,
                Created = gdprAudit.Created,
                DataHandler = gdprAudit.DataHandlerId == null ? null : new PersonDataReportListItemResponse.PersonTechnicalData
                {
                    PersonTechnicalId = gdprAudit.DataHandlerId.Value,
                    Persons = gdprAudit.DataHandler.Persons.Select(person => new PersonDataReportListItemResponse.PersonTechnicalData.PersonData
                    {
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        PrivatePersonalIdentifier = person.PrivatePersonalIdentifier
                    })
                },
                DataHandlerPrivatePersonalIdentifier = gdprAudit.DataHandlerPrivatePersonalIdentifier,
                DataOwner = gdprAudit.DataOwnerId == null ? null : new PersonDataReportListItemResponse.PersonTechnicalData
                {
                    PersonTechnicalId = gdprAudit.DataOwnerId.Value,
                    Persons = gdprAudit.DataOwner.Persons.Select(person => new PersonDataReportListItemResponse.PersonTechnicalData.PersonData
                    {
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        PrivatePersonalIdentifier = person.PrivatePersonalIdentifier
                    })
                },
                DataOwnerPrivatePersonalIdentifier = gdprAudit.DataOwnerPrivatePersonalIdentifier,
                ProcessedData = gdprAudit.Data.Select(data => new PersonDataReportListItemResponse.ProcessedDataEntry
                {
                    Type = data.Type,
                    Value = data.Value
                })
            };
        }
    }
}
