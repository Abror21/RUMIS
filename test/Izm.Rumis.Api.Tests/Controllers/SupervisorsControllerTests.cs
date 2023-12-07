using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public sealed class SupervisorsControllerTests
    {
        private SupervisorsController controller;
        private SupervisorServiceFake supervisorServiceFake;

        public SupervisorsControllerTests()
        {
            supervisorServiceFake = new SupervisorServiceFake();

            controller = new SupervisorsController(supervisorServiceFake);
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            // Act
            var result = await controller.Create(new SupervisorCreateRequest());

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
            // Act
            supervisorServiceFake.Supervisors = new TestAsyncEnumerable<Supervisor>(new List<Supervisor>()
            {
                new Supervisor()
            });

            var result = await controller.Get();

            // Assert
            Assert.Equal(supervisorServiceFake.Supervisors.Count(), result.Value.Count());
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            // Act
            var result = await controller.Update(1, new SupervisorUpdateRequest());

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
