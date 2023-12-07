using Izm.Rumis.Api.Tests.Services;
using Microsoft.EntityFrameworkCore;
using System;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal class ServiceFactory
    {
        public static AppDbContext ConnectDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString("N"))
                .Options;

            var db = new DbContextFake(options, CreateCurrentUserService(), CreateMediator());

            return db;
        }

        public static CurrentUserServiceFake CreateCurrentUserService()
        {
            return new CurrentUserServiceFake();
        }

        public static LoggerFake<T> CreateLogger<T>()
        {
            return new LoggerFake<T>();
        }

        public static MediatorFake CreateMediator()
        {
            return new MediatorFake();
        }

        public static HttpMessageHandlerFake CreateHttpMessageHandler()
        {
            return new HttpMessageHandlerFake();
        }

        public static VraaClientFake CreateVraaClientFake()
        {
            return new VraaClientFake();
        }

        public static VraaUserFake CreateVraaUser()
        {
            return new VraaUserFake();
        }

        public static ViisServiceFake CreateViisService()
        {
            return new ViisServiceFake();
        }

        public static ApplicationServiceFake CreateApplicationService()
        {
            return new ApplicationServiceFake();
        }

        public static ApplicationResourceServiceFake CreateApplicationResourceService()
        {
            return new ApplicationResourceServiceFake();
        }

        public static AuthorizationServiceFake CreateAuthorizationService()
        {
            return new AuthorizationServiceFake();
        }

        public static DistributedCacheFake CreateDistributedCache()
        {
            return new DistributedCacheFake();
        }

        public static SequenceServiceFake CreateSequenceService()
        {
            return new SequenceServiceFake();
        }


        public static GdprAuditServiceFake CreateGdprAuditService()
        {
            return new GdprAuditServiceFake();
        }

        public static FileServiceFake CreateFileService()
        {
            return new FileServiceFake();
        }

        public static DocumentTemplateServiceFake CreateDocumentTemplateService() 
        {
            return new DocumentTemplateServiceFake();
        }
    }
}
