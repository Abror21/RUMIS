using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public class ParametersControllerTests
    {
        private ParametersController controller;
        private ParameterServiceFake service;
        private CurrentUserProfileServiceFake currentUserProfile;

        /// <summary>
        /// Parameter code visible only to admin
        /// </summary>
        private const string adminParamCode = "P1";

        private readonly int publicTotal;
        private readonly int privateTotal;

        public ParametersControllerTests()
        {
            service = new ParameterServiceFake();
            currentUserProfile = new CurrentUserProfileServiceFake();
            controller = new ParametersController(service, currentUserProfile);

            var publicParams = controller.PublicParameters.Select(t => new Parameter { Code = t });
            var privateParams = controller.PrivateParameters.Select(t => new Parameter { Code = t });

            var allParams = new List<Parameter>();
            allParams.AddRange(publicParams);
            allParams.AddRange(privateParams);

            allParams.Add(new Parameter { Code = adminParamCode });

            publicTotal = publicParams.Count();
            privateTotal = privateParams.Count();

            service.Parameters = new TestAsyncEnumerable<Parameter>(allParams);
        }

        [Fact]
        public async Task Get_ReturnsAll()
        {
            currentUserProfile.Permissions = new string[] { IdentityPermissions.ParameterView };

            var result = await controller.Get();
            var data = result.Value;

            Assert.Equal(service.Parameters.Count(), data.Count());
        }

        [Fact]
        public async Task Get_ReturnsPublicAndPrivate()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity("TestAuth"));

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            var result = await controller.Get();
            var data = result.Value;

            Assert.Equal(publicTotal + privateTotal, data.Count());
            Assert.DoesNotContain(data, t => t.Code == adminParamCode);
        }

        [Fact]
        public async Task Get_ReturnsOnlyPublic()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            var result = await controller.Get();
            var data = result.Value;

            Assert.Equal(publicTotal, data.Count());
            Assert.DoesNotContain(data, t => !controller.PublicParameters.Contains(t.Code));
        }

        [Fact]
        public async Task Put_ReturnsNoContent()
        {
            var result = await controller.Put(1, new ParameterUpdateModel
            {
                Value = "x"
            });

            Assert.IsType<NoContentResult>(result);
        }
    }
}
