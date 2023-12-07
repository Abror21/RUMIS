using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public class ApplicationAttachmentService : IApplicationAttachmentService
    {
        private readonly IAppDbContext db;
        private readonly IApplicationAttachmentValidator validator;
        private readonly IFileService fileService;

        public ApplicationAttachmentService(
            IAppDbContext db,
            IApplicationAttachmentValidator validator,
            IFileService fileService)
        {
            this.db = db;
            this.fileService = fileService;
            this.validator = validator;
        }

        /// <inheritdoc/>
        public SetQuery<ApplicationAttachment> Get()
        {
            return new SetQuery<ApplicationAttachment>(db.ApplicationAttachments.AsNoTracking());
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="ValidationException"></exception>
        public async Task<Guid> CreateAsync(ApplicationAttachmentCreateDto item, CancellationToken cancellationToken = default)
        {
            var application = await db.Applications.FindAsync(new object[] { item.ApplicationId }, cancellationToken);

            if (application == null)
                throw new EntityNotFoundException();

            var entity = new ApplicationAttachment
            {
                ApplicationId = item.ApplicationId,
                AttachmentNumber = item.AttachmentNumber,
                AttachmentDate = item.AttachmentDate,
                FileId = Guid.NewGuid()
            };

            validator.Validate(entity);

            validator.ValidateFile(item.File);

            await db.ApplicationAttachments.AddAsync(entity, cancellationToken);

            await fileService.AddOrUpdateAsync(entity.FileId, item.File);

            await db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="ValidationException"></exception>
        public async Task UpdateAsync(Guid id, ApplicationAttachmentUpdateDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationAttachments.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            entity.AttachmentNumber = item.AttachmentNumber;
            entity.AttachmentDate = item.AttachmentDate;

            validator.Validate(entity);

            await UpdateFile(item.File, entity.FileId);

            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task UpdateFile(FileDto file, Guid fileId)
        {
            if (file == null) // null dto - remove the file
            {
                await fileService.DeleteAsync(fileId);
            }
            else
            {
                if (file.HasValue) // file content provided - update file
                {
                    validator.ValidateFile(file);
                    await fileService.AddOrUpdateAsync(fileId, file);
                }
            }
        }
    }
}
