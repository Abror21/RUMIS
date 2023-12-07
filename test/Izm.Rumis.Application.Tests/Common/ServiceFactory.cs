using Izm.Rumis.Infrastructure;
using Izm.Rumis.Infrastructure.Tests.Common;
using Microsoft.EntityFrameworkCore;
using System;

namespace Izm.Rumis.Application.Tests.Common
{
    internal class ServiceFactory
    {
        public static DbContextFake ConnectDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString("N"))
                .Options;

            var db = new DbContextFake(options, CreateCurrentUserService(), CreateMediator());

            return db;
        }

        public static CurrentUserServiceFake CreateCurrentUserService() => new();

        public static LoggerFake<T> CreateLogger<T>() => new();

        public static FileServiceFake CreateFileService() => new();

        public static EmailServiceFake CreateEmailService() => new();

        public static CurrentUserProfileServiceFake CreateCurrentUserProfileService() => new();

        public static PersonValidatorFake CreatePersonValidator() => new();

        public static ClassifierValidatorFake CreateClassifierValidator() => new();

        public static ApplicationAttachmentValidatorFake CreateApplicationAttachmentValidator() => new();

        public static AuthorizationServiceFake CreateAuthorizationService() => new();

        public static ApplicationResourceValidatorFake CreateApplicationResourceValidator() => new();

        public static DocumentTemplateServiceFake CreateDocumentTemplateService() => new();

        public static DocumentTemplateValidatorFake CreateDocumentTemplateValidator() => new();

        public static MediatorFake CreateMediator() => new();

        public static ApplicationValidatorFake CreateApplicationValidator() => new();

        public static ApplicationSocialStatusCheckService CreateApplicationSocialStatusCheckService() => new();

        public static PersonDataServiceFake CreatePersonDataService() => new();

        public static SequenceServiceFake CreateSequenceService() => new();

        public static GdprAuditServiceFake CreateGdprAuditService() => new();

        public static ResourceValidatorFake CreateResourceValidatorFake() => new();

        public static ApplicationDuplicateServiceFake CreateApplicationDuplicateService() => new();

        public static GdprAuditValidatorFake CreateGdprAuditValidator() => new();

        public static DistributedCacheFake CreateDistributedCache() => new();

        public static PersonDataValidatorFake CreatePersonDataReportValidator() => new();
    }
}
