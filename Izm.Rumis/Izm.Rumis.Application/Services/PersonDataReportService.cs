using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Mappers;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public sealed class PersonDataReportService : IPersonDataReportService
    {
        private readonly IAppDbContext db;
        private readonly ICurrentUserProfileService currentUserProfileService;
        private readonly IGdprAuditService gdprAuditService;
        private readonly IPersonDataReportValidator validator;

        public PersonDataReportService(IAppDbContext db, ICurrentUserProfileService currentUserProfileService, IGdprAuditService gdprAuditService, IPersonDataReportValidator validator)
        {
            this.db = db;
            this.currentUserProfileService = currentUserProfileService;
            this.gdprAuditService = gdprAuditService;
            this.validator = validator;
        }

        /// <inheritdoc/>
        public async Task<SetQuery<GdprAudit>> GenerateAsync(PersonDataReportGenerateDto item, CancellationToken cancellationToken = default)
        {
            validator.Validate(item);

            var entity = new PersonDataReport
            {
                UserProfileId = currentUserProfileService.Id
            };

            db.PersonDataReports.Add(PersonDataReportMapper.Map(item, entity));

            await gdprAuditService.TraceRangeAsync(GdprAuditHelper.GenerateTracesForGenerateOperation(item), cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            var query = db.GdprAudits.AsNoTracking();

            if (!string.IsNullOrEmpty(item.DataHandlerPrivatePersonalIdentifier))
                query = query.Where(t => t.DataHandlerPrivatePersonalIdentifier == item.DataHandlerPrivatePersonalIdentifier);

            if (!string.IsNullOrEmpty(item.DataOwnerPrivatePersonalIdentifier))
                query = query.Where(t => t.DataOwnerPrivatePersonalIdentifier == item.DataOwnerPrivatePersonalIdentifier);

            switch (currentUserProfileService.Type)
            {
                case UserProfileType.EducationalInstitution:
                    query = query.Where(t => t.EducationalInstitutionId == currentUserProfileService.EducationalInstitutionId);
                    break;

                case UserProfileType.Supervisor:
                    query = query.Where(t => t.SupervisorId == currentUserProfileService.SupervisorId
                        || t.EducationalInstitution.SupervisorId == currentUserProfileService.SupervisorId);
                    break;

                case UserProfileType.Country:
                default:
                    break;
            }

            return new SetQuery<GdprAudit>(query);
        }

        public static class GdprAuditHelper
        {
            public static IEnumerable<GdprAuditTraceDto> GenerateTracesForGenerateOperation(PersonDataReportGenerateDto item)
            {
                var result = new List<GdprAuditTraceDto>();

                if (!string.IsNullOrEmpty(item.DataOwnerPrivatePersonalIdentifier))
                    result.Add(new GdprAuditTraceDto
                    {
                        Action = "personDataReport.generate",
                        DataOwnerPrivatePersonalIdentifier = item.DataOwnerPrivatePersonalIdentifier,
                        Data = new PersonDataProperty[]
                        {
                            new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = item.DataOwnerPrivatePersonalIdentifier }
                        }
                    });

                if (!string.IsNullOrEmpty(item.DataHandlerPrivatePersonalIdentifier))
                    result.Add(new GdprAuditTraceDto
                    {
                        Action = "personDataReport.generate",
                        DataOwnerPrivatePersonalIdentifier = item.DataHandlerPrivatePersonalIdentifier,
                        Data = new PersonDataProperty[]
                        {
                            new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = item.DataHandlerPrivatePersonalIdentifier }
                        }
                    });

                return result;
            }
        }
    }
}
