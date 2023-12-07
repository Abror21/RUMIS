using ExcelDataReader;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Helpers;
using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using Izm.Rumis.Infrastructure.ResourceImport.Dtos;
using Izm.Rumis.Infrastructure.ResourceImport.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.ResourceImport
{
    public interface IResourceImportService
    {
        /// <summary>
        /// Import data via file.
        /// </summary>
        /// <param name="item">Resource import data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task<ResourcesImportResult> ImportAsync(ResourceImportDataDto item, CancellationToken cancellationToken = default);
    }

    public class ResourcesImportService : IResourceImportService
    {
        private readonly IAppDbContext db;
        private readonly ISequenceService sequenceService;
        private readonly IAuthorizationService authorizationService;

        public ResourcesImportService(
            IAppDbContext db, 
            ISequenceService sequenceService,
            IAuthorizationService authorizationService)
        {
            this.db = db;
            this.sequenceService = sequenceService;
            this.authorizationService = authorizationService;
        }


        /// <inheritdoc />
        /// <inheritdoc/>
        public async Task<ResourcesImportResult> ImportAsync(ResourceImportDataDto item, CancellationToken cancellationToken = default)
        {
            authorizationService.Authorize(item.EducationalInstitutionId);

            if (item.File == null || string.IsNullOrEmpty(item.File.FileName) || item.File.Content == null || item.File.Content.Length == 0)
                throw new InvalidOperationException(Error.FileRequired);

            if (Path.GetExtension(item.File.FileName).ToLower() != ".xlsx")
                throw new InvalidOperationException(Error.ExtensionNotAllowed);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = new MemoryStream(item.File.Content);
            using var xlsx = ExcelReaderFactory.CreateReader(stream);

            var maxErrorCount = int.Parse((await db.Parameters.FirstAsync(t => t.Code == ParameterCode.ResourceImportMaxErrorCount, cancellationToken)).Value);

            var result = new ResourcesImportResult(maxErrorCount);

            var propertyMap = new Dictionary<string, ResourcesImportDataProperty>();

            foreach (var property in typeof(ResourcesImportData).GetProperties())
            {
                var jsonAttribute = property.GetCustomAttribute<JsonPropertyAttribute>();

                if (jsonAttribute != null)
                    propertyMap[jsonAttribute.PropertyName] = new ResourcesImportDataProperty
                    {
                        Type = property.PropertyType,
                        IsRequired = property.PropertyType == typeof(string)
                                        ? property.GetCustomAttribute<RequiredAttribute>() != null
                                        : Nullable.GetUnderlyingType(property.PropertyType) == null,
                        ClassifierType = property.GetCustomAttribute<ClassifierTypeAttribute>()?.Value,
                    };
            }

            var ds = xlsx.AsDataSet();
            var dt = ds.Tables[0];

            if (dt.Rows == null)
            {
                result.AddError(Error.EmptyFile);
                return result;
            }

            int firstRowIx;

            for (firstRowIx = 0; firstRowIx < dt.Rows.Count; firstRowIx++)
                if (dt.Rows[firstRowIx].ItemArray.Any(t => propertyMap.ContainsKey(t.ToString())))
                    break;

            if (firstRowIx >= dt.Rows.Count)
            {
                result.AddError(Error.EmptyTable);
                return result;
            }

            var columnNames = dt.Rows[firstRowIx].ItemArray
                .Select((t, ix) => new { value = t.ToString(), ix })
                .Where(t => propertyMap.ContainsKey(t.value))
                .ToArray();

            var classifierTypes = propertyMap.Values
                .Select(p => p.ClassifierType)
                .Where(t => t != null);

            var importClassifiers = await db.Classifiers
                .Where(t => classifierTypes.Contains(t.Type))
                .Select(t => new { t.Id, t.Type, t.Code })
                .ToArrayAsync(cancellationToken);

            var items = new Dictionary<int, ResourcesImportData>();

            for (int i = firstRowIx + 1; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i];
                var obj = new ExpandoObject() as IDictionary<string, object>;
                var rowIx = i - firstRowIx;

                int notEmptyValues = 0;
                foreach (object rowItem in row.ItemArray)
                    if (rowItem != DBNull.Value)
                    {
                        notEmptyValues++;
                        if (notEmptyValues > 2)
                            break;
                    }

                if (notEmptyValues <= 2)
                    continue;

                foreach (var colName in columnNames)
                {
                    var cellValue = row[colName.ix];
                    var property = propertyMap[colName.value];

                    if (cellValue is double doubleValue)
                    {
                        if (property.Type == typeof(int) || property.Type == typeof(int?))
                            cellValue = (int)Math.Round(doubleValue);
                        else if (property.Type == typeof(decimal) || property.Type == typeof(decimal?))
                            cellValue = (decimal)doubleValue;
                        else if (property.Type == typeof(bool) || property.Type == typeof(bool?))
                            cellValue = doubleValue != 0;
                        else if (property.Type == typeof(string))
                            cellValue = doubleValue.ToString();
                    }
                    else if (cellValue is string stringValue && property.ClassifierType != null)
                    {
                        var classifier = importClassifiers.FirstOrDefault(t => t.Type == property.ClassifierType && t.Code == stringValue);

                        if (classifier != null)
                            cellValue = classifier.Id;
                        else if (result.AddError(Error.ClassifierNotFound, rowIx, colName.value))
                            return result;
                    }

                    if ((property.IsRequired
                                ? cellValue == null || cellValue == DBNull.Value || cellValue.GetType() != property.Type
                                : cellValue.GetType() != (property.Type == typeof(string) ? cellValue.GetType() : Nullable.GetUnderlyingType(property.Type)))
                            && result.AddError(Error.RequiredOrIncorrectValue, rowIx, colName.value))
                        return result;

                    obj.Add(colName.value, cellValue);
                }

                if (!result.Errors.Any())
                    items.Add(rowIx, JsonConvert.DeserializeObject<ResourcesImportData>(JsonConvert.SerializeObject(obj)));
            }

            if (result.Errors.Any())
                return result;

            var educationalInstitutionCode = await db.EducationalInstitutions
              .Where(t => t.Id == item.EducationalInstitutionId)
              .Select(t => t.Code)
              .FirstAsync(cancellationToken);

            var resources = await db.Resources
                .Where(t => t.EducationalInstitutionId == item.EducationalInstitutionId)
                .Select(t => new
                {
                    t.InventoryNumber,
                    t.SerialNumber,
                    t.ResourceSubTypeId,
                })
                .ToListAsync(cancellationToken);

            var statusId = await db.Classifiers
                .Where(t => t.Code == ResourceStatus.New && t.Type == ClassifierTypes.ResourceStatus)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);

            var resourceSubTypeMap = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.ResourceSubType)
                .Select(t => new { t.Id, t.Payload })
                .ToDictionaryAsync(
                    t => t.Id,
                    t => System.Text.Json.JsonSerializer.Deserialize<ResourceSubTypePayload>(t.Payload).ResourceType,
                    cancellationToken);

            foreach (var itemData in items)
            {
                if (resources.Any(t => t.InventoryNumber == itemData.Value.InventoryNumber
                            || (t.SerialNumber == itemData.Value.SerialNumber
                                && resourceSubTypeMap[t.ResourceSubTypeId] == resourceSubTypeMap[itemData.Value.ResourceSubTypeId])))
                {
                    if (result.AddError(Error.AlreadyExists, itemData.Key))
                        return result;

                    continue;
                }

                var serialNumberWithinInsitution = sequenceService.GetByKey(NumberingPatternHelper.ResourceKeyFormat(educationalInstitutionCode));

                var entity = ResourcesImportMapper.Map(itemData.Value, new Resource());

                entity.ResourceNumber = NumberingPatternHelper.ResourceNumberFormat(educationalInstitutionCode, serialNumberWithinInsitution);
                entity.EducationalInstitutionId = item.EducationalInstitutionId;
                entity.ResourceStatusId = statusId;

                await db.Resources.AddAsync(entity, cancellationToken);

                resources.Add(new
                {
                    entity.InventoryNumber,
                    entity.SerialNumber,
                    entity.ResourceSubTypeId
                });
            }

            if (!result.Errors.Any())
            {
                await db.SaveChangesAsync(cancellationToken);

                result.Imported = items.Count;
            }

            return result;
        }

        public static class Error
        {
            public const string EmptyFile = "resource.emptyFile";
            public const string EmptyTable = "resource.emptyTable";
            public const string RequiredOrIncorrectValue = "resource.requiredOrIncorrectValue";
            public const string ClassifierNotFound = "resource.classifierNotFound";
            public const string AlreadyExists = "resource.alreadyExists";
            public const string FileRequired = "resource.fileRequired";
            public const string ExtensionNotAllowed = "resource.extensionNotAllowed";
        }
    }
}

