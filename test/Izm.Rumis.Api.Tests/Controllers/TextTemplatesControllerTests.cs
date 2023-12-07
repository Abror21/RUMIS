using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public class TextTemplatesControllerTests
    {
        private TextTemplatesController controller;
        private TextTemplateServiceFake service;

        public TextTemplatesControllerTests()
        {
            service = new TextTemplateServiceFake();
            controller = new TextTemplatesController(service);
        }

        [Fact]
        public async Task Get_ReturnsData()
        {
            var list = new TestAsyncEnumerable<TextTemplate>(new List<TextTemplate>
            {
                new TextTemplate(),
                new TextTemplate()
            });

            service.TextTemplates = list;

            var result = await controller.Get();
            var data = result.Value;

            Assert.Equal(list.Count(), data.Count());
        }

        [Fact]
        public async Task Get_ByCode_ReturnsData()
        {
            const string code = "c1";

            var list = new TestAsyncEnumerable<TextTemplate>(new List<TextTemplate>
            {
                new TextTemplate { Code = "c1" },
                new TextTemplate { Code = "c2" }
            });

            service.TextTemplates = list;

            var result = await controller.Get(code);
            var data = result.Value;

            Assert.Single(data);
            Assert.Contains(data, t => t.Code == code);
        }

        [Fact]
        public async Task GetTermOfUse_ReturnsData()
        {

            using var db = ServiceFactory.ConnectDb();

            await db.TextTemplates.AddAsync(new TextTemplate
            {
                Code = "c1",
                Title = string.Empty,
                Content = string.Empty
            });

            await db.TextTemplates.AddAsync(new TextTemplate
            {
                Code = TextTemplateCode.TermOfUse,
                Title = string.Empty,
                Content = string.Empty
            });

            await db.SaveChangesAsync();

            service.TextTemplates = db.TextTemplates.AsQueryable(); ;

            var result = await controller.TermsOfUse();
            var data = result.Value;
            Assert.Equal(TextTemplateCode.TermOfUse, data.Code);
        }

        [Fact]
        public async Task Post_ReturnsOk()
        {
            var result = await controller.Post(new TextTemplateEditModel());
            var data = result.Value;

            Assert.Equal(service.CreateResult, data);
        }

        [Fact]
        public async Task Put_ReturnsOk()
        {
            var result = await controller.Put(1, new TextTemplateEditModel());
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsOk()
        {
            var result = await controller.Delete(1);
            Assert.IsType<NoContentResult>(result);
        }
    }
}
