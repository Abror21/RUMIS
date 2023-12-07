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

namespace Izm.Rumis.Infrastructure.Notifications.EventHandlers.ApplicationStatusChanged
{
    public sealed class NotifyContactPersonEventHandler : INotificationHandler<ApplicationStatusChangedEvent>
    {
        private readonly string[] supportedStatuses = new string[]
        {
            ApplicationStatus.Declined,
            ApplicationStatus.Deleted,
            ApplicationStatus.Submitted
        };

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

        public async Task Handle(ApplicationStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            if (!options.Enabled || options.Ignore)
                return;

            try
            {
                await HandleAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send notification(-s) for ApplicationStatusChangedEvent. ApplicationId:{applicationId} | StatusId:{statusId}.", notification.ApplicationId, notification.ApplicationStatusId);
            }
        }

        private async Task HandleAsync(ApplicationStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            var classifier = await db.Classifiers.FindAsync(new object[] { notification.ApplicationStatusId }, cancellationToken);

            if (!supportedStatuses.Contains(classifier.Code))
                return;

            var application = db.Applications.Local
                .First(t => t.Id == notification.ApplicationId);

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

            var (subject, body) = classifier.Code switch
            {
                ApplicationStatus.Declined => await CreateDeclinedMessageAsync(application, cancellationToken),
                ApplicationStatus.Deleted => await CreateDeletedMessageAsync(application, cancellationToken),
                ApplicationStatus.Submitted => await CreateSubmittedMessageAsync(application, cancellationToken),
                _ => (null, null)
            };

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
                    logger.LogError(ex, "Failed to send eAddress notification for ApplicationStatusChangedEvent. ApplicationId:{applicationId} | StatusId:{statusId}.", notification.ApplicationId, notification.ApplicationStatusId);
                }
            }

            await emailService.SendAsync(
                message: emailService.CreateMessage(contactData.Email, subject, body)
                );
        }

        private async Task<(string, string)> CreateDeclinedMessageAsync(Domain.Entities.Application application, CancellationToken cancellationToken = default)
        {
            var templates = await db.TextTemplates
                .Where(t => t.Code == TextTemplateCode.ApplicationDeclinedNotificationSubject
                    || t.Code == TextTemplateCode.ApplicationDeclinedNotificationBody)
                .Select(t => new { t.Code, t.Content })
                .ToArrayAsync(cancellationToken);

            var propertyMap = ApplicationTextTemplateHelper.CreatePropertyMap(application);

            var subject = TextTemplateParser.Parse(templates.First(t => t.Code == TextTemplateCode.ApplicationDeclinedNotificationSubject).Content, propertyMap);
            var body = TextTemplateParser.Parse(templates.First(t => t.Code == TextTemplateCode.ApplicationDeclinedNotificationBody).Content, propertyMap);

            return (subject, body);
        }

        private async Task<(string, string)> CreateDeletedMessageAsync(Domain.Entities.Application application, CancellationToken cancellationToken = default)
        {
            var templates = await db.TextTemplates
                .Where(t => t.Code == TextTemplateCode.ApplicationDeletedNotificationSubject
                    || t.Code == TextTemplateCode.ApplicationDeletedNotificationBody)
                .Select(t => new { t.Code, t.Content })
                .ToArrayAsync(cancellationToken);

            var propertyMap = ApplicationTextTemplateHelper.CreatePropertyMap(application);

            var subject = TextTemplateParser.Parse(templates.First(t => t.Code == TextTemplateCode.ApplicationDeletedNotificationSubject).Content, propertyMap);
            var body = TextTemplateParser.Parse(templates.First(t => t.Code == TextTemplateCode.ApplicationDeletedNotificationBody).Content, propertyMap);

            return (subject, body);
        }

        private async Task<(string, string)> CreateSubmittedMessageAsync(Domain.Entities.Application application, CancellationToken cancellationToken = default)
        {
            var templates = await db.TextTemplates
                .Where(t => t.Code == TextTemplateCode.ApplicationSubmittedNotificationSubject
                    || t.Code == TextTemplateCode.ApplicationSubmittedNotificationBody)
                .Select(t => new { t.Code, t.Content })
                .ToArrayAsync(cancellationToken);

            // Since application does not have virtual properties loaded at this point,
            // they must be retrieved separately.
            var propertyMap = ApplicationTextTemplateHelper.CreatePropertyMap(application);

            var educationalInstitutionName = await db.EducationalInstitutions
                .Where(t => t.Id == application.EducationalInstitutionId)
                .Select(t => t.Name)
                .FirstAsync(cancellationToken);

            var resourceSubTypeId = await db.Classifiers
                .Where(t => t.Id == application.ResourceSubTypeId)
                .Select(t => t.Value)
                .FirstAsync(cancellationToken);

            propertyMap["EducationalInstitution"] = educationalInstitutionName;
            propertyMap["ResourceSubType"] = resourceSubTypeId;

            propertyMap.Add("ApplicationPublicUrl", options.EServicePublicUrl);

            var subject = TextTemplateParser.Parse(templates.First(t => t.Code == TextTemplateCode.ApplicationSubmittedNotificationSubject).Content, propertyMap);
            var body = TextTemplateParser.Parse(templates.First(t => t.Code == TextTemplateCode.ApplicationSubmittedNotificationBody).Content, propertyMap);

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
            public static GdprAuditTraceDto GenerateTraceForHandleOperation(Guid dataOwnerId, string email, int educationalInstitutionId, ApplicationStatusChangedEvent notification)
            {
                return new GdprAuditTraceDto
                {
                    Action = "email.applicationStatusChanged",
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
