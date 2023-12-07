using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Domain.Constants.Classifiers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public class ApplicationDuplicateService : IApplicationDuplicateService
    {
        private readonly IAppDbContext db;
        private readonly IGdprAuditService gdprAuditService;

        public ApplicationDuplicateService(
            IAppDbContext db,
            IGdprAuditService gdprAuditService)
        {
            this.db = db;
            this.gdprAuditService = gdprAuditService;
        }

        /// <inheritdoc/>
        public async Task CheckApplicationsDuplicatesAsync(IEnumerable<Guid> applicationIds, CancellationToken cancellationToken = default)
        {
            var applications = await db.Applications
                .Where(t => applicationIds.Contains(t.Id) 
                            && (ApplicationStatus.ActiveStatuses.Contains(t.ApplicationStatus.Code) 
                                || t.ApplicationResources.Any(ar => PnaStatus.ActiveStatuses.Contains(ar.PNAStatus.Code)) 
                                || t.ApplicationDuplicateId.HasValue))
                .Include(t => t.ApplicationResources)
                .Include(t => t.ApplicationStatus)
                .ToArrayAsync(cancellationToken);

            foreach (var application in applications)
            {
                if (ApplicationStatus.ActiveStatuses.Contains(application.ApplicationStatus.Code)
                    || application.ApplicationResources.Any(ar => PnaStatus.ActiveStatuses.Contains(ar.PNAStatus.Code)))
                {
                    var duplicate = await db.Applications
                        .Where(t => t.ResourceSubTypeId == application.ResourceSubTypeId
                                    && t.ResourceTargetPersonId == application.ResourceTargetPersonId
                                    && t.EducationalInstitutionId != application.EducationalInstitutionId
                                    && (ApplicationStatus.ActiveStatuses.Contains(t.ApplicationStatus.Code)
                                            || t.ApplicationResources.Any(ar => PnaStatus.ActiveStatuses.Contains(ar.PNAStatus.Code))))
                        .FirstOrDefaultAsync(cancellationToken);

                    if (application.ApplicationDuplicateId.HasValue && (duplicate == null || duplicate.Id != application.ApplicationDuplicateId))
                    {
                        var currentDuplicate = await db.Applications.FindAsync(new object[] { application.ApplicationDuplicateId }, cancellationToken);

                        currentDuplicate.ApplicationDuplicateId = null;

                        application.ApplicationDuplicateId = null;
                    }

                    if (duplicate != null)
                    {
                        duplicate.ApplicationDuplicateId = application.Id;
                        application.ApplicationDuplicateId = duplicate.Id;
                    }
                }
                else if (application.ApplicationDuplicateId.HasValue)
                {
                    var currentDuplicate = await db.Applications.FindAsync(new object[] { application.ApplicationDuplicateId }, cancellationToken);

                    currentDuplicate.ApplicationDuplicateId = null;

                    application.ApplicationDuplicateId = null;
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
