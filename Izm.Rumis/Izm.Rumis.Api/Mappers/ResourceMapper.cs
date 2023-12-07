using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.ResourceImport.Dtos;
using Izm.Rumis.Infrastructure.ResourceImport.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    public static class ResourceMapper
    {
        public static Expression<Func<Resource, ResourceIntermediateResponse>> IntermediateProject()
        {
            return t => new ResourceIntermediateResponse
            {
                Id = t.Id,
                ResourceNumber = t.ResourceNumber,
                ResourceName = t.ResourceName,
                Manufacturer = t.Manufacturer,
                ModelName = t.ModelName,
                ModelIdentifier = t.ModelIdentifier,
                EducationalInstitution = t.EducationalInstitution,
                ResourceSubType = t.ResourceSubType,
                ResourceStatus = t.ResourceStatus,
                ResourceLocation = t.ResourceLocation,
                AcquisitionsValue = t.AcquisitionsValue,
                ManufactureYear = t.ManufactureYear,
                InventoryNumber = t.InventoryNumber,
                SerialNumber = t.SerialNumber,
                SocialSupportResource = t.SocialSupportResource,
                ResourceStatusHistory = t.ResourceStatusHistory,
                Notes = t.Notes,
                ResourceGroup = t.ResourceGroup,
                TargetGroup = t.TargetGroup,
                UsagePurposeType = t.UsagePurposeType,
                AcquisitionType = t.AcquisitionType,
                ResourceParameters = t.ResourceParameters.Select(p => new ResourceParameter
                {
                    Id = p.Id,
                    Value = p.Value,
                    ParameterId = p.ParameterId,
                    Parameter = p.Parameter,
                }).ToArray(),
                Supervisor = t.EducationalInstitution.Supervisor
            };
        }

        public static Expression<Func<ResourceIntermediateResponse, ResourceResponse>> Project()
        {
            return t => new ResourceResponse
            {
                Id = t.Id,
                ResourceNumber = t.ResourceNumber,
                ResourceName = t.ResourceName,
                Manufacturer = MapClassifier(t.Manufacturer),
                ModelName = MapClassifier(t.ModelName),
                ModelIdentifier = t.ModelIdentifier,
                EducationalInstitution = new ResourceResponse.EducationalInstitutionData
                {
                    Id = t.EducationalInstitution.Id,
                    Name = t.EducationalInstitution.Name,
                    Code = t.EducationalInstitution.Code
                },
                ResourceSubType = MapClassifier(t.ResourceSubType),
                ResourceType = MapClassifier(t.ResourceType),
                ResourceStatus = MapClassifier(t.ResourceStatus),
                ResourceLocation = MapClassifier(t.ResourceLocation),
                AcquisitionsValue = t.AcquisitionsValue,
                ManufactureYear = t.ManufactureYear,
                InventoryNumber = t.InventoryNumber,
                SerialNumber = t.SerialNumber,
                SocialSupportResource = t.SocialSupportResource,
                ResourceStatusHistory = t.ResourceStatusHistory,
                Notes = t.Notes,
                ResourceGroup = MapClassifier(t.ResourceGroup),
                TargetGroup = MapClassifier(t.TargetGroup),
                UsagePurposeType = MapClassifier(t.UsagePurposeType),
                AcquisitionType = MapClassifier(t.AcquisitionType),
                ResourceParameters = t.ResourceParameters.Select(p => new ResourceResponse.ResourceParameterData
                {
                    Id = p.Id,
                    Value = p.Value,
                    Parameter = MapClassifier(p.Parameter),
                }).ToArray(),
                Supervisor = new ResourceResponse.SupervisorData
                {
                    Id = t.Supervisor.Id,
                    Name = t.Supervisor.Name,
                    Code = t.Supervisor.Code
                }
            };
        }

        private static ResourceEditDto Map(ResourceEditRequest model, ResourceEditDto dto)
        {
            dto.ResourceName = model.ResourceName;
            dto.ModelIdentifier = model.ModelIdentifier;
            dto.AcquisitionsValue = model.AcquisitionsValue;
            dto.ManufactureYear = model.ManufactureYear;
            dto.InventoryNumber = model.InventoryNumber;
            dto.SerialNumber = model.SerialNumber;
            dto.SocialSupportResource = model.SocialSupportResource;
            dto.Notes = model.Notes;
            dto.ResourceGroupId = model.ResourceGroupId;
            dto.ResourceStatusId = model.ResourceStatusId;
            dto.ResourceLocationId = model.ResourceLocationId;
            dto.TargetGroupId = model.TargetGroupId;
            dto.UsagePurposeTypeId = model.UsagePurposeTypeId;
            dto.AcquisitionTypeId = model.AcquisitionTypeId;
            dto.ManufacturerId = model.ManufacturerId;
            dto.ModelNameId = model.ModelNameId;
            dto.ResourceParameters = model.ResourceParameters.Select(t => new ResourceParameterDto
            {
                Id = t.Id,
                ParameterId = t.ParameterId,
                Value = t.Value
            }).ToArray();

            return dto;
        }

        public static ResourceCreateDto Map(ResourceCreateRequest model, ResourceCreateDto dto)
        {
            dto.EducationalInstitutionId = model.EducationalInstitutionId;
            dto.ResourceSubTypeId = model.ResourceSubTypeId;

            return (ResourceCreateDto)Map(model as ResourceEditRequest, dto);
        }

        public static ResourceUpdateDto Map(ResourceUpdateRequest model, ResourceUpdateDto dto)
        {
            return (ResourceUpdateDto)Map(model as ResourceEditRequest, dto);
        }

        private static readonly Func<Classifier, ResourceResponse.ClassifierData> classifierProjection =
            ((ClassifierData())).Compile();

        public static readonly Func<ResourceIntermediateResponse, ResourceResponse> ProjectCompiled =
           ((Project())).Compile();

        public static Expression<Func<Classifier, ResourceResponse.ClassifierData>> ClassifierData()
        {
            return t => new ResourceResponse.ClassifierData
            {
                Id = t.Id,
                Code = t.Code,
                Value = t.Value
            };
        }

        public static ResourceResponse.ClassifierData MapClassifier(Classifier entity)
        {
            return entity == null ? null : classifierProjection.Invoke(entity);
        }

        public static ResourceImportDataDto Map(ResourceImportDataRequest model, ResourceImportDataDto dto)
        {
            dto.EducationalInstitutionId = model.EducationalInstitutionId;
            dto.File = model.File == null ? null : Mapper.MapFile(model.File, new FileDto());

            return dto;
        }

        public static ResourceImportDataResponse Map(ResourcesImportResult result, ResourceImportDataResponse response)
        {
            response.Imported = result.Imported;
            response.Errors = result.Errors.Select(t => new ResourceImportDataResponse.Error
            {
                Message = t.Message,
                Row = t.Row,
                Column = t.Column
            }).ToArray();

            return response;
        }
    }
}
