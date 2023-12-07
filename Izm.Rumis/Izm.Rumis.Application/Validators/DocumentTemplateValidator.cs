using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Validators
{
    public interface IDocumentTemplateValidator
    {
        /// <summary>
        /// Validate <see cref="DocumentTemplate"/> item.
        /// </summary>
        /// <param name="item">Item to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ValidateAsync(DocumentTemplate item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Validate <see cref="FileDto"/> item.
        /// </summary>
        /// <param name="item">Item to validate.</param>
        /// <returns></returns>
        Task ValidateFileAsync(FileDto item, CancellationToken cancellationToken = default);
    }

    public sealed class DocumentTemplateValidator : IDocumentTemplateValidator
    {
        private readonly IAppDbContext db;

        public DocumentTemplateValidator(
             IAppDbContext db)
        {
            this.db = db;
        }

        // <inheritdoc/>
        public async Task ValidateAsync(DocumentTemplate item, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.Code))
                throw new ValidationException(Error.CodeRequired);

            if (string.IsNullOrEmpty(item.Title))
                throw new ValidationException(Error.TitleRequired);

            if (item.Code == DocumentType.Hyperlink && string.IsNullOrEmpty(item.Hyperlink))
                throw new ValidationException(Error.HyperlinkRequired);

            var date = DateOnly.FromDateTime(DateTime.UtcNow);

            if (item.ValidFrom != null && item.ValidFrom < date)
                throw new ValidationException(Error.DateNowGreaterThanDateFrom);

            if (item.ValidTo != null && item.ValidTo < date)
                throw new ValidationException(Error.DateNowGreaterThanDateTo);

            if (item.ValidFrom != null && item.ValidTo != null && item.ValidFrom > item.ValidTo)
                throw new ValidationException(Error.DateFromGreaterThanDateTo);

            var docs = await db.DocumentTemplates
                .Where(t => t.Id != item.Id
                            && t.Code == item.Code
                            && t.ResourceTypeId == item.ResourceTypeId)
                .ToArrayAsync(cancellationToken);

            if (item.PermissionType != UserProfileType.Country && !docs.Any(t => t.PermissionType == UserProfileType.Country))
                throw new ValidationException(Error.CountryRecordRequired);

            if (docs.Any(t => t.PermissionType == item.PermissionType
                            && t.SupervisorId == item.SupervisorId
                            && ((!t.ValidFrom.HasValue && !t.ValidTo.HasValue)
                                || ((!t.ValidFrom.HasValue || t.ValidFrom <= (item.ValidTo ?? date))
                                    && (!t.ValidTo.HasValue || t.ValidTo >= (item.ValidFrom ?? date))))))
                throw new ValidationException(Error.AlreadyExists);
        }

        // <inheritdoc/>
        public async Task ValidateFileAsync(FileDto item, CancellationToken cancellationToken = default)
        {
            if (item == null || string.IsNullOrEmpty(item.FileName) || item.Content == null || item.Content.Length == 0)
                throw new ValidationException(Error.FileRequired);

            var maxSize = (await db.Parameters.FirstAsync(t => t.Code == ParameterCode.DocumentTemplateMaxSize, cancellationToken)).Value;

            if (item.Content.Length > int.Parse(maxSize))
                throw new ValidationException(Error.MaxSizeExceeded);

            var allowedExt = (await db.Parameters.FirstAsync(t => t.Code == ParameterCode.DocumentTemplateExtensions, cancellationToken)).Value.ToLower().Split(',');

            if (!allowedExt.Contains(Path.GetExtension(item.FileName).ToLower()))
                throw new ValidationException(Error.ExtensionNotAllowed);
        }

        public static class Error
        {
            public const string AlreadyExists = "documentTemplate.alreadyExists";
            public const string CodeRequired = "documentTemplate.codeRequired";
            public const string DateNowGreaterThanDateFrom = "documentTemplate.dateNowGreaterThanDateFrom";
            public const string DateNowGreaterThanDateTo = "documentTemplate.dateNowGreaterThanDateTo";
            public const string DateFromGreaterThanDateTo = "documentTemplate.dateFromGreaterThanDateTo";
            public const string ExtensionNotAllowed = "documentTemplate.extensionNotAllowed";
            public const string FileRequired = "documentTemplate.fileRequired";
            public const string MaxSizeExceeded = "documentTemplate.maxSizeExceeded";
            public const string TitleRequired = "documentTemplate.titleRequired";
            public const string HyperlinkRequired = "documentTemplate.hyperlinkRequired";
            public const string CountryRecordRequired = "documentTemplate.countryRecordRequired";
        }
    }
}
