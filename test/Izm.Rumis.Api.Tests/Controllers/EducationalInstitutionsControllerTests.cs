using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public sealed class EducationalInstitutionsControllerTests
    {
        private EducationalInstitutionsController controller;
        private EducationalInstitutionServiceFake EducationalInstitutionServiceFake;

        public EducationalInstitutionsControllerTests()
        {
            EducationalInstitutionServiceFake = new EducationalInstitutionServiceFake();

            controller = new EducationalInstitutionsController(EducationalInstitutionServiceFake);
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            // Act
            var result = await controller.Create(new EducationalInstitutionCreateRequest());

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
            const int educationalInstitutionId = 1;
            const int supervisorId = 1;
            var statusClassifierId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            await db.SaveChangesAsync();

            await db.Supervisors.AddAsync(new Supervisor
            {
                Id = supervisorId,
                Code = string.Empty,
                Name = string.Empty
            });

            await db.EducationalInstitutions.AddAsync(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                StatusId = statusClassifierId,
                SupervisorId = supervisorId,
                Code = string.Empty,
                Name = string.Empty
            });

            await db.SaveChangesAsync();

            var result = await controller.Get();

            // Assert
            Assert.Equal(EducationalInstitutionServiceFake.EducationalInstitutions.Count(), result.Value.Count());
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            // Act
            var result = await controller.Update(1, new EducationalInstitutionUpdateRequest());

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
