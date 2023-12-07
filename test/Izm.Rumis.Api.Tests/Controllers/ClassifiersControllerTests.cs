using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public class ClassifiersControllerTests
    {
        private ClassifiersController controller;
        private ClassifierServiceFake service;

        public ClassifiersControllerTests()
        {
            service = new ClassifierServiceFake();

            controller = new ClassifiersController(service);
        }

        [Fact]
        public async Task Get_ReturnsActive()
        {
            // Assign
            var list = new TestAsyncEnumerable<Classifier>(new List<Classifier>
            {
                new Classifier { PermissionType = UserProfileType.Country },
                new Classifier { PermissionType = UserProfileType.Country, IsDisabled = true },
                new Classifier { PermissionType = UserProfileType.Country },
                new Classifier { PermissionType = UserProfileType.Supervisor, SupervisorId = 1 },
                new Classifier { PermissionType = UserProfileType.Supervisor, SupervisorId = 2 },
                new Classifier { PermissionType = UserProfileType.EducationalInstitution, EducationalInstitutionId = 1 },
                new Classifier { PermissionType = UserProfileType.EducationalInstitution, EducationalInstitutionId = 2 },
                new Classifier { PermissionType = UserProfileType.EducationalInstitution, EducationalInstitutionId = 3 }
            });

            service.Data = list;

            // Act
            var result = await controller.Get();
            var data = result.Value;

            // Assert
            Assert.Equal(7, data.Count());
        }

        [Fact]
        public async Task Get_ReturnsAll()
        {
            var list = new TestAsyncEnumerable<Classifier>(new List<Classifier>
            {
                new Classifier(),
                new Classifier { IsDisabled = true },
                new Classifier()
            });

            service.Data = list;

            var result = await controller.Get(includeDisabled: true);
            var data = result.Value;

            Assert.Equal(list.Count(), data.Count());
        }

        [Fact]
        public async Task Get_ByType_ReturnsActive()
        {
            const string type = "x";

            var list = new TestAsyncEnumerable<Classifier>(new List<Classifier>
            {
                new Classifier { Type = "x" },
                new Classifier { Type = "x", IsDisabled = true }
            });

            service.Data = list;

            var result = await controller.Get(type);
            var data = result.Value;

            Assert.Single(data);
            Assert.Contains(data, t => t.Type == type);
        }

        [Fact]
        public async Task Get_ByCode_ReturnsData()
        {
            const string code = "c1";

            var list = new TestAsyncEnumerable<Classifier>(new List<Classifier>
            {
                new Classifier { Code = "c1" },
                new Classifier { Code = "c2" }
            });

            service.Data = list;

            var result = await controller.Get(null, code);
            var data = result.Value;

            Assert.Single(data);
            Assert.Contains(data, t => t.Code == code);
        }

        [Fact]
        public async Task Get_ByTypeAndCode_ReturnsData()
        {
            const string type = "x";
            const string code = "c1";

            var list = new TestAsyncEnumerable<Classifier>(new List<Classifier>
            {
                new Classifier { Type = "x", Code = "c1" },
                new Classifier { Type = "x", Code = "c2" }
            });

            service.Data = list;

            var result = await controller.Get(type, code);
            var data = result.Value;

            Assert.Single(data);
            Assert.Contains(data, t => t.Type == type && t.Code == code);
        }

        [Fact]
        public async Task Post_ReturnsOk()
        {

            var result = await controller.Post(new ClassifierCreateModel
            {
                PermissionType = UserProfileType.Country
            });
            var data = result.Value;

            Assert.Equal(service.CreateResult, data);
        }

        [Fact]
        public async Task Put_ReturnsNoContent()
        {
            var result = await controller.Put(Guid.NewGuid(), new ClassifierEditModel
            {
                Value = "x"
            });

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent()
        {
            var result = await controller.Delete(Guid.NewGuid());
            Assert.IsType<NoContentResult>(result);
        }
    }
}
