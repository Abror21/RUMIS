using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Mappers;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public class DocumentTemplateService : IDocumentTemplateService
    {
        private readonly IAppDbContext db;
        private readonly IFileService fileService;
        private readonly IDocumentTemplateValidator validator;
        private readonly ICurrentUserProfileService currentUserProfile;
        private readonly IAuthorizationService authorizationService;

        public DocumentTemplateService(
            IAppDbContext db,
            IFileService fileService,
            IDocumentTemplateValidator validator,
            ICurrentUserProfileService currentUserProfile,
            IAuthorizationService authorizationService)
        {
            this.db = db;
            this.fileService = fileService;
            this.validator = validator;
            this.currentUserProfile = currentUserProfile;
            this.authorizationService = authorizationService;
        }


        /// <inheritdoc/>
        /// <exception cref="ValidationException"></exception>
        public async Task<int> CreateAsync(DocumentTemplateEditDto item, CancellationToken cancellationToken = default)
        {
            authorizationService.Authorize((IAuthorizedDocumentTemplateEditDto)item);

            var entity = new DocumentTemplate
            {
                FileId = Guid.NewGuid(),
                FileName = item.File?.FileName
            };

            DocumentTemplateMapper.Map(item, entity);

            entity.PermissionType = entity.SupervisorId == null ? UserProfileType.Country : UserProfileType.Supervisor;
            var fileRequired = (entity.Code != DocumentType.Hyperlink);

            await validator.ValidateAsync(entity);

            if (fileRequired)
                await validator.ValidateFileAsync(item.File, cancellationToken);

            await db.DocumentTemplates.AddAsync(entity, cancellationToken);
            if (fileRequired)
                await fileService.AddOrUpdateAsync(entity.FileId, item.File);

            await db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await db.DocumentTemplates.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            db.DocumentTemplates.Remove(entity);

            await fileService.DeleteAsync(entity.FileId);

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public SetQuery<DocumentTemplate> Get()
        {
            return new SetQuery<DocumentTemplate>(db.DocumentTemplates.AsNoTracking());
        }

        /// <inheritdoc/>
        public SetQuery<DocumentTemplate> GetByEducationalInstitution(int eduInstId)
        {
            var date = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = db.DocumentTemplates.Where(t => (!t.ValidFrom.HasValue || t.ValidFrom.Value <= date)
                                                        && (!t.ValidTo.HasValue || t.ValidTo.Value >= date));

            var predicates = new Stack<Expression<Func<DocumentTemplate, bool>>>();

            if (!currentUserProfile.IsInitialized)
            {
                predicates.Push(t =>
                    t.PermissionType == UserProfileType.EducationalInstitution
                    && t.EducationalInstitutionId == eduInstId);

                predicates.Push(t =>
                    t.PermissionType == UserProfileType.Supervisor
                    && t.Supervisor.EducationalInstitutions.Any(e => e.Id == eduInstId));
            }
            else
            {
                switch (currentUserProfile.Type)
                {
                    case UserProfileType.Supervisor:
                        predicates.Push(t =>
                            t.PermissionType == UserProfileType.Supervisor
                            && t.SupervisorId == currentUserProfile.SupervisorId);
                        break;

                    case UserProfileType.EducationalInstitution:
                        predicates.Push(t =>
                            t.PermissionType == UserProfileType.EducationalInstitution
                            && t.EducationalInstitutionId == currentUserProfile.EducationalInstitutionId);

                        predicates.Push(t =>
                            t.PermissionType == UserProfileType.Supervisor
                            && t.Supervisor.EducationalInstitutions.Any(e => e.Id == currentUserProfile.EducationalInstitutionId));

                        break;

                    default:
                        break;
                }
            }

            foreach (var predicate in predicates)
                if (query.Any(predicate))
                    return new SetQuery<DocumentTemplate>(query.Where(predicate));

            return new SetQuery<DocumentTemplate>(query.Where(t => t.PermissionType == UserProfileType.Country));
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task<string> GetSampleAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await db.DocumentTemplates.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            var template = await fileService.GetAsync(entity.FileId);
            var templateHtml = Encoding.UTF8.GetString(template.Content);

            var values = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.Placeholder)
                .Select(t => new { t.Code, t.Payload })
                .ToDictionaryAsync(
                    t => t.Code,
                    t => (object)JsonSerializer.Deserialize<PlaceholderPayload>(t.Payload).Value, cancellationToken);

            return HtmlTemplateParser.Parse(templateHtml, values);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="ValidationException"></exception>
        public async Task UpdateAsync(int id, DocumentTemplateEditDto item, CancellationToken cancellationToken = default)
        {
            authorizationService.Authorize((IAuthorizedDocumentTemplateEditDto)item);

            var entity = await db.DocumentTemplates.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            DocumentTemplateMapper.Map(item, entity);

            entity.PermissionType = entity.SupervisorId == null ? UserProfileType.Country : UserProfileType.Supervisor;

            var fileRequired = (entity.Code != DocumentType.Hyperlink);

            await validator.ValidateAsync(entity);

            entity.FileId = await UpdateFile(item.File, entity.FileId, fileRequired);

            await db.SaveChangesAsync(cancellationToken);
        }


        private async Task<Guid> UpdateFile(FileDto file, Guid fileId, bool fileRequired = false, CancellationToken cancellationToken = default)
        {
            if (fileRequired || (file != null && file.HasValue)) // file content is or must be provided - update file
            {
                if (fileId == Guid.Empty)
                    fileId = Guid.NewGuid();
                await validator.ValidateFileAsync(file, cancellationToken);
                await fileService.AddOrUpdateAsync(fileId, file);
            }
            else if (file == null) // null dto - remove the file
            {
                if (fileId != Guid.Empty)
                    await fileService.DeleteAsync(fileId);
                return Guid.Empty;
            }
            return fileId;
        }
    }
}
