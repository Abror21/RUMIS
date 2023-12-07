using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Helpers;
using Izm.Rumis.Application.Mappers;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{

    public class ApplicationResourceService : IApplicationResourceService
    {
        private readonly IAppDbContext db;
        private readonly ISequenceService sequenceService;
        private readonly ICurrentUserProfileService currentUserProfile;
        private readonly IApplicationResourceValidator validator;
        private readonly IFileService fileService;
        private readonly IAuthorizationService authorizationService;
        private readonly IDocumentTemplateService documentTemplateService;
        private readonly IApplicationDuplicateService applicationDuplicateService;

        public ApplicationResourceService(
            IAppDbContext db,
            ICurrentUserProfileService currentUserProfile,
            IApplicationResourceValidator validator,
            IFileService fileService,
            ISequenceService sequenceService,
            IAuthorizationService authorizationService,
            IDocumentTemplateService documentTemplateService,
            IApplicationDuplicateService applicationDuplicateService)
        {
            this.db = db;
            this.currentUserProfile = currentUserProfile;
            this.validator = validator;
            this.fileService = fileService;
            this.sequenceService = sequenceService;
            this.authorizationService = authorizationService;
            this.documentTemplateService = documentTemplateService;
            this.applicationDuplicateService = applicationDuplicateService;
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ChangeStatusToLostAsync(Guid id, ApplicationResourceChangeStatusDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources
                .Include(t => t.PNAStatus)
                .Include(t => t.ApplicationResourceAttachmentList)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity.Application.EducationalInstitutionId);

            if (entity.PNAStatus.Code != PnaStatus.Issued && entity.PNAStatus.Code != PnaStatus.Lost)
                throw new InvalidOperationException(Error.IncorrectPNAStatus);

            if (entity.PNAStatus.Code == PnaStatus.Issued)
            {
                var lostStatusId = await db.Classifiers
                    .Where(t => t.Code == PnaStatus.Lost && t.Type == ClassifierTypes.PnaStatus)
                    .Select(t => t.Id)
                    .FirstAsync();

                entity.SetPnaStatus(lostStatusId);
            }

            entity.Notes = item.Notes;

            if (item.FileToDeleteIds != null)
            {
                db.ApplicationResourceAttachments.RemoveRange(entity.ApplicationResourceAttachmentList
                .Where(t => item.FileToDeleteIds.Any(n => n == t.FileId)));

                await fileService.DeleteRangeAsync(item.FileToDeleteIds, cancellationToken);
            }

            if (item.Files != null)
                foreach (var file in item.Files)
                {
                    var fileId = Guid.NewGuid();

                    entity.ApplicationResourceAttachmentList.Add(new ApplicationResourceAttachment
                    {
                        ApplicationResourceId = id,
                        DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        FileId = fileId
                    });

                    await fileService.AddOrUpdateAsync(fileId, file);
                }

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(new[] { entity.ApplicationId }, cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task ChangeStatusToPreparedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources
                .Include(t => t.PNAStatus)
                .Include(t => t.AssignedResource)
                    .ThenInclude(r => r.ResourceStatus)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity.Application.EducationalInstitutionId);

            if (entity.PNAStatus.Code != PnaStatus.Preparing)
                throw new InvalidOperationException(Error.IncorrectPNAStatus);

            if (entity.AssignedResource.ResourceStatus.Code != ResourceStatus.Reserved)
                throw new InvalidOperationException(Error.IncorrectResourceStatus);

            var preparedStatusId = await db.Classifiers
                .Where(t => t.Code == PnaStatus.Prepared && t.Type == ClassifierTypes.PnaStatus)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);

            entity.SetPnaStatus(preparedStatusId);

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(new[] { entity.ApplicationId }, cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ChangeStatusToStolenAsync(Guid id, ApplicationResourceChangeStatusDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources
                .Include(t => t.PNAStatus)
                .Include(t => t.ApplicationResourceAttachmentList)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity.Application.EducationalInstitutionId);

            if (entity.PNAStatus.Code != PnaStatus.Issued && entity.PNAStatus.Code != PnaStatus.Stolen)
                throw new InvalidOperationException(Error.IncorrectPNAStatus);

            if (entity.PNAStatus.Code == PnaStatus.Issued)
            {
                var stolenStatusId = await db.Classifiers
                    .Where(t => t.Code == PnaStatus.Stolen && t.Type == ClassifierTypes.PnaStatus)
                    .Select(t => t.Id)
                    .FirstAsync();

                entity.SetPnaStatus(stolenStatusId);
            }

            entity.Notes = item.Notes;

            if (item.FileToDeleteIds != null)
            {
                db.ApplicationResourceAttachments.RemoveRange(entity.ApplicationResourceAttachmentList
                .Where(t => item.FileToDeleteIds.Any(n => n == t.FileId)));

                foreach (var fileToDeleteId in item.FileToDeleteIds)
                    await fileService.DeleteAsync(fileToDeleteId);
            }

            if (item.Files != null)
                foreach (var file in item.Files)
                {
                    var fileId = Guid.NewGuid();

                    entity.ApplicationResourceAttachmentList.Add(new ApplicationResourceAttachment
                    {
                        ApplicationResourceId = id,
                        DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        FileId = fileId
                    });

                    await fileService.AddOrUpdateAsync(fileId, file);
                }

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(new[] { entity.ApplicationId }, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Guid> CreateWithDraftStatusAsync(ApplicationResourceCreateDto item, CancellationToken cancellationToken = default)
        {
            var application = await db.Applications
                .Include(t => t.ApplicationStatus)
                .Include(t => t.EducationalInstitution)
                .FirstOrDefaultAsync(a => a.Id == item.ApplicationId, cancellationToken);

            if (application == null ||
                (application.ApplicationStatus.Code != ApplicationStatus.Submitted &&
                application.ApplicationStatus.Code != ApplicationStatus.Postponed &&
                application.ApplicationStatus.Code != ApplicationStatus.Confirmed))
                throw new InvalidOperationException(Error.IncorrectApplicationStatus);

            authorizationService.Authorize(application.EducationalInstitutionId);

            var resource = await db.Resources
                .Include(t => t.ResourceStatus)
                .FirstOrDefaultAsync(r => r.Id == item.AssignedResourceId, cancellationToken);

            if (resource == null ||
                (resource.ResourceStatus.Code != ResourceStatus.Available))
                throw new InvalidOperationException(Error.IncorrectResourceStatus);

            if (await db.ApplicationResources
                .Where(ar => ar.ApplicationId == item.ApplicationId)
                .AnyAsync(ar => ar.PNAStatus.Code == PnaStatus.Preparing
                             || ar.PNAStatus.Code == PnaStatus.Prepared
                             || ar.PNAStatus.Code == PnaStatus.Issued, cancellationToken))
                throw new InvalidOperationException(Error.ApplicationAlreadyExists);

            await validator.ValidateAsync(item);

            var preparingStatusId = await db.Classifiers
                .Where(t => t.Code == PnaStatus.Preparing && t.Type == ClassifierTypes.PnaStatus)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);

            var serialNumberWithinInsitution = sequenceService.GetByKey(NumberingPatternHelper.ApplicationResourcesKeyFormat(application.EducationalInstitution.Code));

            var entity = new ApplicationResource
            {
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(application.EducationalInstitution.Code, serialNumberWithinInsitution),
            };

            ApplicationResourceMapper.Map(item, entity);

            entity.SetApplicationResourceReturnDeadline(item.AssignedResourceReturnDate);
            entity.SetPnaStatus(preparingStatusId);

            db.ApplicationResources.Add(entity);

            if (item.EducationalInstitutionContactPersonIds != null)
                await db.ApplicationResourceContactPersons.AddRangeAsync(item.EducationalInstitutionContactPersonIds.Select(t => new ApplicationResourceContactPerson
                {
                    ApplicationResourceId = entity.Id,
                    EducationalInstitutionContactPersonId = t
                }), cancellationToken);

            var confirmedApplicationStatusId = await db.Classifiers
                .Where(t => t.Code == ApplicationStatus.Confirmed && t.Type == ClassifierTypes.ApplicationStatus)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);

            if (application.ApplicationStatusId != confirmedApplicationStatusId)
                application.SetApplicationStatus(confirmedApplicationStatusId);

            var reservedResourceStatusId = await db.Classifiers
                .Where(t => t.Code == ResourceStatus.Reserved && t.Type == ClassifierTypes.ResourceStatus)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);

            resource.ResourceStatusId = reservedResourceStatusId;

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(new[] { entity.ApplicationId }, cancellationToken);

            return entity.Id;
        }

        /// <inheritdoc/>
        public async Task<Guid> CreateWithPreparedStatusAsync(ApplicationResourceCreateDto item, CancellationToken cancellationToken = default)
        {
            var application = await db.Applications
                .Include(t => t.ApplicationStatus)
                .Include(t => t.EducationalInstitution)
                .FirstOrDefaultAsync(a => a.Id == item.ApplicationId, cancellationToken);

            if (application == null ||
                (application.ApplicationStatus.Code != ApplicationStatus.Submitted &&
                application.ApplicationStatus.Code != ApplicationStatus.Postponed &&
                application.ApplicationStatus.Code != ApplicationStatus.Confirmed))
                throw new InvalidOperationException(Error.IncorrectApplicationStatus);

            authorizationService.Authorize(application.EducationalInstitutionId);

            var resource = await db.Resources
                .Include(t => t.ResourceStatus)
                .FirstOrDefaultAsync(r => r.Id == item.AssignedResourceId, cancellationToken);

            if (resource == null || resource.ResourceStatus.Code != ResourceStatus.Available)
                throw new InvalidOperationException(Error.IncorrectResourceStatus);

            if (await db.ApplicationResources
                .Where(ar => ar.ApplicationId == item.ApplicationId)
                .AnyAsync(ar => ar.PNAStatus.Code == PnaStatus.Preparing
                             || ar.PNAStatus.Code == PnaStatus.Prepared
                             || ar.PNAStatus.Code == PnaStatus.Issued, cancellationToken))
                throw new InvalidOperationException(Error.ApplicationAlreadyExists);

            await validator.ValidateAsync(item);

            var preparedStatusId = await db.Classifiers
                .Where(t => t.Code == PnaStatus.Prepared && t.Type == ClassifierTypes.PnaStatus)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);

            var serialNumberWithinInsitution = sequenceService.GetByKey(NumberingPatternHelper.ApplicationResourcesKeyFormat(application.EducationalInstitution.Code));

            var entity = new ApplicationResource
            {
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(application.EducationalInstitution.Code, serialNumberWithinInsitution),
            };

            ApplicationResourceMapper.Map(item, entity);

            entity.SetApplicationResourceReturnDeadline(item.AssignedResourceReturnDate);
            entity.SetPnaStatus(preparedStatusId);

            db.ApplicationResources.Add(entity);

            if (item.EducationalInstitutionContactPersonIds != null)
                await db.ApplicationResourceContactPersons.AddRangeAsync(item.EducationalInstitutionContactPersonIds.Select(t => new ApplicationResourceContactPerson
                {
                    ApplicationResourceId = entity.Id,
                    EducationalInstitutionContactPersonId = t
                }), cancellationToken);

            var confirmedApplicationStatusId = await db.Classifiers
                .Where(t => t.Code == ApplicationStatus.Confirmed && t.Type == ClassifierTypes.ApplicationStatus)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);

            if (application.ApplicationStatusId != confirmedApplicationStatusId)
                application.SetApplicationStatus(confirmedApplicationStatusId);

            var reservedResourceStatusId = await db.Classifiers
                .Where(t => t.Code == ResourceStatus.Reserved && t.Type == ClassifierTypes.ResourceStatus)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);

            resource.ResourceStatusId = reservedResourceStatusId;

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(new[] { entity.ApplicationId }, cancellationToken);

            return entity.Id;
        }

        /// <inheritdoc/>
        public SetQuery<ApplicationResource> Get()
        {
            var query = db.ApplicationResources.AsNoTracking();

            switch (currentUserProfile.Type)
            {
                case UserProfileType.Supervisor:
                    query = query.Where(t => t.Application.EducationalInstitution.SupervisorId == currentUserProfile.SupervisorId);
                    break;

                case UserProfileType.EducationalInstitution:
                    query = query.Where(t => t.Application.EducationalInstitutionId == currentUserProfile.EducationalInstitutionId);
                    break;

                case UserProfileType.Country:
                default:
                    break;
            }

            return new SetQuery<ApplicationResource>(query);
        }

        /// <inheritdoc/>
        public async Task<string> GetExploitationRulesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity.Application.EducationalInstitutionId);

            var documentTemplateFileId = await documentTemplateService
                .GetByEducationalInstitution(entity.Application.EducationalInstitutionId)
                .Where(t => t.Code == DocumentType.ExploitationRules)
                .FirstAsync(t => t.FileId, cancellationToken);

            var template = await fileService.GetAsync(documentTemplateFileId);

            return Encoding.UTF8.GetString(template.Content);
        }

        /// <inheritdoc/>
        public async Task<FileDto> GetExploitationRulesPdfAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity.Application.EducationalInstitutionId);

            var documentTemplateFileId = await documentTemplateService
                .GetByEducationalInstitution(entity.Application.EducationalInstitutionId)
                .Where(t => t.Code == DocumentType.ExploitationRules)
                .FirstAsync(t => t.FileId, cancellationToken);

            var template = await fileService.GetAsync(documentTemplateFileId);

            return new FileDto
            {
                FileName = $"{entity.Id}.pdf",
                ContentType = MediaTypeNames.Application.Pdf,
                Content = fileService.HtmlToPdf(Encoding.UTF8.GetString(template.Content))
            };
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task<string> GetPnaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources
                .Include(t => t.Application)
                .Include(t => t.ApplicationResourceAttachmentList)
                    .ThenInclude(t => t.DocumentTemplate)
                .Include(t => t.ApplicationResourceAttachmentList)
                    .ThenInclude(t => t.DocumentType) 
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity.Application.EducationalInstitutionId);

            var pna = entity.ApplicationResourceAttachmentList
                .FirstOrDefault(t => t.DocumentType.Code == DocumentType.PNA);

            if (pna != null && pna.FileId.HasValue)
                return Encoding.UTF8.GetString((await fileService.GetAsync(pna.FileId.Value)).Content);

            var documentTemplateFileId = pna != null ? pna.DocumentTemplate.FileId : await documentTemplateService
                    .GetByEducationalInstitution(entity.Application.EducationalInstitutionId)
                    .Where(t => t.Code == DocumentType.PNA)
                    .FirstAsync(t => t.FileId, cancellationToken);

            var template = await fileService.GetAsync(documentTemplateFileId);

            var html = HtmlTemplateParser.Parse(
                Encoding.UTF8.GetString(template.Content),
                ApplicationResourceHtmlTemplateHelper.CreateProperyMap(entity));

            if (pna != null)
            {
                pna.FileId = Guid.NewGuid();

                await fileService.AddOrUpdateAsync(pna.FileId.Value, new FileDto
                {
                    FileName = $"{pna.Id}.html",
                    ContentType = MediaTypeNames.Text.Html,
                    Content = Encoding.UTF8.GetBytes(html),
                    SourceType = FileSourceType.S3
                });

                await db.SaveChangesAsync(cancellationToken);
            }

            return html;
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task<FileDto> GetPnaPdfAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources
                .Include(t => t.Application)
                .Include(t => t.ApplicationResourceAttachmentList)
                    .ThenInclude(t => t.DocumentTemplate)
                .Include(t => t.ApplicationResourceAttachmentList)
                    .ThenInclude(t => t.DocumentType)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity.Application.EducationalInstitutionId);

            var file = new FileDto
            {
                FileName = $"{entity.PNANumber}.pdf",
                ContentType = MediaTypeNames.Application.Pdf
            };

            var pna = entity.ApplicationResourceAttachmentList
                .FirstOrDefault(t => t.DocumentType.Code == DocumentType.PNA);

            if (pna != null && pna.FileId.HasValue)
            {
                file.Content = fileService.HtmlToPdf(
                        Encoding.UTF8.GetString((await fileService.GetAsync(pna.FileId.Value)).Content));

                return file;
            }

            var documentTemplateFileId = pna != null ? pna.DocumentTemplate.FileId : await documentTemplateService
                .GetByEducationalInstitution(entity.Application.EducationalInstitutionId)
                .Where(t => t.Code == DocumentType.PNA)
                .FirstAsync(t => t.FileId, cancellationToken);

            var template = await fileService.GetAsync(documentTemplateFileId);

            var html = HtmlTemplateParser.Parse(
                Encoding.UTF8.GetString(template.Content),
                ApplicationResourceHtmlTemplateHelper.CreateProperyMap(entity));

            if (pna != null)
            {
                pna.FileId = Guid.NewGuid();

                await fileService.AddOrUpdateAsync(pna.FileId.Value, new FileDto
                {
                    FileName = $"{pna.Id}.html",
                    ContentType = MediaTypeNames.Text.Html,
                    Content = Encoding.UTF8.GetBytes(html),
                    SourceType = FileSourceType.S3
                });

                await db.SaveChangesAsync(cancellationToken);
            }

            file.Content = fileService.HtmlToPdf(html);
            return file;
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task UpdateAsync(Guid id, ApplicationResourceUpdateDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources
                .Include(t => t.PNAStatus)
                .Include(t => t.Application)
                .Include(t => t.AssignedResource)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            if (entity.PNAStatus.Code != PnaStatus.Prepared && entity.PNAStatus.Code != PnaStatus.Preparing)
                throw new InvalidOperationException(Error.IncorrectPNAStatus);

            authorizationService.Authorize(entity.Application.EducationalInstitutionId);

            if (entity.AssignedResourceId != item.AssignedResourceId)
            {
                var newResource = await db.Resources
                    .Include(t => t.ResourceStatus)
                    .FirstOrDefaultAsync(t => t.Id == item.AssignedResourceId, cancellationToken);

                if (newResource.ResourceStatus.Code != ResourceStatus.Available)
                    throw new InvalidOperationException(Error.IncorrectResourceStatus);

                var availableStatusId = await db.Classifiers
                    .Where(t => t.Code == ResourceStatus.Available && t.Type == ClassifierTypes.ResourceStatus)
                    .Select(t => t.Id)
                    .FirstAsync(cancellationToken);

                entity.AssignedResource.ResourceStatusId = availableStatusId;

                var reservedStatusId = await db.Classifiers
                    .Where(t => t.Code == ResourceStatus.Reserved && t.Type == ClassifierTypes.ResourceStatus)
                    .Select(t => t.Id)
                    .FirstAsync(cancellationToken);

                newResource.ResourceStatusId = reservedStatusId;

                entity.AssignedResource = newResource;
            }

            entity.AssignedResource.Notes = item.Notes;
            entity.SetApplicationResourceReturnDeadline(item.AssignedResourceReturnDate);

            await db.ApplicationResourceContactPersons.AddRangeAsync(
                item.EducationalInstitutionContactPersonIds
                    .Except(entity.ApplicationResourceContactPersons.Select(t => t.EducationalInstitutionContactPersonId))
                    .Select(t => new ApplicationResourceContactPerson
                    {
                        ApplicationResourceId = entity.Id,
                        EducationalInstitutionContactPersonId = t
                    }), cancellationToken);

            db.ApplicationResourceContactPersons.RemoveRange(
                entity.ApplicationResourceContactPersons
                    .ExceptBy(item.EducationalInstitutionContactPersonIds, t => t.EducationalInstitutionContactPersonId));

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ReturnAsync(Guid id, ApplicationResourceReturnEditDto item, CancellationToken cancellationToken = default)
        {
            await validator.ValidateResourceStatusAsync(item, cancellationToken);

            var entity = await db.ApplicationResources
                .Include(t => t.PNAStatus)
                .Include(t => t.AssignedResource)
                    .ThenInclude(r => r.ResourceStatus)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity.Application.EducationalInstitutionId);

            if (entity.PNAStatus.Code != PnaStatus.Issued)
                throw new InvalidOperationException(Error.IncorrectPNAStatus);

            if (entity.AssignedResource.ResourceStatus.Code != ResourceStatus.InUse)
                throw new InvalidOperationException(Error.IncorrectResourceStatus);

            var returnedStatusId = await db.Classifiers
                .Where(t => t.Code == PnaStatus.Returned && t.Type == ClassifierTypes.PnaStatus)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);

            entity.SetPnaStatus(returnedStatusId);

            ApplicationResourceMapper.Map(item, entity);

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(new[] { entity.ApplicationId }, cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SetReturnDeadlineAsync(ApplicationResourceReturnDeadlineDto item, CancellationToken cancellationToken = default)
        {
            var applicationResources = await db.ApplicationResources
                .Include(t => t.PNAStatus)
                .Where(t => item.ApplicationResourceIds.Contains(t.Id))
                .ToArrayAsync(cancellationToken);

            if (applicationResources.Count() != item.ApplicationResourceIds.Count())
                throw new EntityNotFoundException();

            foreach (var entity in applicationResources)
            {
                if (entity.PNAStatus.Code != PnaStatus.Preparing && entity.PNAStatus.Code != PnaStatus.Prepared && entity.PNAStatus.Code != PnaStatus.Issued)
                    throw new InvalidOperationException(Error.IncorrectPNAStatus);

                entity.SetApplicationResourceReturnDeadline(item.AssignedResourceReturnDate);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SignAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources
                .Include(t => t.PNAStatus)
                .Include(t => t.AssignedResource)
                    .ThenInclude(r => r.ResourceStatus)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity.Application.EducationalInstitutionId);

            if (entity.PNAStatus.Code != PnaStatus.Prepared)
                throw new InvalidOperationException(Error.IncorrectPNAStatus);

            if (entity.AssignedResource.ResourceStatus.Code != ResourceStatus.Reserved)
                throw new InvalidOperationException(Error.IncorrectResourceStatus);

            var issuedStatusId = await db.Classifiers
                .Where(t => t.Code == PnaStatus.Issued && t.Type == ClassifierTypes.PnaStatus)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);

            entity.SetPnaStatus(issuedStatusId);

            var documentTypeId = await db.Classifiers
                .Where(t => t.Code == DocumentType.PNA && t.Type == ClassifierTypes.DocumentType)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);

            var documentDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var documentTemplateId = await documentTemplateService
                .GetByEducationalInstitution(entity.Application.EducationalInstitutionId)
                .Where(t => t.Code == DocumentType.PNA)
                .FirstAsync(t => t.Id, cancellationToken);

            entity.ApplicationResourceAttachmentList.Add(new ApplicationResourceAttachment
            {
                ApplicationResourceId = id,
                DocumentDate = documentDate,
                DocumentTypeId = documentTypeId,
                DocumentTemplateId = documentTemplateId
            });

            var inUseResourceStatusId = await db.Classifiers
                .Where(t => t.Code == ResourceStatus.InUse && t.Type == ClassifierTypes.ResourceStatus)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);

            entity.AssignedResource.ResourceStatusId = inUseResourceStatusId;

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(new[] { entity.ApplicationId }, cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task CancelAsync(Guid id, ApplicationResourceCancelDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources
                .Include(t => t.PNAStatus)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity.Application.EducationalInstitutionId);

            if (entity.PNAStatus.Code != PnaStatus.Prepared && entity.PNAStatus.Code != PnaStatus.Preparing)
                throw new InvalidOperationException(Error.IncorrectPNAStatus);

            var reasonType = await db.Classifiers
               .Where(t => t.Id == item.ReasonId)
               .Select(t => t.Type)
               .FirstOrDefaultAsync(cancellationToken);

            if (reasonType == null || reasonType != ClassifierTypes.PnaCancelingReason)
                throw new InvalidOperationException(Error.IncorrectModelData);

            entity.CancelingReasonId = item.ReasonId;
            entity.CancelingDescription = item.Description;

            if (item.ChangeApplicationStatusToWithdrawn)
            {
                var resource = await db.Resources
                    .FirstOrDefaultAsync(t => t.Id == entity.AssignedResourceId, cancellationToken);

                if (resource != null)
                {
                    var issuedStatusId = await db.Classifiers
                        .Where(t => t.Code == ResourceStatus.Available && t.Type == ClassifierTypes.ResourceStatus)
                        .Select(t => t.Id)
                        .FirstAsync(cancellationToken);

                    resource.ResourceStatusId = issuedStatusId;
                }

                var withdrawnStatusId = await db.Classifiers
                       .Where(t => t.Code == ApplicationStatus.Withdrawn && t.Type == ClassifierTypes.ApplicationStatus)
                       .Select(t => t.Id)
                       .FirstAsync(cancellationToken);

                entity.Application.SetApplicationStatus(withdrawnStatusId);
            }

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(new[] { entity.ApplicationId }, cancellationToken);
        }

        public static class Error
        {
            public const string IncorrectPNAStatus = "applicationResource.incorrectPNAStatus";
            public const string IncorrectApplicationStatus = "applicationResource.incorrectApplicationStatus";
            public const string ApplicationAlreadyExists = "applicationResource.applicationAlreadyExists";
            public const string IncorrectResourceStatus = "applicationResource.incorrectPNAStatus";
            public const string IncorrectModelData = "applicationResource.incorrectModelData";
        }
    }
}
