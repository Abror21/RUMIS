using Izm.Rumis.Application;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Helpers;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Events.Application;
using Izm.Rumis.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Notifications.EventHandlers.ApplicationPersonStatusChanged
{
    public sealed class NotifyContactPersonEventHandler : INotificationHandler<ApplicationPersonStatusChangedEvent>
    {
        private readonly string[] supportedStatuses = new string[]
        {
            ResourceTargetPersonEducationalStatus.NotStudying,
            ResourceTargetPersonWorkStatus.NotWorking
        };

        private readonly NotificationOptions options;
        private readonly IAppDbContext db;
        private readonly IEmailService emailService;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<NotifyContactPersonEventHandler> logger;

        public NotifyContactPersonEventHandler(
            NotificationOptions options,
            IAppDbContext db,
            IEmailService emailService,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.options = options;
            this.db = db;
            this.emailService = emailService;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Handle(ApplicationPersonStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            if (!options.Enabled || options.Ignore)
                return;

            try
            {
                await HandleAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send notification(-s) for ApplicationPersonStatusChangedEvent. ApplicationId:{applicationId} | StatusId:{statusId}.", notification.ApplicationId, notification.StatusId);
            }
        }

        private async Task HandleAsync(ApplicationPersonStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            var classifier = await db.Classifiers.FindAsync(new object[] { notification.StatusId }, cancellationToken);

            if (!supportedStatuses.Contains(classifier.Code))
                return;

            var application = db.Applications.Local
                .First(t => t.Id == notification.ApplicationId);

            if (!application.ApplicationResources.Any(t => PnaStatus.ActiveStatuses.Contains(t.PNAStatus.Code)))
                return;

            var contactPersons = application
                .GetApplicationResource()
                .ApplicationResourceContactPersons
                .Where(t => t.EducationalInstitutionContactPerson.JobPosition.Code == EducationalInstitutionJobPosition.IctContact)
                .Select(t => t.EducationalInstitutionContactPerson.Email)
                .ToArray();

            var (subject, body) = await CreateMessageAsync(application, classifier.Value, cancellationToken);

            if (subject == null || body == null)
                return;

            // This is done because gdprAuditService internally also calls SaveChangesAsync()
            // which might lead to an infinite loop.
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var gdprAuditService = scope.ServiceProvider.GetRequiredService<IGdprAuditService>();

                await gdprAuditService.TraceRangeAsync(
                    contactPersons.Select(GdprAuditHelper.ProjectTraces(application.EducationalInstitutionId, notification)),
                    cancellationToken
                    );
            }

            foreach (var contactPerson in contactPersons)
                await emailService.SendAsync(
                    message: emailService.CreateMessage(contactPerson, subject, body)
                    );
        }

        private async Task<(string, string)> CreateMessageAsync(Domain.Entities.Application application, string status, CancellationToken cancellationToken = default)
        {
            var templates = await db.TextTemplates
                .Where(t => t.Code == TextTemplateCode.ApplicationResourceTargetStatusChangedNotificationSubject
                    || t.Code == TextTemplateCode.ApplicationResourceTargetStatusChangedNotificationBody)
                .Select(t => new { t.Code, t.Content })
                .ToArrayAsync(cancellationToken);

            var propertyMap = ApplicationTextTemplateHelper.CreatePropertyMap(application);

            propertyMap.Add("ResourceTargetPersonStatus", status);

            var subject = TextTemplateParser.Parse(templates.First(t => t.Code == TextTemplateCode.ApplicationResourceTargetStatusChangedNotificationSubject).Content, propertyMap);
            var body = TextTemplateParser.Parse(templates.First(t => t.Code == TextTemplateCode.ApplicationResourceTargetStatusChangedNotificationBody).Content, propertyMap);

            return (subject, body);
        }

        public static class GdprAuditHelper
        {
            public static Func<string, GdprAuditTraceDto> ProjectTraces(int educationalInstitutionId, ApplicationPersonStatusChangedEvent notification)
            {
                return email => new GdprAuditTraceDto
                {
                    Action = "email.ApplicationPersonStatusChanged",
                    ActionData = JsonSerializer.Serialize(notification),
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.Contact, Value = email }
                    },
                    EducationalInstitutionId = educationalInstitutionId
                };
            }
        }
    }
}
