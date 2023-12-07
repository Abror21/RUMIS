using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using Izm.Rumis.Infrastructure;
using Izm.Rumis.Infrastructure.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public sealed class ApplicationResourcesControllerTests
    {
        private ApplicationResourcesController controller;
        private ApplicationResourceServiceFake service;
        private ClassifierServiceFake classifierService;
        private GdprAuditServiceFake gdprAuditService;

        public ApplicationResourcesControllerTests()
        {
            service = new ApplicationResourceServiceFake();
            classifierService = new ClassifierServiceFake();
            gdprAuditService = new GdprAuditServiceFake();

            controller = new ApplicationResourcesController(
                service: service,
                classifierService: classifierService,
                gdprAuditService: gdprAuditService);
        }

        [Fact]
        public async Task Get_NoData()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            service.ApplicationResources = db.ApplicationResources.AsQueryable();

            // Act
            var result = await controller.Get();

            // Assert
            Assert.Empty(result.Value.Items);
        }

        [Fact]
        public async Task Get_ReturnsData_NoPaging_NoFilter()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;
            const int supervisorId = 1;

            var pnaStatusId = Guid.NewGuid();
            var eduStatusId = Guid.NewGuid();
            var documentTypeId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var manufacturerId = Guid.NewGuid();
            var resourceTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var resourceLocationId = Guid.NewGuid();
            var targetGroupId = Guid.NewGuid();
            var usagePurposeTypeId = Guid.NewGuid();
            var acquisitionTypeId = Guid.NewGuid();
            var resourceStatusId = Guid.NewGuid();
            var resourceModelId = Guid.NewGuid();

            var eduStatus = new Classifier
            {
                Id = eduStatusId,
                Type = ClassifierTypes.EducationalInstitutionStatus,
                Code = EducationalInstitutionStatus.Active,
                Value = string.Empty
            };

            var pnaStatus = new Classifier
            {
                Id = pnaStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Issued,
                Value = string.Empty
            };

            var resourceType = new Classifier
            {
                Id = resourceTypeId,
                Type = ClassifierTypes.ResourceType,
                Code = ResourceType.Computer,
                Value = string.Empty
            };

            var resourceSubType = new Classifier
            {
                Id = resourceSubTypeId,
                Type = ClassifierTypes.ResourceSubType,
                Code = ResourceSubType.ChromebookLaptop,
                Value = string.Empty,
                Payload = JsonSerializer.Serialize(new ResourceSubTypePayload { ResourceType = ResourceType.Computer })
            };

            var manufacturer = new Classifier
            {
                Id = manufacturerId,
                Type = ClassifierTypes.ResourceManufacturer,
                Code = string.Empty,
                Value = string.Empty
            };

            db.Classifiers.AddRange(
                eduStatus,
                pnaStatus,
                resourceType,
                resourceSubType,
                manufacturer,
                new Classifier
                {
                    Id = resourceModelId,
                    Type = ClassifierTypes.ResourceModelName,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = resourceStatusId,
                    Type = ClassifierTypes.ResourceStatus,
                    Code = ResourceStatus.New,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = documentTypeId,
                    Type = ClassifierTypes.DocumentType,
                    Code = DocumentType.PNA,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = resourceLocationId,
                    Type = ClassifierTypes.ResourceLocation,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = targetGroupId,
                    Type = ClassifierTypes.TargetGroup,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = usagePurposeTypeId,
                    Type = ClassifierTypes.ResourceUsingPurpose,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = acquisitionTypeId,
                    Type = ClassifierTypes.ResourceAcquisitionType,
                    Code = string.Empty,
                    Value = string.Empty
                });

            db.SaveChanges();

            var supervisor = new Supervisor
            {
                Id = supervisorId,
                Code = "c",
                Name = "n"
            };

            var educationalInstitution = new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = eduStatusId,
                SupervisorId = supervisorId
            };

            db.Supervisors.Add(supervisor);

            db.EducationalInstitutions.Add(educationalInstitution);

            db.SaveChanges();

            var application = CreateApplication(db, applicationId, resourceSubTypeId, educationalInstitutionId);

            var resource = new Resource
            {
                Id = resourceId,
                ModelNameId = resourceModelId,
                ResourceStatusId = resourceStatusId,
                ResourceNumber = "ResourceNumber",
                ResourceName = "ResourceName",
                InventoryNumber = "InventoryNumber",
                SerialNumber = "SerialNumber",
                AcquisitionTypeId = acquisitionTypeId,
                UsagePurposeTypeId = usagePurposeTypeId,
                TargetGroupId = targetGroupId,
                ResourceLocationId = resourceLocationId,
                ResourceSubTypeId = resourceSubTypeId,
                ManufacturerId = manufacturerId,
                EducationalInstitutionId = educationalInstitutionId
            };

            var applicationResource = new ApplicationResource
            {
                Id = applicationResourceId,
                PNANumber = "PNANumber",
                ApplicationId = applicationId,
                AssignedResourceId = resourceId
            };

            applicationResource.SetApplicationResourceReturnDeadline(new DateTime(2020, 1, 1));
            applicationResource.SetPnaStatus(pnaStatusId);

            var applicationResourceAttachment = new ApplicationResourceAttachment
            {
                DocumentDate = DateOnly.FromDateTime(DateTime.Now).AddDays(10),
                ApplicationResourceId = applicationResourceId,
                DocumentTypeId = documentTypeId,
                DocumentTemplateId = 1,
                FileId = Guid.NewGuid()
            };

            db.Resources.Add(resource);

            db.ApplicationResources.Add(applicationResource);

            db.ApplicationResourceAttachments.AddRange(
                applicationResourceAttachment,
                new ApplicationResourceAttachment
                {
                    DocumentDate = DateOnly.FromDateTime(DateTime.Now),
                    ApplicationResourceId = applicationResourceId,
                    DocumentTypeId = documentTypeId,
                    DocumentTemplateId = 1,
                    FileId = Guid.NewGuid()
                });

            db.SaveChanges();

            service.ApplicationResources = db.ApplicationResources.AsQueryable();
            classifierService.Data = db.Classifiers.AsQueryable();

            // Act
            var result = await controller.Get();

            var item = result.Value.Items.First();

            // Assert
            Assert.Single(result.Value.Items);
            Assert.Equal(service.ApplicationResources.Count(), result.Value.Total);
            Assert.Equal(applicationResourceId, item.Id);
            Assert.Equal(application.Id, item.Application.Id);
            Assert.Equal(application.ApplicationNumber, item.Application.ApplicationNumber);
            Assert.Equal(educationalInstitution.Id, item.Application.EducationalInstitution.Id);
            Assert.Equal(educationalInstitution.Code, item.Application.EducationalInstitution.Code);
            Assert.Equal(educationalInstitution.Name, item.Application.EducationalInstitution.Name);
            Assert.Equal(resourceType.Id, item.Application.ResourceType.Id);
            Assert.Equal(resourceType.Code, item.Application.ResourceType.Code);
            Assert.Equal(resourceType.Value, item.Application.ResourceType.Value);
            Assert.Equal(resourceSubType.Id, item.Application.ResourceSubType.Id);
            Assert.Equal(resourceSubType.Code, item.Application.ResourceSubType.Code);
            Assert.Equal(resourceSubType.Value, item.Application.ResourceSubType.Value);
            Assert.Equal(application.ResourceTargetPersonClassGrade, item.Application.ResourceTargetPersonClassGrade);
            Assert.Equal(application.ResourceTargetPersonClassParallel, item.Application.ResourceTargetPersonClassParallel);
            Assert.Equal(application.ResourceTargetPersonGroup, item.Application.ResourceTargetPersonGroup);
            Assert.Equal(supervisor.Id, item.Application.Supervisor.Id);
            Assert.Equal(supervisor.Code, item.Application.Supervisor.Code);
            Assert.Equal(supervisor.Name, item.Application.Supervisor.Name);
            Assert.Equal(applicationResource.AssignedResourceReturnDate, item.AssignedResourceReturnDate);
            Assert.Equal(applicationResourceAttachment.DocumentDate, DateOnly.FromDateTime(item.Attachment.DocumentDate));
            Assert.Equal(applicationResource.PNANumber, item.PNANumber);
            Assert.Equal(pnaStatus.Id, item.PNAStatus.Id);
            Assert.Equal(pnaStatus.Code, item.PNAStatus.Code);
            Assert.Equal(pnaStatus.Value, item.PNAStatus.Value);
            Assert.Equal(resource.Id, item.Resource.Id);
            Assert.Equal(resource.InventoryNumber, item.Resource.InventoryNumber);
            Assert.Equal(manufacturer.Id, item.Resource.Manufacturer.Id);
            Assert.Equal(manufacturer.Code, item.Resource.Manufacturer.Code);
            Assert.Equal(manufacturer.Value, item.Resource.Manufacturer.Value);
            Assert.Equal(resource.ResourceNumber, item.Resource.ResourceNumber);
            Assert.Equal(resourceType.Id, item.Resource.ResourceType.Id);
            Assert.Equal(resourceType.Code, item.Resource.ResourceType.Code);
            Assert.Equal(resourceType.Value, item.Resource.ResourceType.Value);
            Assert.Equal(resourceSubType.Id, item.Resource.ResourceSubType.Id);
            Assert.Equal(resourceSubType.Code, item.Resource.ResourceSubType.Code);
            Assert.Equal(resourceSubType.Value, item.Resource.ResourceSubType.Value);
            Assert.Equal(resource.SerialNumber, item.Resource.SerialNumber);
        }

        [Fact]
        public async Task Get_ReturnsData_NoFilter()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;
            const int supervisorId = 1;

            var pnaStatusId = Guid.NewGuid();
            var eduStatusId = Guid.NewGuid();
            var documentTypeId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var manufacturerId = Guid.NewGuid();
            var resourceTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var resourceLocationId = Guid.NewGuid();
            var targetGroupId = Guid.NewGuid();
            var usagePurposeTypeId = Guid.NewGuid();
            var acquisitionTypeId = Guid.NewGuid();
            var resourceStatusId = Guid.NewGuid();
            var resourceModelId = Guid.NewGuid();

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = resourceModelId,
                    Type = ClassifierTypes.ResourceModelName,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = resourceStatusId,
                    Type = ClassifierTypes.ResourceStatus,
                    Code = ResourceStatus.New,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = eduStatusId,
                    Type = ClassifierTypes.EducationalInstitutionStatus,
                    Code = EducationalInstitutionStatus.Active,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = pnaStatusId,
                    Type = ClassifierTypes.PnaStatus,
                    Code = PnaStatus.Issued,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = resourceTypeId,
                    Type = ClassifierTypes.ResourceType,
                    Code = ResourceType.Computer,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = resourceSubTypeId,
                    Type = ClassifierTypes.ResourceSubType,
                    Code = ResourceSubType.ChromebookLaptop,
                    Value = string.Empty,
                    Payload = JsonSerializer.Serialize(new ResourceSubTypePayload { ResourceType = ResourceType.Computer })
                },
                new Classifier
                {
                    Id = manufacturerId,
                    Type = ClassifierTypes.ResourceManufacturer,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = documentTypeId,
                    Type = ClassifierTypes.DocumentType,
                    Code = DocumentType.PNA,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = resourceLocationId,
                    Type = ClassifierTypes.ResourceLocation,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = targetGroupId,
                    Type = ClassifierTypes.TargetGroup,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = usagePurposeTypeId,
                    Type = ClassifierTypes.ResourceUsingPurpose,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = acquisitionTypeId,
                    Type = ClassifierTypes.ResourceAcquisitionType,
                    Code = string.Empty,
                    Value = string.Empty
                });

            db.SaveChanges();

            db.Supervisors.Add(new Supervisor
            {
                Id = supervisorId,
                Code = "c",
                Name = "n"
            });

            db.EducationalInstitutions.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = eduStatusId,
                SupervisorId = supervisorId
            });

            db.SaveChanges();

            var application = CreateApplication(db, applicationId, resourceSubTypeId, educationalInstitutionId);

            var applicationResource = new ApplicationResource
            {
                Id = applicationResourceId,
                PNANumber = "PNANumber",
                ApplicationId = applicationId,
                AssignedResourceId = resourceId
            };

            applicationResource.SetApplicationResourceReturnDeadline(new DateTime(2020, 1, 1));

            db.Resources.Add(new Resource
            {
                Id = resourceId,
                ResourceStatusId = resourceStatusId,
                ResourceNumber = "ResourceNumber",
                ResourceName = "ResourceName",
                InventoryNumber = "InventoryNumber",
                SerialNumber = "SerialNumber",
                AcquisitionTypeId = acquisitionTypeId,
                UsagePurposeTypeId = usagePurposeTypeId,
                TargetGroupId = targetGroupId,
                ResourceLocationId = resourceLocationId,
                ResourceSubTypeId = resourceSubTypeId,
                ManufacturerId = manufacturerId,
                ModelNameId = resourceModelId,
                EducationalInstitutionId = educationalInstitutionId
            });

            var resource = new ApplicationResource
            {
                Id = applicationResourceId,
                PNANumber = "PNANumber",
                ApplicationId = applicationId,
                AssignedResourceId = resourceId
            };

            resource.SetPnaStatus(pnaStatusId);

            db.ApplicationResources.Add(resource);
            db.ApplicationResources.Add(resource);


            db.ApplicationResourceAttachments.Add(new ApplicationResourceAttachment
            {
                DocumentDate = DateOnly.FromDateTime(DateTime.Now),
                ApplicationResourceId = applicationResourceId,
                DocumentTypeId = documentTypeId,
                DocumentTemplateId = 1,
                FileId = Guid.NewGuid()
            });

            db.SaveChanges();

            service.ApplicationResources = db.ApplicationResources.AsQueryable();
            classifierService.Data = db.Classifiers.AsQueryable();

            var pagingRequest = new PagingRequest
            {
                Page = 1,
                Take = 1
            };

            // Act
            var result = await controller.Get(paging: pagingRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(service.ApplicationResources.Count(), result.Value.Total);
        }

        [Fact]
        public async Task GetPna_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            service.Pna = "pna";

            // Act
            var result = await controller.GetPna(id);

            // Assert
            Assert.Equal(service.Pna, result.Value);
            Assert.Equal(service.GetPnaAsyncCalledWith, id);
        }

        [Fact]
        public async Task DownloadPna_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            service.PnaPdf = new FileDto
            {
                FileName = "fileName.pdf",
                ContentType = MediaTypeNames.Application.Pdf,
                Content = Encoding.UTF8.GetBytes("content")
            };

            // Act
            var result = await controller.DownloadPna(id);

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(service.GetPnaPdfAsyncCalledWith, id);
        }

        [Fact]
        public async Task GetExploitationRules_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            service.ExploitationRules = "exploitationRules";

            // Act
            var result = await controller.GetExploitationRules(id);

            // Assert
            Assert.Equal(service.ExploitationRules, result.Value);
            Assert.Equal(service.GetExploitationRulesAsyncCalledWith, id);
        }

        [Fact]
        public async Task DownloadExploitationRules_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            service.ExploitationRulesPdf = new FileDto
            {
                FileName = "fileName.pdf",
                ContentType = MediaTypeNames.Application.Pdf,
                Content = Encoding.UTF8.GetBytes("content")
            };

            // Act
            var result = await controller.DownloadExploitationRules(id);

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(service.GetExploitationRulesPdfAsyncCalledWith, id);
        }

        [Fact]
        public async Task ChangeStatusToLost_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            var request = new ApplicationResourceChangeStatusRequest
            {
                FileToDeleteIds = new[] { Guid.NewGuid() },
                Files = new[] { GetFile(), GetFile() },
                Notes = string.Empty
            };

            // Act
            var result = await controller.ChangeStatusToLost(id, request);

            var withId = service.ChangeStatusToLostAsyncCalledWithId;
            var withDto = service.ChangeStatusToLostAsyncCalledWithDto;

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(withId, id);
            Assert.Equal(withDto.FileToDeleteIds.Count(), request.FileToDeleteIds.Count());
            Assert.True(withDto.FileToDeleteIds.SequenceEqual(request.FileToDeleteIds));
            Assert.Equal(FileSourceType.S3, withDto.Files.First().SourceType);
            Assert.Equal(withDto.Notes, request.Notes);
        }

        [Fact]
        public async Task ChangeStatusToPrepared_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            // Act
            var result = await controller.ChangeStatusToPrepared(id);

            var with = service.ChangeStatusToPreparedAsyncCalledWith;

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(with, id);
        }

        [Fact]
        public async Task ChangeStatusToStolen_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            var request = new ApplicationResourceChangeStatusRequest
            {
                FileToDeleteIds = new[] { Guid.NewGuid() },
                Files = new[] { GetFile(), GetFile() },
                Notes = string.Empty
            };

            // Act
            var result = await controller.ChangeStatusToStolen(id, request);

            var withId = service.ChangeStatusToStolenAsyncCalledWithId;
            var withDto = service.ChangeStatusToStolenAsyncCalledWithDto;

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(withId, id);
            Assert.Equal(withDto.FileToDeleteIds.Count(), request.FileToDeleteIds.Count());
            Assert.True(withDto.FileToDeleteIds.SequenceEqual(request.FileToDeleteIds));
            Assert.Equal(FileSourceType.S3, withDto.Files.First().SourceType);
            Assert.Equal(withDto.Notes, request.Notes);
        }

        [Fact]
        public async Task CreateWithDraftStatus_Succeeds()
        {
            // Assign
            var model = new ApplicationResourceCreateRequest
            {
                ApplicationId = Guid.NewGuid(),
                AssignedResourceId = Guid.NewGuid(),
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            // Act
            var result = await controller.CreateWithDraftStatus(model);

            var with = service.CreateWithDraftStatusAsyncCalledWith;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(with.ApplicationId, model.ApplicationId);
            Assert.Equal(with.AssignedResourceId, model.AssignedResourceId);
            Assert.Equal(with.AssignedResourceReturnDate, model.AssignedResourceReturnDate);
            Assert.Equal(with.Notes, model.Notes);
        }

        [Fact]
        public async Task CreateWithPreparedStatus_Succeeds()
        {
            // Assign
            var model = new ApplicationResourceCreateRequest
            {
                ApplicationId = Guid.NewGuid(),
                AssignedResourceId = Guid.NewGuid(),
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            // Act
            var result = await controller.CreateWithPreparedStatus(model);

            var with = service.CreateWithPreparedStatusAsyncCalledWith;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(with.ApplicationId, model.ApplicationId);
            Assert.Equal(with.AssignedResourceId, model.AssignedResourceId);
            Assert.Equal(with.AssignedResourceReturnDate, model.AssignedResourceReturnDate);
            Assert.Equal(with.Notes, model.Notes);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            // Assign
            var model = new ApplicationResourceUpdateRequest
            {
                AssignedResourceId = Guid.NewGuid(),
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            var id = Guid.NewGuid();

            // Act
            var result = await controller.Update(id, model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(service.UpdateAsyncCalledWithDto.AssignedResourceId, model.AssignedResourceId);
            Assert.Equal(service.UpdateAsyncCalledWithDto.AssignedResourceReturnDate, model.AssignedResourceReturnDate);
            Assert.Equal(service.UpdateAsyncCalledWithDto.Notes, model.Notes);
            Assert.Equal(service.UpdateAsyncCalledWithId, id);
        }

        [Fact]
        public async Task SetReturnDeadline_Succeeds()
        {
            // Assign
            var request = new ApplicationResourceReturnDeadlineRequest
            {
                ApplicationResourceIds = new List<Guid>() {
                    Guid.NewGuid(),
                },
                AssignedResourceReturnDate = DateTime.UtcNow
            };

            // Act
            var result = await controller.SetReturnDeadline(request, new NotificationOptions());

            var withDto = service.SetReturnDeadlineAsyncCalledWithDto;

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(withDto.ApplicationResourceIds, request.ApplicationResourceIds);
            Assert.Equal(withDto.AssignedResourceReturnDate, request.AssignedResourceReturnDate);
        }

        [Fact]
        public async Task Return_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            var request = new ApplicationResourceReturnEditRequest
            {
                ResourceStatusId = Guid.NewGuid(),
                ReturnResourceStateId = Guid.NewGuid(),
                ReturnResourceDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            // Act
            var result = await controller.Return(id, request);

            var withId = service.ReturnAsyncCalledWithId;
            var withDto = service.ReturnAsyncCalledWithDto;

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(withId, id);
            Assert.Equal(withDto.ResourceStatusId, request.ResourceStatusId);
            Assert.Equal(withDto.ReturnResourceStateId, request.ReturnResourceStateId);
            Assert.Equal(withDto.ReturnResourceDate, request.ReturnResourceDate);
            Assert.Equal(withDto.Notes, request.Notes);
        }

        [Fact]
        public async Task Sign_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            // Act
            var result = await controller.Sign(id);

            var with = service.SignAsyncCalledWith;

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(with, id);
        }

        [Fact]
        public async Task Cancel_Succeeds()
        {
            using var db = ServiceFactory.ConnectDb();

            var reasonId = Guid.NewGuid();

            var reason = new Classifier
            {
                Id = reasonId,
                Type = ClassifierTypes.PnaCancelingReason,
                Code = string.Empty,
                Value = string.Empty
            };

            db.Classifiers.Add(reason);

            // Assign
            var id = Guid.NewGuid();

            ApplicationResourceCancelRequest model = new ApplicationResourceCancelRequest
            {
                ReasonId = reasonId,
                Description = string.Empty,
                ChangeApplicationStatusToWithdrawn = true
            };

            // Act
            var result = await controller.Cancel(id, model);

            var withId = service.CancelAsyncCalledWithId;
            var withDto = service.CancelAsyncCalledWithDto;

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(withDto.ReasonId, model.ReasonId);
            Assert.Equal(withDto.Description, model.Description);
            Assert.Equal(withDto.ChangeApplicationStatusToWithdrawn, model.ChangeApplicationStatusToWithdrawn);
            Assert.Equal(withId, id);
        }

        private IFormFile GetFile()
        {
            byte[] bytes = Encoding.UTF8.GetBytes("test");

            var file = new FormFile(
                baseStream: new MemoryStream(bytes),
                baseStreamOffset: 0,
                length: bytes.Length,
                name: "test",
                fileName: "test.pdf"
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = MediaTypeNames.Application.Pdf
            };

            return file;
        }

        private Domain.Entities.Application CreateApplication(AppDbContext db, Guid applicationId, Guid resourceSubTypeId, int educationalInstitutionId)
        {
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var personTechnicalId = Guid.NewGuid();
            var resourceTargetPersonEducationalStatusId = Guid.NewGuid();

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = applicationStatusId,
                    Type = ClassifierTypes.ApplicationStatus,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = submitterTypeId,
                    Type = ClassifierTypes.ApplicantRole,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = resourceTargetPersonTypeId,
                    Type = ClassifierTypes.ResourceTargetPersonType,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = resourceTargetPersonEducationalStatusId,
                    Type = ClassifierTypes.ResourceTargetPersonEducationalStatus,
                    Code = ResourceTargetPersonEducationalStatus.Studying,
                    Value = string.Empty
                });

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = personTechnicalId
            });

            db.Persons.AddAsync(new Person
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                PrivatePersonalIdentifier = string.Empty,
                PersonTechnicalId = personTechnicalId
            });

            db.SaveChanges();

            var applications = new Domain.Entities.Application(applicationStatusId)
            {
                Id = applicationId,
                ApplicationNumber = Guid.NewGuid().ToString().Substring(0, 10),
                ApplicationDate = DateTime.UtcNow,
                EducationalInstitutionId = educationalInstitutionId,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonGroup = "group",
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonId = personTechnicalId,
                ResourceTargetPersonEducationalStatusId = resourceTargetPersonEducationalStatusId
            };

            db.Applications.Add(applications);

            db.SaveChanges();

            return applications;
        }
    }
}
