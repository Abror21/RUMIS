using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Validators
{
    public interface IResourceValidator
    {
        /// <summary>
        /// Validate <see cref="ResourceCreateDto"/> data.
        /// </summary>
        /// <param name="item">Data to validate.</param>
        Task ValidateAsync(ResourceCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Validate <see cref="Resource"/> data.
        /// </summary>
        /// <param name="item">Data to validate.</param>
        Task ValidateAsync(Resource item, CancellationToken cancellationToken = default);
    }

    public sealed class ResourceValidator : IResourceValidator
    {
        private readonly IAppDbContext db;

        public ResourceValidator(IAppDbContext db)
        {
            this.db = db;
        }

        /// <inheritdoc/>
        /// <exception cref="ValidationException"></exception>
        public async Task ValidateAsync(ResourceCreateDto item, CancellationToken cancellationToken = default)
        {
            var resources = await db.Resources
                .Where(t => t.EducationalInstitutionId == item.EducationalInstitutionId)
                .Select(t => new
                {
                    t.InventoryNumber,
                    t.SerialNumber,
                    t.ResourceSubTypeId
                })
                .ToArrayAsync(cancellationToken);

            if (resources.Any(t => t.InventoryNumber == item.InventoryNumber))
                throw new ValidationException(Error.AlreadyExists);

            var resourceSubTypeMap = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.ResourceSubType)
                .Select(t => new { t.Id, t.Payload })
                .ToDictionaryAsync(
                    t => t.Id,
                    t => JsonSerializer.Deserialize<ResourceSubTypePayload>(t.Payload),
                    cancellationToken);

            if (resources.Any(t => t.SerialNumber == item.SerialNumber 
                                    && resourceSubTypeMap[t.ResourceSubTypeId].ResourceType == resourceSubTypeMap[item.ResourceSubTypeId].ResourceType))
                throw new ValidationException(Error.AlreadyExists);

            var parameters = await db.Classifiers.Where(t => t.Type == ClassifierTypes.ResourceParameter).Select(t => new
            {
                t.Id,
                t.Code
            }).ToArrayAsync(cancellationToken);

            foreach (var resourceParameter in item.ResourceParameters)
            {
                var parameter = parameters.First(t => t.Id == resourceParameter.ParameterId);

                var payloadParameter = resourceSubTypeMap[item.ResourceSubTypeId].ResourceParameterGroups
                    .SelectMany(g => g.Parameters)
                    .FirstOrDefault(p => p.Code == parameter.Code);

                if (payloadParameter != null && payloadParameter.IsRequired && string.IsNullOrEmpty(resourceParameter.Value))
                    throw new ValidationException(Error.ParameterRequired);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ValidationException"></exception>
        public async Task ValidateAsync(Resource item, CancellationToken cancellationToken = default)
        {
            var resources = await db.Resources
                .Where(t => t.Id != item.Id && t.EducationalInstitutionId == item.EducationalInstitutionId)
                .Select(t => new
                {
                    t.InventoryNumber,
                    t.SerialNumber,
                    t.ResourceSubTypeId
                })
                .ToArrayAsync(cancellationToken);

            if (resources.Any(t => t.InventoryNumber == item.InventoryNumber))
                throw new ValidationException(Error.AlreadyExists);

            var resourceSubTypeMap = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.ResourceSubType)
                .Select(t => new { t.Id, t.Payload })
                .ToDictionaryAsync(
                    t => t.Id,
                    t => JsonSerializer.Deserialize<ResourceSubTypePayload>(t.Payload),
                    cancellationToken);

            if (resources.Any(t => t.SerialNumber == item.SerialNumber
                                    && resourceSubTypeMap[t.ResourceSubTypeId].ResourceType == resourceSubTypeMap[item.ResourceSubTypeId].ResourceType))
                throw new ValidationException(Error.AlreadyExists);

            var parameters = await db.Classifiers.Where(t => t.Type == ClassifierTypes.ResourceParameter).Select(t => new
            {
                t.Id,
                t.Code
            }).ToArrayAsync(cancellationToken);

            foreach (var resourceParameter in item.ResourceParameters)
            {
                var parameter = parameters.First(t => t.Id == resourceParameter.ParameterId);

                var payloadParameter = resourceSubTypeMap[item.ResourceSubTypeId].ResourceParameterGroups
                    .SelectMany(g => g.Parameters)
                    .FirstOrDefault(p => p.Code == parameter.Code);

                if (payloadParameter != null && payloadParameter.IsRequired && string.IsNullOrEmpty(resourceParameter.Value))
                    throw new ValidationException(Error.ParameterRequired);
            }
        }

        public static class Error
        {
            public const string ParameterRequired = "resource.parameterRequired";
            public const string AlreadyExists = "resource.alreadyExists";
        }
    }
}
