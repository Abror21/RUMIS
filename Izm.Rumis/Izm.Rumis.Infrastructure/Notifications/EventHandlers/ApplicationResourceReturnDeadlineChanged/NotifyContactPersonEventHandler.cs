using Izm.Rumis.Application;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Helpers;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Events.ApplicationResource;
using Izm.Rumis.Domain.Models;
using Izm.Rumis.Infrastructure.EAddress;
using Izm.Rumis.Infrastructure.EAddress.Enums;
using Izm.Rumis.Infrastructure.EAddress.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Notifications.EventHandlers.ApplicationResourceReturnDeadlineChanged
{
    public sealed class NotifyContactPersonEventHandler : INotificationHandler<ApplicationResourceReturnDeadlineChangedEvent>
    {
        private readonly NotificationOptions options;
        private readonly IAppDbContext db;
        private readonly IEmailService emailService;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<NotifyContactPersonEventHandler> logger;

        public NotifyContactPersonEventHandler(
            NotificationOptions options,
            IAppDbContext db,
            IEmailService emailService,
            IServiceScopeFactory serviceScopeFactory,
            IServiceProvider serviceProvider,
            ILogger<NotifyContactPersonEventHandler> logger)
        {
            this.options = options;
            this.db = db;
            this.emailService = emailService;
            this.serviceScopeFactory = serviceScopeFactory;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task Handle(ApplicationResourceReturnDeadlineChangedEvent notification, CancellationToken cancellationToken)
        {
            if (!options.Enabled || options.Ignore)
                return;

            try
            {
                await HandleAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send notification(-s) for ApplicationResourceReturnDeadlineChangedEvent. ApplicationResourceId:{applicationId} | AssignedResourceReturnDate:{statusId}.", notification.ApplicationResourceId, notification.AssignedResourceReturnDate);
            }
        }

        private async Task HandleAsync(ApplicationResourceReturnDeadlineChangedEvent notification, CancellationToken cancellationToken)
        {
            var application = db.Applications.Local
                    .First(t => t.ApplicationResources.Any(n => n.Id == notification.ApplicationResourceId));

            var applicationResourceReturnDate = db.ApplicationResources.Local
                .First(t => t.Id == notification.ApplicationResourceId).AssignedResourceReturnDate;

            var contactData = await db.PersonContacts
                .Where(t => t.IsActive && t.PersonTechnicalId == application.SubmitterPersonId && t.ContactType.Code == ContactType.Email)
                .Select(t => new
                {
                    Email = t.ContactValue,
                    t.PersonTechnicalId,
                    PrivatePersonalIdentifier = t.PersonTechnical.Persons
                        .OrderBy(p => p.ActiveFrom)
                        .Select(p => p.PrivatePersonalIdentifier)
                        .Last()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (contactData == null)
                return;

            // This is done because gdprAuditService internally also calls SaveChangesAsync()
            // which might lead to an infinite loop.
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var gdprAuditService = scope.ServiceProvider.GetRequiredService<IGdprAuditService>();

                await gdprAuditService.TraceAsync(
                    GdprAuditHelper.GenerateTraceForHandleOperation(contactData.PersonTechnicalId, contactData.Email, application.EducationalInstitutionId, notification),
                    cancellationToken
                );
            }

            var (subject, body) = await CreateMessageAsync(application, cancellationToken);

            if (subject == null || body == null)
                return;

            if (options.EAddressEnabled)
            {
                try
                {
                    await SendEAddressMessage(contactData.PrivatePersonalIdentifier, body, application.ApplicationNumber, subject, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send eAddress notification for ApplicationResourceReturnDeadlineChangedEvent. ApplicationResourceId:{applicationId} | AssignedResourceReturnDate:{statusId}.", notification.ApplicationResourceId, notification.AssignedResourceReturnDate);
                }
            }

            await emailService.SendAsync(
                message: emailService.CreateMessage(contactData.Email, subject, body)
                );
        }

        private async Task<(string, string)> CreateMessageAsync(Domain.Entities.Application application, CancellationToken cancellationToken = default)
        {
            var templates = await db.TextTemplates
                .Where(t => t.Code == TextTemplateCode.ApplicationDeadlineChangedNotificationSubject
                    || t.Code == TextTemplateCode.ApplicationDeadlineChangedNotificationBody)
                .Select(t => new { t.Code, t.Content })
                .ToArrayAsync(cancellationToken);

            var propertyMap = ApplicationTextTemplateHelper.CreatePropertyMap(application);

            var subject = TextTemplateParser.Parse(templates.First(t => t.Code == TextTemplateCode.ApplicationDeadlineChangedNotificationSubject).Content, propertyMap);
            var body = TextTemplateParser.Parse(templates.First(t => t.Code == TextTemplateCode.ApplicationDeadlineChangedNotificationBody).Content, propertyMap);

            return (subject, body);
        }

        private async Task SendEAddressMessage(string privatePersonalIdentifier, string body, string applicationNumber, string subject, CancellationToken cancellationToken = default)
        {
            var eAddressClient = serviceProvider.GetRequiredService<IEAddressClient>();

            var validationResult = await eAddressClient.ValidateNaturalPersonAsync(privatePersonalIdentifier, cancellationToken);

            if (!validationResult.IsValid)
                return;

            await eAddressClient.SendMessageAsync(new EAddressSendMessageRequest
            {
                RecipientIdentifier = privatePersonalIdentifier,
                RecipientType = RecipientType.NaturalPerson,
                Content = body,
                RegistrationNumber = applicationNumber,
                Subject = subject
            }, cancellationToken);
        }

        public static class GdprAuditHelper
        {
            public static GdprAuditTraceDto GenerateTraceForHandleOperation(Guid dataOwnerId, string email, int educationalInstitutionId, ApplicationResourceReturnDeadlineChangedEvent notification)
            {
                return new GdprAuditTraceDto
                {
                    Action = "email.applicationResourceReturnDeadlineChanged",
                    ActionData = JsonSerializer.Serialize(notification),
                    DataOwnerId = dataOwnerId,
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
