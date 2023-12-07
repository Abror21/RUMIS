using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Validators
{
    public interface IApplicationAttachmentValidator
    {
        /// <summary>
        /// Validate <see cref="ApplicationAttachment"/> item.
        /// </summary>
        /// <param name="item">Item to validate.</param>
        /// <returns></returns>
        void Validate(ApplicationAttachment item);
        /// <summary>
        /// Validate <see cref="FileDto"/> item.
        /// </summary>
        /// <param name="item">Item to validate.</param>
        /// <returns></returns>
        void ValidateFile(FileDto item);
    }

    public sealed class ApplicationAttachmentValidator : IApplicationAttachmentValidator
    {
        private readonly IAppDbContext db;

        public ApplicationAttachmentValidator(IAppDbContext db)
        {
            this.db = db;
        }

        // <inheritdoc/>
        public void Validate(ApplicationAttachment item)
        {
            if (string.IsNullOrEmpty(item.AttachmentNumber))
                throw new ValidationException(Error.NumberRequired);
        }

        // <inheritdoc/>
        public void ValidateFile(FileDto item)
        {
            if (item == null || string.IsNullOrEmpty(item.FileName) || item.Content == null || item.Content.Length == 0)
                throw new ValidationException(Error.FileRequired);

            var para = db.Parameters.Where(t => t.Code == ParameterCode.ApplicationAttachmentMaxSize).Select(t => new
            {
                t.Code,
                t.Value
            }).ToList();

            if (item.Content.Length > int.Parse(para.First(t => t.Code == ParameterCode.ApplicationAttachmentMaxSize).Value))
                throw new ValidationException(Error.FileMaxSizeExceeded);
        }

        public static class Error
        {
            public const string FileRequired = "applicationAttachment.fileRequired";
            public const string FileMaxSizeExceeded = "applicationAttachment.maxSizeExceeded";
            public const string NumberRequired = "applicationAttachment.numberRequired";
        }
    }
}
