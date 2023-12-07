using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public class ApplicationAttachmentControllerTests
    {
        private ApplicationAttachmentsController controller;
        private ApplicationAttachmentServiceFake service;

        public ApplicationAttachmentControllerTests()
        {
            service = new ApplicationAttachmentServiceFake();
            controller = new ApplicationAttachmentsController(service);
        }

        [Fact]
        public async Task Get_ReturnsData()
        {
            // Assign
            var applicationId = Guid.NewGuid();

            var list = new TestAsyncEnumerable<ApplicationAttachment>(new List<ApplicationAttachment>
            {
                new ApplicationAttachment{ ApplicationId = applicationId },
                new ApplicationAttachment{ ApplicationId = applicationId },
                new ApplicationAttachment{ ApplicationId = new Guid() }
            });

            service.ApplicationAttachments = list;

            // Act
            var result = await controller.Get(applicationId);
            var data = result.Value;

            // Assert
            Assert.Equal(2, data.Count());
        }

        [Fact]
        public async Task Post_ReturnsOk()
        {
            // Act
            var result = await controller.Post(new ApplicationAttachmentCreateRequest
            {
                ApplicationId = Guid.NewGuid(),
                File = CreateFile()
            });
            var data = result.Value;

            // Assert
            Assert.Equal(service.CreateResult, data);
        }

        [Fact]
        public async Task Put_NoFile_ReturnsOk()
        {
            // Act
            var result = await controller.Put(Guid.NewGuid(), new ApplicationAttachmentUpdateRequest());

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Put_HasFile_ReturnsOk()
        {
            // Act
            var result = await controller.Put(Guid.NewGuid(), new ApplicationAttachmentUpdateRequest
            {
                File = CreateFile()
            });

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Download_ReturnsFile()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            await db.ApplicationAttachments.AddAsync(new ApplicationAttachment
            {
                Id = id,
                FileId = Guid.NewGuid(),
                AttachmentNumber = "number",
                AttachmentDate = new DateOnly(2020, 1, 1)
            });

            await db.SaveChangesAsync();

            service.ApplicationAttachments = db.ApplicationAttachments.AsQueryable();

            // Act
            var result = await controller.Download(new FileManagerFake(), id);

            // Assert
            Assert.IsType<FileContentResult>(result);
        }

        private IFormFile CreateFile()
        {
            var bytes = Encoding.UTF8.GetBytes("content");
            var file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "test", "test.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/text"
            };

            return file;
        }
    }
}
