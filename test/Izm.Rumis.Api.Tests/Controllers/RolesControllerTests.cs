using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public sealed class RolesControllerTests
    {
        private RolesController controller;
        private RoleServiceFake roleServiceFake;

        public RolesControllerTests()
        {
            roleServiceFake = new RoleServiceFake();

            controller = new RolesController(roleServiceFake);
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            // Act
            var result = await controller.Create(new RoleEditRequest());

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Delete_Succeeds()
        {
            // Act
            var result = await controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Get_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            await db.Roles.AddAsync(new Role
            {
                Code = "someCode",
                Name = "someName"
            });
            await db.Roles.AddAsync(new Role
            {
                Code = "someCode",
                Name = "someName"
            });

            await db.SaveChangesAsync();

            roleServiceFake.Roles = db.Roles.AsQueryable();

            // Act
            var result = await controller.Get();

            // Assert
            Assert.Equal(roleServiceFake.Roles.Count(), result.Value.Total);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            // Act
            var result = await controller.Update(1, new RoleEditRequest());

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
