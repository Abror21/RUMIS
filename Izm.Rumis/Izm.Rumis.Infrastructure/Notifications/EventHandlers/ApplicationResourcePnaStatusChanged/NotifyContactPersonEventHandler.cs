using Izm.Rumis.Application;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Helpers;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
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

namespace Izm.Rumis.Infrastructure.Notifications.EventHandlers.ApplicationResourcePnaStatusChanged
{
    public sealed class NotifyContactPersonEventHandler : INotificationHandler<ApplicationResourcePnaStatusChangedEvent>
    {
        private readonly string[] supportedStatuses = new string[]
        {
            PnaStatus.Prepared,
            PnaStatus.Returned
        };

        private readonly NotificationOptions options;
        private readonly IAppDbContext db;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IServiceProvider serviceProvider;
        private readonly IEmailService emailService;
        private readonly ILogger<NotifyContactPersonEventHandler> logger;

        public NotifyContactPersonEventHandler(
            NotificationOptions options,
            IAppDbContext db,
            IServiceScopeFactory serviceScopeFactory,
            IServiceProvider serviceProvider,
            IEmailService emailService,
            ILogger<NotifyContactPersonEventHandler> logger)
        {
            this.options = options;
            this.db = db;
            this.serviceScopeFactory = serviceScopeFactory;
            this.serviceProvider = serviceProvider;
            this.emailService = emailService;
            this.logger = logger;
        }

        public async Task Handle(ApplicationResourcePnaStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            if (!options.Enabled)
                return;

            try
            {
                await HandleAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send notification(-s) for ApplicationResourcePnaStatusChangedEvent. ApplicationResourceId:{applicationResourceId} | PnaStatusId:{pnaStatusId}.", notification.ApplicationResourceId, notification.PnaStatusId);
            }
        }

        private async Task HandleAsync(ApplicationResourcePnaStatusChangedEvent notification, CancellationToken cancellationToken = default)
        {
            var classifier = await db.Classifiers.FindAsync(new object[] { notification.PnaStatusId }, cancellationToken);

            if (!supportedStatuses.Contains(classifier.Code))
                return;

            var applicationResource = db.ApplicationResources.Local
                .First(t => t.Id == notification.ApplicationResourceId);

            var contactData = await db.PersonContacts
                .Where(t => t.IsActive && t.PersonTechnicalId == applicationResource.Application.SubmitterPersonId && t.ContactType.Code == ContactType.Email)
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
                    GdprAuditHelper.GenerateTraceForHandleOperation(contactData.PersonTechnicalId, contactData.Email, applicationResource.Application.EducationalInstitutionId, notification),
                    cancellationToken
                    );
            }

            var (subject, body) = classifier.Code switch
            {
                PnaStatus.Prepared => await CreatePreparedMessageAsync(applicationResource, cancellationToken),
                _ => (null, null)
            };

            if (subject == null || body == null)
                return;

            if (options.EAddressEnabled)
            {
                try
                {
                    await SendEAddressMessage(contactData.PrivatePersonalIdentifier, body, applicationResource.Application.ApplicationNumber, subject, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send eAddress notification for ApplicationResourcePnaStatusChangedEvent. ApplicationResourceId:{applicationResourceId} | PnaStatusId:{pnaStatusId}.", notification.ApplicationResourceId, notification.PnaStatusId);
                }
            }

            await emailService.SendAsync(
                message: emailService.CreateMessage(contactData.Email, subject, body)
                );
        }

        private async Task<(string, string)> CreatePreparedMessageAsync(ApplicationResource applicationResource, CancellationToken cancellationToken = default)
        {
            var templates = await db.TextTemplates
                .Where(t => t.Code == TextTemplateCode.ApplicationResourcePreparedNotificationSubject
                    || t.Code == TextTemplateCode.ApplicationResourcePreparedNotificationBody)
                .Select(t => new { t.Code, t.Content })
                .ToArrayAsync(cancellationToken);

            var propertyMap = ApplicationResourceTextTemplateHelper.CreatePropertyMap(applicationResource);

            var subject = TextTemplateParser.Parse(templates.First(t => t.Code == TextTemplateCode.ApplicationResourcePreparedNotificationSubject).Content, propertyMap);
            var body = TextTemplateParser.Parse(templates.First(t => t.Code == TextTemplateCode.ApplicationResourcePreparedNotificationBody).Content, propertyMap);

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
            public static GdprAuditTraceDto GenerateTraceForHandleOperation(Guid dataOwnerId, string email, int educationalInstitutionId, ApplicationResourcePnaStatusChangedEvent notification)
            {
                return new GdprAuditTraceDto
                {
                    Action = "email.applicationResourcePnaStatusChanged",
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
