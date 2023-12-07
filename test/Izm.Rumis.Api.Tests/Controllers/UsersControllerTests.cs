using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public sealed class UsersControllerTests
    {
        private UsersController controller;
        private UserServiceFake service;
        private GdprAuditServiceFake gdprAuditServiceFake;

        public UsersControllerTests()
        {
            service = new UserServiceFake();
            gdprAuditServiceFake = new GdprAuditServiceFake();

            controller = new UsersController(service, gdprAuditServiceFake);
        }

        [Fact]
        public async Task Delete_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            // Act
            var result = await controller.Delete(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(service.Deleted, id);
        }

        [Fact]
        public async Task GetPersons_ReturnsData()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            db.Persons.Add(new Person
            {
                PrivatePersonalIdentifier = "00000000001",
                PersonTechnical = new PersonTechnical
                {
                    User = User.Create()
                }
            });

            await db.SaveChangesAsync();

            service.Users = db.Users.AsQueryable();

            // Act
            var result = await controller.GetPersons();
            var data = result.Value;

            // Assert
            Assert.Equal(service.Users.Count(), data.Items.Count());
            Assert.NotNull(gdprAuditServiceFake.TraceRangeAsyncCalledWith);
        }
    }
}
