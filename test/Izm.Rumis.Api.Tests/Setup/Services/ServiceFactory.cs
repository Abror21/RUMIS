using Izm.Rumis.Api.Tests.Setup.Options;
using Izm.Rumis.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal static class ServiceFactory
    {
        public static DbContextFake ConnectDb()
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

        public static AuthUserProfileOptionsFake CreateAuthUserProfileOptions()
        {
            return new AuthUserProfileOptionsFake();
        }

        public static HttpContextAccessorFake CreateHttpContextAccessor()
        {
            return new HttpContextAccessorFake();
        }

        public static MediatorFake CreateMediator()
        {
            return new MediatorFake();
        }

        public static PersonServiceFake CreatePersonService()
        {
            return new PersonServiceFake();
        }

        public static ViisServiceFake CreateViisService()
        {
            return new ViisServiceFake();
        }

        public static EServicesServiceFake CreateEServicesService()
        {
            return new EServicesServiceFake();
        }

        public static AuthorizationServiceFake CreateAuthorizationService()
        {
            return new AuthorizationServiceFake();
        }

        public static VraaUserFake CreateVraaUser()
        {
            return new VraaUserFake();
        }

        public static SessionFake CreateSession()
        {
            return new SessionFake();
        }

        public static SessionServiceFake CreateSessionService()
        {
            return new SessionServiceFake();
        }

        public static SessionManagerFake CreateSessionManager()
        {
            return new SessionManagerFake();
        }

        public static GdprAuditServiceFake CreateGdprAuditService()
        {
            return new GdprAuditServiceFake();
        }

        public static PersonDateReportServiceFake CreatePersonDataReportService()
        {
            return new PersonDateReportServiceFake();
        }

        public static ClassifierServiceFake CreateClassifierService()
        {
            return new ClassifierServiceFake();
        }

        public static AuthSettingsFake CreateAuthSettings()
        {
            return new AuthSettingsFake();
        }

        public static DistributedCacheFake CreateDistributedCache()
        {
            return new DistributedCacheFake();
        }
    }
}
