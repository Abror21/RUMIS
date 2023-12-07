using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
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
    public class DocumentTemplatesControllerTests
    {
        private DocumentTemplatesController controller;
        private DocumentTemplateServiceFake service;
        private ClassifierServiceFake classifierServiceFake;

        public DocumentTemplatesControllerTests()
        {
            service = new DocumentTemplateServiceFake();
            classifierServiceFake = new ClassifierServiceFake();
            controller = new DocumentTemplatesController(service, classifierServiceFake);
        }

        [Fact]
        public async Task Get_ReturnsData_NoPaging_NoFilter()
        {
            using var db = ServiceFactory.ConnectDb();

            var resourceTypeId = Guid.NewGuid();
            var hyperlinkId = Guid.NewGuid();

            var classifiers = new TestAsyncEnumerable<Classifier>(new List<Classifier>
            {
              new Classifier
                {
                    Id = resourceTypeId,
                    Code = "c",
                    Value = "v",
                    Type = ClassifierTypes.ResourceType
                },
               new Classifier
                {
                    Id = hyperlinkId,
                    Code =  DocumentType.Hyperlink,
                    Value = "v",
                    Type = ClassifierTypes.DocumentType
                }
            });

            db.Classifiers.AddRange(classifiers);

            await db.SaveChangesAsync();

            var list = new TestAsyncEnumerable<DocumentTemplate>(new List<DocumentTemplate>
            {
                new DocumentTemplate {
                    Title = "Test",
                    Code = DocumentType.Hyperlink,
                    ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    ResourceTypeId = resourceTypeId,
                    Hyperlink = "123"
                },
                new DocumentTemplate {
                    Title = "Test",
                    Code = DocumentType.Hyperlink,
                    ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    ResourceTypeId = resourceTypeId,
                    Hyperlink = "1"
                }
            });

            db.DocumentTemplates.AddRange(list);

            await db.SaveChangesAsync();

            classifierServiceFake.Data = classifiers;
            service.DocumentTemplates = db.DocumentTemplates.AsQueryable();

            var result = await controller.Get();
            var data = result.Value.Items;

            Assert.Equal(list.Count(), data.Count());
        }

        [Fact]
        public async Task Get_ReturnsData_NoFilter()
        {
            using var db = ServiceFactory.ConnectDb();

            var resourceTypeId = Guid.NewGuid();
            var hyperlinkId = Guid.NewGuid();

            var classifiers = new TestAsyncEnumerable<Classifier>(new List<Classifier>
            {
              new Classifier
                {
                    Id = resourceTypeId,
                    Code = "c",
                    Value = "v",
                    Type = ClassifierTypes.ResourceType
                },
               new Classifier
                {
                    Id = hyperlinkId,
                    Code =  DocumentType.Hyperlink,
                    Value = "v",
                    Type = ClassifierTypes.DocumentType
                }
            });

            db.Classifiers.AddRange(classifiers);

            await db.SaveChangesAsync();

            var list = new TestAsyncEnumerable<DocumentTemplate>(new List<DocumentTemplate>
            {
                new DocumentTemplate {
                    Title = "Test",
                    Code = DocumentType.Hyperlink,
                    ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    ResourceTypeId = resourceTypeId,
                    Hyperlink = "123"
                },
                new DocumentTemplate {
                    Title = "Test",
                    Code = DocumentType.Hyperlink,
                    ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    ResourceTypeId = resourceTypeId,
                    Hyperlink = "1"
                }
            });

            db.DocumentTemplates.AddRange(list);

            await db.SaveChangesAsync();

            classifierServiceFake.Data = classifiers;
            service.DocumentTemplates = db.DocumentTemplates.AsQueryable();

            var pagingRequest = new PagingRequest
            {
                Page = 1,
                Take = 1
            };

            var result = await controller.Get(paging: pagingRequest);
            var data = result.Value.Items;

            Assert.NotNull(result);
            Assert.Equal(pagingRequest.Take, data.Count());
        }


        [Fact]
        public async Task Get_ByCode_ReturnsData()
        {
            using var db = ServiceFactory.ConnectDb();

            var hyperlinkId = Guid.NewGuid();
            var pnaId = Guid.NewGuid();
            var resourceTypeId = Guid.NewGuid();

            var classifiers = new TestAsyncEnumerable<Classifier>(new List<Classifier>
            {
                new Classifier
                {
                    Id = resourceTypeId,
                    Code = "c",
                    Value = "v",
                    Type = ClassifierTypes.ResourceType
                },
                new Classifier
                {
                    Id = hyperlinkId,
                    Code =  DocumentType.Hyperlink,
                    Value = "v",
                    Type = ClassifierTypes.DocumentType
                },
                new Classifier
                {
                    Id = pnaId,
                    Code =  DocumentType.PNA,
                    Value = "v",
                    Type = ClassifierTypes.DocumentType
                }
            });

            db.Classifiers.AddRange(classifiers);

            await db.SaveChangesAsync();

            var list = new TestAsyncEnumerable<DocumentTemplate>(new List<DocumentTemplate>
            {
                new DocumentTemplate {
                    Title = "Test",
                    Code = DocumentType.Hyperlink,
                    ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    ResourceTypeId = resourceTypeId,
                    Hyperlink = "123"
                },
                new DocumentTemplate {
                    Title = "Test",
                    Code = DocumentType.PNA,
                    ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    ResourceTypeId = resourceTypeId
                }
            });

            db.DocumentTemplates.AddRange(list);

            await db.SaveChangesAsync();

            classifierServiceFake.Data = classifiers;
            service.DocumentTemplates = db.DocumentTemplates.AsQueryable();

            var filterRequset = new DocumentTemplateFilterRequest
            {
                Codes = new[] { DocumentType.PNA }
            };


            var result = await controller.Get(filter: filterRequset);
            var data = result.Value.Items;

            Assert.Single(data);
            Assert.Contains(data, t => t.DocumentType.Code == DocumentType.PNA);
        }


        [Fact]
        public async Task Get_ReturnsData_Filter()
        {
            using var db = ServiceFactory.ConnectDb();

            var resourceTypeId = Guid.NewGuid();
            var hyperlinkId = Guid.NewGuid();
            var pnaId = Guid.NewGuid();

            var classifiers = new TestAsyncEnumerable<Classifier>(new List<Classifier>
            {
              new Classifier
                {
                    Id = resourceTypeId,
                    Code = "c",
                    Value = "v",
                    Type = ClassifierTypes.ResourceType
                },
               new Classifier
                {
                    Id = hyperlinkId,
                    Code =  DocumentType.Hyperlink,
                    Value = "v",
                    Type = ClassifierTypes.DocumentType
                },
                new Classifier
                {
                    Id = pnaId,
                    Code =  DocumentType.PNA,
                    Value = "v",
                    Type = ClassifierTypes.DocumentType
                }
            });

            db.Classifiers.AddRange(classifiers);

            await db.SaveChangesAsync();

            var list = new TestAsyncEnumerable<DocumentTemplate>(new List<DocumentTemplate>
            {
                new DocumentTemplate {
                    Title = "Test",
                    Code = DocumentType.Hyperlink,
                    ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    ResourceTypeId = resourceTypeId,
                    Hyperlink = "123"
                },
                new DocumentTemplate {
                    Title = "Test",
                    Code = DocumentType.PNA,
                    ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    ResourceTypeId = resourceTypeId,
                    Hyperlink = "1"
                }
            });

            db.DocumentTemplates.AddRange(list);

            await db.SaveChangesAsync();

            classifierServiceFake.Data = classifiers;
            service.DocumentTemplates = db.DocumentTemplates.AsQueryable();

            var filterRequsetBoth = new DocumentTemplateFilterRequest
            {
                Title = "Test",
                Codes = new[] { DocumentType.Hyperlink, DocumentType.PNA },
                ValidFromMin = DateTime.Today,
                ValidFromMax = DateTime.Today,
                ValidToMin = DateTime.Today.AddMonths(1),
                ValidToMax = DateTime.Today.AddMonths(1),
                ResourceTypeIds = new[] { resourceTypeId },
                Hyperlink = "1"
            };


            var result = await controller.Get(filter: filterRequsetBoth);
            var data = result.Value.Items;

            Assert.Equal(list.Count(), data.Count());

            var filterRequsetSingle = new DocumentTemplateFilterRequest
            {
                Title = "Test",
                Codes = new[] { DocumentType.Hyperlink, DocumentType.PNA },
                ValidFromMin = DateTime.Today,
                ValidFromMax = DateTime.Today,
                ValidToMin = DateTime.Today.AddMonths(1),
                ValidToMax = DateTime.Today.AddMonths(1),
                ResourceTypeIds = new[] { resourceTypeId },
                Hyperlink = "123"
            };


            result = await controller.Get(filter: filterRequsetSingle);
            data = result.Value.Items;

            Assert.Single(data);
            Assert.Contains(data, t => t.Hyperlink == filterRequsetSingle.Hyperlink);

            var filterRequsetNone = new DocumentTemplateFilterRequest
            {
                Title = "Test",
                Codes = new[] { DocumentType.Hyperlink, DocumentType.PNA },
                ValidFromMin = DateTime.Today.AddMonths(1),
                ValidFromMax = DateTime.Today.AddMonths(1),
                ValidToMin = DateTime.Today.AddMonths(1),
                ValidToMax = DateTime.Today.AddMonths(1),
                ResourceTypeIds = new[] { resourceTypeId },
                Hyperlink = "123"
            };


            result = await controller.Get(filter: filterRequsetNone);
            data = result.Value.Items;

            Assert.Empty(data);
        }

        [Fact]
        public async Task Get_ReturnsData_Suppervisor_Filter()
        {
            using var db = ServiceFactory.ConnectDb();

            var resourceTypeId = Guid.NewGuid();
            const int supervisorFirstId = 1;
            const int supervisorSecondId = 2;
            var hyperlinkId = Guid.NewGuid();
            var pnaId = Guid.NewGuid();

            var classifiers = new TestAsyncEnumerable<Classifier>(new List<Classifier>
            {
              new Classifier
                {
                    Id = resourceTypeId,
                    Code = "c",
                    Value = "v",
                    Type = ClassifierTypes.ResourceType
                },
               new Classifier
                {
                    Id = hyperlinkId,
                    Code =  DocumentType.Hyperlink,
                    Value = "v",
                    Type = ClassifierTypes.DocumentType
                },
                new Classifier
                {
                    Id = pnaId,
                    Code =  DocumentType.PNA,
                    Value = "v",
                    Type = ClassifierTypes.DocumentType
                }
            });

            var supervisors = new TestAsyncEnumerable<Supervisor>(new List<Supervisor>
            {
              new Supervisor
                {
                      Id = supervisorFirstId,
                    Code = "c",
                    Name = "n"
                },
               new Supervisor
                {
                      Id = supervisorSecondId,
                    Code = "c",
                    Name = "n"
                }
            });

            db.Supervisors.AddRange(supervisors);

            db.Classifiers.AddRange(classifiers);

            await db.SaveChangesAsync();

            var list = new TestAsyncEnumerable<DocumentTemplate>(new List<DocumentTemplate>
            {
                new DocumentTemplate {
                    Title = "Test",
                    Code = DocumentType.Hyperlink,
                    ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    ResourceTypeId = resourceTypeId,
                    PermissionType = UserProfileType.Supervisor,
                    SupervisorId = supervisorFirstId,
                    Hyperlink = "123"
                },
                new DocumentTemplate {
                    Title = "Test",
                    Code = DocumentType.PNA,
                    ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    ResourceTypeId = resourceTypeId,
                    PermissionType = UserProfileType.Supervisor,
                    SupervisorId = supervisorSecondId,
                    Hyperlink = "1"
                },
                new DocumentTemplate {
                    Title = "Test",
                    Code = DocumentType.PNA,
                    ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    ResourceTypeId = resourceTypeId,
                    PermissionType = UserProfileType.Country,
                    SupervisorId = null,
                    Hyperlink = "1"
                }
            });

            db.DocumentTemplates.AddRange(list);

            await db.SaveChangesAsync();

            classifierServiceFake.Data = classifiers;
            service.DocumentTemplates = db.DocumentTemplates.AsQueryable();

            var filterAll = new DocumentTemplateFilterRequest
            {
                Title = "Test",
                Codes = new[] { DocumentType.Hyperlink, DocumentType.PNA },
                ValidFromMin = DateTime.Today,
                ValidFromMax = DateTime.Today,
                ValidToMin = DateTime.Today.AddMonths(1),
                ValidToMax = DateTime.Today.AddMonths(1),
                ResourceTypeIds = new[] { resourceTypeId },
                PermissionTypes = new[] { UserProfileType.Country, UserProfileType.Supervisor },
                Hyperlink = "1"
            };

            var result = await controller.Get(filter: filterAll);
            var data = result.Value.Items;

            Assert.Equal(list.Count(), data.Count());

            var filterSuppervisers = new DocumentTemplateFilterRequest
            {
                Title = "Test",
                Codes = new[] { DocumentType.Hyperlink, DocumentType.PNA },
                ValidFromMin = DateTime.Today,
                ValidFromMax = DateTime.Today,
                ValidToMin = DateTime.Today.AddMonths(1),
                ValidToMax = DateTime.Today.AddMonths(1),
                ResourceTypeIds = new[] { resourceTypeId },
                PermissionTypes = new[] { UserProfileType.Supervisor },
                SupervisorIds = new[] { supervisorFirstId, supervisorSecondId },
                Hyperlink = "1"
            };


            result = await controller.Get(filter: filterSuppervisers);
            data = result.Value.Items;

            Assert.Equal(2, data.Count());

            Assert.DoesNotContain(data, t => t.PermissionType == UserProfileType.Country);

            var filterRequsetSingle = new DocumentTemplateFilterRequest
            {
                Title = "Test",
                Codes = new[] { DocumentType.Hyperlink, DocumentType.PNA },
                ValidFromMin = DateTime.Today,
                ValidFromMax = DateTime.Today,
                ValidToMin = DateTime.Today.AddMonths(1),
                ValidToMax = DateTime.Today.AddMonths(1),
                PermissionTypes = new[] { UserProfileType.Supervisor },
                ResourceTypeIds = new[] { resourceTypeId },
                SupervisorIds = new[] { supervisorSecondId }
            };


            result = await controller.Get(filter: filterRequsetSingle);
            data = result.Value.Items;

            Assert.Single(data);

            Assert.Contains(data, t => t.DocumentType.Code == DocumentType.PNA);
            Assert.Contains(data, t => t.SupervisorId == supervisorSecondId);

            var filterRequsetCountry = new DocumentTemplateFilterRequest
            {
                Title = "Test",
                Codes = new[] { DocumentType.Hyperlink, DocumentType.PNA },
                ValidFromMin = DateTime.Today,
                ValidFromMax = DateTime.Today,
                ValidToMin = DateTime.Today.AddMonths(1),
                ValidToMax = DateTime.Today.AddMonths(1),
                PermissionTypes = new[] { UserProfileType.Country },
                SupervisorIds = new[] { supervisorSecondId }
            };


            result = await controller.Get(filter: filterRequsetCountry);
            data = result.Value.Items;

            Assert.Single(data);
            Assert.Contains(data, t => t.PermissionType == UserProfileType.Country);

            var filterRequsetNone = new DocumentTemplateFilterRequest
            {
                Title = "Test",
                Codes = new[] { DocumentType.Hyperlink, DocumentType.PNA },
                ValidFromMin = DateTime.Today.AddMonths(1),
                ValidFromMax = DateTime.Today.AddMonths(1),
                ValidToMin = DateTime.Today.AddMonths(1),
                ValidToMax = DateTime.Today.AddMonths(1),
                PermissionTypes = new[] { UserProfileType.Supervisor },
                ResourceTypeIds = new[] { resourceTypeId },
                SupervisorIds = { }
            };


            result = await controller.Get(filter: filterRequsetNone);
            data = result.Value.Items;

            Assert.Empty(data);
        }

        [Fact]
        public async Task Post_ReturnsOk()
        {
            var result = await controller.Post(new DocumentTemplateCreateModel
            {
                File = CreateFile()
            });
            var data = result.Value;

            Assert.Equal(service.CreateResult, data);
        }

        [Fact]
        public async Task Put_NoFile_ReturnsOk()
        {
            var result = await controller.Put(1, new DocumentTemplateUpdateModel());
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Put_HasFile_ReturnsOk()
        {
            var result = await controller.Put(1, new DocumentTemplateUpdateModel
            {
                File = CreateFile()
            });

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsOk()
        {
            var result = await controller.Delete(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Sample_Succeeds()
        {
            // Assign
            const int id = 1;
            const string content = "sample";

            service.DocumentTemplateSample = content;

            // Act
            var result = await controller.Sample(id);

            // Assert
            Assert.Equal(id, service.GetSampleAsyncCalledWith);
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task Download_ReturnsFile()
        {
            // Assign
            const int id = 1;

            using var db = ServiceFactory.ConnectDb();

            var resourceTypeId = Guid.NewGuid();

            var classifiers = new Classifier[]
            {
                new Classifier
                {
                    Id = resourceTypeId,
                    Code = "c",
                    Value = "v",
                    Type = ClassifierTypes.ResourceType
                }
            };

            db.Classifiers.AddRange(classifiers);

            await db.SaveChangesAsync();

            await db.DocumentTemplates.AddAsync(new DocumentTemplate
            {
                Id = id,
                Code = "someCode",
                FileName = "test.txt",
                FileId = Guid.NewGuid(),
                Title = "someTitle",
                ResourceTypeId = resourceTypeId
            });

            await db.SaveChangesAsync();

            service.DocumentTemplates = db.DocumentTemplates.AsQueryable();
            classifierServiceFake.Data = new TestAsyncEnumerable<Classifier>(classifiers);

            // Act
            var result = await controller.Download(new FileManagerFake(), id);

            // Assert
            Assert.IsType<FileContentResult>(result);

            //Assert.Equal(template.FileName, ok.FileDownloadName);
            //Assert.Equal(bytes, ok.FileContents);
            //Assert.Equal("application/octet-stream", ok.ContentType);
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
