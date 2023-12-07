using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using Izm.Rumis.Infrastructure;
using Izm.Rumis.Infrastructure.Services;
using Izm.Rumis.Infrastructure.Viis;
using Izm.Rumis.Infrastructure.Vraa;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using static Izm.Rumis.Api.Models.EServiceChangeStatusRequest;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public sealed class EServicesControllerTests
    {
        [Fact]
        public async Task CreateApplication_CheckData()
        {
            // Assign
            var request = new EServiceApplicationCreateRequest
            {
                ApplicationSocialStatuses = new Guid[]
                {
                    Guid.NewGuid(),
                    Guid.NewGuid()
                },
                ApplicationStatusHistory = "ApplicationStatusHistory",
                ApplicationStatusId = Guid.NewGuid(),
                EducationalInstitutionId = 1,
                Notes = "Notes",
                ResourceSubTypeId = Guid.NewGuid(),
                ResourceTargetPerson = new EServiceApplicationCreateRequest.PersonData
                {
                    ContactInformation = new EServiceApplicationCreateRequest.PersonData.ContactData[]
                    {
                        new EServiceApplicationCreateRequest.PersonData.ContactData
                        {
                            TypeId = Guid.NewGuid(),
                            Value = "Value"
                        }
                    },
                    FirstName = "FirstName",
                    LastName = "LastName",
                    PrivatePersonalIdentifier = "00000000000"
                },
                ResourceTargetPersonClassGrade = 3,
                ResourceTargetPersonClassParallel = "ResourceTargetPersonClassParallel",
                ResourceTargetPersonEducationalProgram = "",
                ResourceTargetPersonEducationalStatusId = Guid.NewGuid(),
                ResourceTargetPersonEducationalSubStatusId = Guid.NewGuid(),
                ResourceTargetPersonGroup = "ResourceTargetPersonGroup",
                ResourceTargetPersonTypeId = Guid.NewGuid(),
                ResourceTargetPersonWorkStatusId = Guid.NewGuid(),
                SocialStatus = true,
                SocialStatusApproved = true,
                SubmitterContactInformation = new EServiceApplicationCreateRequest.PersonData.ContactData[]
                {
                    new EServiceApplicationCreateRequest.PersonData.ContactData
                        {
                            TypeId = Guid.NewGuid(),
                            Value = "Value1"
                        }
                },
                SubmitterTypeId = Guid.NewGuid()
            };

            var eServicesService = ServiceFactory.CreateEServicesService();

            var controller = GetController(
                eServicesService: eServicesService
                );

            // Act
            _ = await controller.CreateApplication(request);

            var with = eServicesService.CreateApplicationAsyncCalledWith;

            var resourceTargetContactWith = eServicesService.CreateApplicationAsyncCalledWith.ResourceTargetPerson.ContactInformation.First();
            var resourceTargetContact = request.ResourceTargetPerson.ContactInformation.First();

            var submitterContactWith = eServicesService.CreateApplicationAsyncCalledWith.SubmitterContactData.First();
            var submitterContact = request.SubmitterContactInformation.First();

            // Assert
            Assert.Equal(with.ApplicationSocialStatuses.Count(), request.ApplicationSocialStatuses.Count());
            Assert.True(with.ApplicationSocialStatuses.SequenceEqual(request.ApplicationSocialStatuses));
            Assert.Equal(with.ApplicationStatusHistory, request.ApplicationStatusHistory);
            Assert.Equal(with.EducationalInstitutionId, request.EducationalInstitutionId);
            Assert.Equal(with.Notes, request.Notes);
            Assert.Equal(with.ResourceSubTypeId, request.ResourceSubTypeId);
            Assert.Equal(with.ResourceTargetPerson.ContactInformation.Count(), request.ResourceTargetPerson.ContactInformation.Count());
            Assert.Equal(resourceTargetContactWith.TypeId, resourceTargetContact.TypeId);
            Assert.Equal(resourceTargetContactWith.Value, resourceTargetContact.Value);
            Assert.Equal(with.ResourceTargetPerson.FirstName, request.ResourceTargetPerson.FirstName);
            Assert.Equal(with.ResourceTargetPerson.LastName, request.ResourceTargetPerson.LastName);
            Assert.Equal(with.ResourceTargetPerson.PrivatePersonalIdentifier, request.ResourceTargetPerson.PrivatePersonalIdentifier);
            Assert.Equal(with.ResourceTargetPersonClassGrade, request.ResourceTargetPersonClassGrade);
            Assert.Equal(with.ResourceTargetPersonClassParallel, request.ResourceTargetPersonClassParallel);
            Assert.Equal(with.ResourceTargetPersonEducationalProgram, request.ResourceTargetPersonEducationalProgram);
            Assert.Equal(with.ResourceTargetPersonEducationalStatusId, request.ResourceTargetPersonEducationalStatusId);
            Assert.Equal(with.ResourceTargetPersonEducationalSubStatusId, request.ResourceTargetPersonEducationalSubStatusId);
            Assert.Equal(with.ResourceTargetPersonGroup, request.ResourceTargetPersonGroup);
            Assert.Equal(with.ResourceTargetPersonTypeId, request.ResourceTargetPersonTypeId);
            Assert.Equal(with.ResourceTargetPersonWorkStatusId, request.ResourceTargetPersonWorkStatusId);
            Assert.Equal(with.SocialStatus, request.SocialStatus);
            Assert.Equal(with.SocialStatusApproved, request.SocialStatusApproved);
            Assert.Equal(with.SubmitterContactData.Count(), request.SubmitterContactInformation.Count());
            Assert.Equal(submitterContactWith.TypeId, submitterContact.TypeId);
            Assert.Equal(submitterContactWith.Value, submitterContact.Value);
            Assert.Equal(with.SubmitterTypeId, request.SubmitterTypeId);
        }

        [Fact]
        public async Task CreateApplication_Succeeds()
        {
            // Assign
            var eServicesService = ServiceFactory.CreateEServicesService();

            var controller = GetController(
                eServicesService: eServicesService
                );

            // Act
            _ = await controller.CreateApplication(new EServiceApplicationCreateRequest());

            // Assert
            Assert.NotNull(eServicesService.CreateApplicationAsyncCalledWith);
        }

        [Fact]
        public async Task CheckDuplicate_Applications_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;

            var applicationStatusId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var eduStatusId = Guid.NewGuid();

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = applicationStatusId,
                    Type = ClassifierTypes.ApplicationStatus,
                    Code = ApplicationStatus.Submitted,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = resourceSubTypeId,
                    Type = ClassifierTypes.ResourceSubType,
                    Code = ResourceSubType.ChromebookLaptop,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = eduStatusId,
                    Type = ClassifierTypes.EducationalInstitutionStatus,
                    Code = EducationalInstitutionStatus.Active,
                    Value = string.Empty
                });

            db.SaveChanges();

            db.EducationalInstitutions.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = eduStatusId,
                SupervisorId = 0
            });

            db.SaveChanges();

            CreateApplication(
                db: db,
                id: Guid.NewGuid(),
                applicationStatusId: applicationStatusId,
                personTechnicalId: Guid.NewGuid(),
                resourceSubTypeId: resourceSubTypeId,
                eduInstId: educationalInstitutionId);

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.Applications = db.Applications.AsQueryable();

            var model = new EServiceApplicationCheckDuplicateRequest
            {
                PrivatePersonalIdentifier = "00000000002",
                ResourceSubTypeId = Guid.NewGuid()
            };

            var controller = GetController(eServicesService: eServicesService);

            // Act
            _ = await controller.CheckApplicationDuplicate(model);

            // Assert
            Assert.NotNull(eServicesService.GetApplicationDuplicatesAsyncCalledWith);
        }

        [Theory]
        [InlineData(ApplicationStatus.Submitted, PnaStatus.Preparing, ApplicationStatus.Submitted)]
        [InlineData(ApplicationStatus.Postponed, PnaStatus.Preparing, ApplicationStatus.Postponed)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Preparing, ApplicationStatus.Confirmed)]
        [InlineData(ApplicationStatus.Declined, PnaStatus.Preparing, ApplicationStatus.Declined)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Prepared, PnaStatus.Prepared)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Issued, PnaStatus.Issued)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Returned, PnaStatus.Returned)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Stolen, PnaStatus.Stolen)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Lost, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Withdrawn, PnaStatus.Preparing, ApplicationStatus.Withdrawn)]
        public async Task GetApplications_Succeeds(string applicationStatusCode, string pnaStatusCode, string resultCode)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;

            var pnaStatusId = Guid.NewGuid();
            var eduStatusId = Guid.NewGuid();
            var documentTypeId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTypeId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var personTechnicalId = Guid.NewGuid();
            var applicationStatusId = Guid.NewGuid();

            var vraaUser = ServiceFactory.CreateVraaUser();

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = applicationStatusId,
                    Type = ClassifierTypes.ApplicationStatus,
                    Code = applicationStatusCode,
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
                    Code = pnaStatusCode,
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
                    Id = resourceTypeId,
                    Type = ClassifierTypes.ResourceType,
                    Code = ResourceType.Computer,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = documentTypeId,
                    Type = ClassifierTypes.DocumentType,
                    Code = DocumentType.PNA,
                    Value = string.Empty
                });

            db.SaveChanges();

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = personTechnicalId
            });

            var person = new Person
            {
                FirstName = vraaUser.FirstName,
                LastName = vraaUser.LastName,
                PrivatePersonalIdentifier = vraaUser.PrivatePersonalIdentifier,
                PersonTechnicalId = personTechnicalId
            };

            db.Persons.Add(person);

            db.EducationalInstitutions.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = eduStatusId,
                SupervisorId = 1
            });

            db.SaveChanges();

            var application = CreateApplication(
                db: db,
                id: applicationId,
                applicationStatusId: applicationStatusId,
                personTechnicalId: personTechnicalId,
                resourceSubTypeId: resourceSubTypeId,
                eduInstId: educationalInstitutionId);

            var resource = new ApplicationResource
            {
                Id = applicationResourceId,
                PNANumber = "PNANumber",
                ApplicationId = applicationId
            };

            resource.SetApplicationResourceReturnDeadline(new DateTime(2020, 1, 1));
            resource.SetPnaStatus(pnaStatusId);

            db.ApplicationResources.Add(resource);

            db.ApplicationResourceAttachments.Add(new ApplicationResourceAttachment
            {
                DocumentDate = DateOnly.FromDateTime(DateTime.Now),
                ApplicationResourceId = applicationResourceId,
                DocumentTypeId = documentTypeId
            });

            db.SaveChanges();

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.Applications = db.Applications.AsQueryable();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var controller = GetController(
                    eServicesService: eServicesService,
                    vraaUser: vraaUser,
                    gdprAuditService: gdprAuditService);

            // Act
            var result = await controller.GetApplications();

            var item = result.Value.FirstOrDefault();

            // Assert
            Assert.NotNull(item);
            Assert.Equal(resultCode, item.ApplicationStatus.Code);
            Assert.NotNull(gdprAuditService.TraceRangeAsyncCalledWith);
        }

        [Fact]
        public async Task GetApplicationById_Succeeds()
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
            var modelNameId = Guid.NewGuid();
            var resourceStatusId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTypeId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var personTechnicalId = Guid.NewGuid();
            var fileId = Guid.NewGuid();
            var resourceLocationId = Guid.NewGuid();
            var targetGroupId = Guid.NewGuid();
            var usagePurposeTypeId = Guid.NewGuid();
            var acquisitionTypeId = Guid.NewGuid();
            var applicationStatusId = Guid.NewGuid();

            var vraaUser = ServiceFactory.CreateVraaUser();

            var eduInstStatus = new Classifier
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

            var resourceStatus = new Classifier
            {
                Id = resourceStatusId,
                Type = ClassifierTypes.ResourceStatus,
                Code = string.Empty,
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

            var resourceType = new Classifier
            {
                Id = resourceTypeId,
                Type = ClassifierTypes.ResourceType,
                Code = ResourceType.Computer,
                Value = string.Empty
            };

            var manufacturer = new Classifier
            {
                Id = manufacturerId,
                Type = ClassifierTypes.ResourceManufacturer,
                Code = string.Empty,
                Value = string.Empty
            };

            var modelName = new Classifier
            {
                Id = modelNameId,
                Type = ClassifierTypes.ResourceModelName,
                Code = string.Empty,
                Value = string.Empty
            };

            var documentType = new Classifier
            {
                Id = documentTypeId,
                Type = ClassifierTypes.DocumentType,
                Code = DocumentType.PNA,
                Value = string.Empty
            };

            db.Classifiers.AddRange(
                eduInstStatus,
                pnaStatus,
                resourceStatus,
                resourceSubType,
                resourceType,
                manufacturer,
                modelName,
                documentType,
                new Classifier
                {
                    Id = applicationStatusId,
                    Type = ClassifierTypes.ApplicationStatus,
                    Code = ApplicationStatus.Submitted,
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


            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = personTechnicalId
            });

            var person = new Person
            {
                FirstName = vraaUser.FirstName,
                LastName = vraaUser.LastName,
                PrivatePersonalIdentifier = vraaUser.PrivatePersonalIdentifier,
                PersonTechnicalId = personTechnicalId
            };

            db.Persons.Add(person);

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

            var application = CreateApplication(
                db: db,
                id: applicationId,
                applicationStatusId: applicationStatusId,
                personTechnicalId: personTechnicalId,
                resourceSubTypeId: resourceSubTypeId,
                eduInstId: educationalInstitutionId);

            var resource = new Resource
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
                ModelNameId = modelNameId,
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

            var attachment = new ApplicationResourceAttachment
            {
                DocumentDate = DateOnly.FromDateTime(DateTime.Now),
                ApplicationResourceId = applicationResourceId,
                DocumentTypeId = documentTypeId,
                DocumentTemplateId = 1,
                FileId = fileId
            };

            var file = new File
            {
                Id = fileId,
                Name = "Name",
                ContentType = "ContentType",
                Extension = "Extension"
            };

            db.Resources.Add(resource);
            db.ApplicationResources.Add(applicationResource);
            db.ApplicationResourceAttachments.Add(attachment);
            db.Files.Add(file);

            db.SaveChanges();

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.Applications = db.Applications.AsQueryable();

            var classifierService = ServiceFactory.CreateClassifierService();

            classifierService.Data = db.Classifiers.AsNoTracking();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var controller = GetController(
                    eServicesService: eServicesService,
                    vraaUser: vraaUser,
                    classifierService: classifierService,
                    gdprAuditService: gdprAuditService
                    );

            // Act
            var result = await controller.GetApplicationById(applicationId);

            var item = result.Value;

            // Assert
            Assert.NotNull(result.Value);
            Assert.Equal(application.ApplicationDate, item.ApplicationDate);
            Assert.Equal(application.ApplicationNumber, item.ApplicationNumber);
            Assert.Equal(application.ApplicationStatusId, item.ApplicationStatus.Id);
            Assert.Equal(application.ApplicationStatus.Code, item.ApplicationStatus.Code);
            Assert.Equal(application.ApplicationStatus.Value, item.ApplicationStatus.Value);
            Assert.Equal(application.ContactPersonId, item.ContactPerson.Id);
            Assert.Equal(application.EducationalInstitutionId, item.EducationalInstitution.Id);
            Assert.Equal(application.EducationalInstitution.Code, item.EducationalInstitution.Code);
            Assert.Equal(application.EducationalInstitution.Name, item.EducationalInstitution.Name);
            Assert.Equal(application.ResourceSubTypeId, item.ResourceSubType.Id);
            Assert.Equal(application.ResourceSubType.Code, item.ResourceSubType.Code);
            Assert.Equal(application.ResourceSubType.Value, item.ResourceSubType.Value);
            Assert.Equal(application.ResourceTargetPersonId, item.ResourceTargetPerson.Id);
            Assert.Equal(applicationResource.Id, item.ApplicationResources.First().Id);
            Assert.Equal(applicationResource.AssignedResourceReturnDate, item.ApplicationResources.First().AssignedResourceReturnDate);
            Assert.Equal(applicationResource.PNANumber, item.ApplicationResources.First().PNANumber);
            Assert.Equal(pnaStatus.Id, item.ApplicationResources.First().PNAStatus.Id);
            Assert.Equal(pnaStatus.Code, item.ApplicationResources.First().PNAStatus.Code);
            Assert.Equal(pnaStatus.Value, item.ApplicationResources.First().PNAStatus.Value);
            Assert.Equal(resource.Id, item.ApplicationResources.First().Resource.Id);
            Assert.Equal(resource.AcquisitionsValue, item.ApplicationResources.First().Resource.AcquisitionsValue);
            Assert.Equal(resource.InventoryNumber, item.ApplicationResources.First().Resource.InventoryNumber);
            Assert.Equal(resource.Notes, item.ApplicationResources.First().Resource.Notes);
            Assert.Equal(resource.SerialNumber, item.ApplicationResources.First().Resource.SerialNumber);
            Assert.Equal(resource.ModelIdentifier, item.ApplicationResources.First().Resource.ModelIdentifier);
            Assert.Equal(manufacturer.Id, item.ApplicationResources.First().Resource.Manufacturer.Id);
            Assert.Equal(manufacturer.Code, item.ApplicationResources.First().Resource.Manufacturer.Code);
            Assert.Equal(manufacturer.Value, item.ApplicationResources.First().Resource.Manufacturer.Value);
            Assert.Equal(modelName.Id, item.ApplicationResources.First().Resource.ModelName.Id);
            Assert.Equal(modelName.Code, item.ApplicationResources.First().Resource.ModelName.Code);
            Assert.Equal(modelName.Value, item.ApplicationResources.First().Resource.ModelName.Value);
            Assert.Equal(resourceStatus.Id, item.ApplicationResources.First().Resource.ResourceStatus.Id);
            Assert.Equal(resourceStatus.Code, item.ApplicationResources.First().Resource.ResourceStatus.Code);
            Assert.Equal(resourceStatus.Value, item.ApplicationResources.First().Resource.ResourceStatus.Value);
            Assert.Equal(attachment.Id, item.ApplicationResources.First().Attachments.First().Id);
            Assert.Equal(attachment.DocumentDate.ToDateTime(new TimeOnly()), item.ApplicationResources.First().Attachments.First().DocumentDate);
            Assert.Equal(documentType.Id, item.ApplicationResources.First().Attachments.First().DocumentType.Id);
            Assert.Equal(documentType.Code, item.ApplicationResources.First().Attachments.First().DocumentType.Code);
            Assert.Equal(documentType.Value, item.ApplicationResources.First().Attachments.First().DocumentType.Value);
            Assert.Equal(file.ContentType, item.ApplicationResources.First().Attachments.First().File.ContentType);
            Assert.Equal(file.Extension, item.ApplicationResources.First().Attachments.First().File.Extension);
            Assert.Equal(file.Name, item.ApplicationResources.First().Attachments.First().File.Name);
            Assert.Equal(person.Id, item.ContactPerson.Persons.First().Id);
            Assert.Equal(person.ActiveFrom, item.ContactPerson.Persons.First().ActiveFrom);
            Assert.Equal(person.FirstName, item.ContactPerson.Persons.First().FirstName);
            Assert.Equal(person.LastName, item.ContactPerson.Persons.First().LastName);
            Assert.Equal(person.PrivatePersonalIdentifier, item.ContactPerson.Persons.First().PrivatePersonalIdentifier);
            Assert.Equal(person.Id, item.ResourceTargetPerson.Persons.First().Id);
            Assert.Equal(person.ActiveFrom, item.ResourceTargetPerson.Persons.First().ActiveFrom);
            Assert.Equal(person.FirstName, item.ResourceTargetPerson.Persons.First().FirstName);
            Assert.Equal(person.LastName, item.ResourceTargetPerson.Persons.First().LastName);
            Assert.Equal(person.PrivatePersonalIdentifier, item.ResourceTargetPerson.Persons.First().PrivatePersonalIdentifier);
            Assert.Equal(resourceType.Id, item.ApplicationResources.First().Resource.ResourceType.Id);
            Assert.Equal(resourceType.Code, item.ApplicationResources.First().Resource.ResourceType.Code);
            Assert.Equal(resourceType.Value, item.ApplicationResources.First().Resource.ResourceType.Value);
            Assert.NotNull(gdprAuditService.TraceRangeAsyncCalledWith);
        }

        [Fact]
        public async Task GetApplicationById_Succeeds_EmptyContactPerson()
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
            var modelNameId = Guid.NewGuid();
            var resourceStatusId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTypeId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var personTechnicalId = Guid.NewGuid();
            var fileId = Guid.NewGuid();
            var resourceLocationId = Guid.NewGuid();
            var targetGroupId = Guid.NewGuid();
            var usagePurposeTypeId = Guid.NewGuid();
            var acquisitionTypeId = Guid.NewGuid();
            var applicationStatusId = Guid.NewGuid();

            var vraaUser = ServiceFactory.CreateVraaUser();

            db.Classifiers.AddRange(
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
                    Id = resourceStatusId,
                    Type = ClassifierTypes.ResourceStatus,
                    Code = string.Empty,
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
                    Id = resourceTypeId,
                    Type = ClassifierTypes.ResourceType,
                    Code = ResourceType.Computer,
                    Value = string.Empty
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
                    Id = modelNameId,
                    Type = ClassifierTypes.ResourceModelName,
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
                },
                new Classifier
                {
                    Id = applicationStatusId,
                    Type = ClassifierTypes.ApplicationStatus,
                    Code = ApplicationStatus.Submitted,
                    Value = string.Empty
                });

            db.SaveChanges();


            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = personTechnicalId
            });

            db.Persons.Add(new Person
            {
                FirstName = vraaUser.FirstName,
                LastName = vraaUser.LastName,
                PrivatePersonalIdentifier = vraaUser.PrivatePersonalIdentifier + "1",
                PersonTechnicalId = personTechnicalId
            });

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

            CreateApplication(
                db: db,
                id: applicationId,
                applicationStatusId: applicationStatusId,
                personTechnicalId: personTechnicalId,
                resourceSubTypeId: resourceSubTypeId,
                eduInstId: educationalInstitutionId);

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
                ModelNameId = modelNameId,
                EducationalInstitutionId = educationalInstitutionId
            });

            var resource = new ApplicationResource
            {
                Id = applicationResourceId,
                PNANumber = "PNANumber",
                ApplicationId = applicationId,
                AssignedResourceId = resourceId
            };

            resource.SetApplicationResourceReturnDeadline(new DateTime(2020, 1, 1));
            resource.SetPnaStatus(pnaStatusId);

            db.ApplicationResources.Add(resource);

            db.ApplicationResourceAttachments.Add(new ApplicationResourceAttachment
            {
                DocumentDate = DateOnly.FromDateTime(DateTime.Now),
                ApplicationResourceId = applicationResourceId,
                DocumentTypeId = documentTypeId,
                DocumentTemplateId = 1,
                FileId = fileId
            });

            db.Files.Add(new File
            {
                Id = fileId,
                Name = "Name",
                ContentType = "ContentType",
                Extension = "Extension"
            });

            db.SaveChanges();

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.Applications = db.Applications.AsQueryable();

            var classifierService = ServiceFactory.CreateClassifierService();
            classifierService.Data = db.Classifiers.AsNoTracking();

            var controller = GetController(
                    eServicesService: eServicesService,
                    vraaUser: vraaUser,
                    classifierService: classifierService);

            // Act
            var result = await controller.GetApplicationById(applicationId);

            var item = result.Value;

            // Assert
            Assert.NotNull(result.Value);
            Assert.Null(item.ContactPerson);
        }

        [Fact]
        public async Task GetApplicationResourcePna_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.ApplicationResourcePna = "pna";

            var controller = GetController(eServicesService: eServicesService);

            // Act
            var result = await controller.GetApplicationResourcePna(id);

            // Assert
            Assert.Equal(eServicesService.ApplicationResourcePna, result.Value);
            Assert.Equal(eServicesService.GetApplicationResourcePnaAsyncCalledWith, id);
        }

        [Fact]
        public async Task GetApplicationResourcePnaSample_Succeeds()
        {
            // Assign
            const int eduInstId = 1;

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.ApplicationResourcePna = "pna";

            var controller = GetController(eServicesService: eServicesService);

            // Act
            var result = await controller.GetApplicationResourcePnaSample(eduInstId);

            // Assert
            Assert.Equal(eServicesService.ApplicationResourceDocumentSample, result.Value);
            Assert.Equal(DocumentType.PNA, eServicesService.GetDocumentSampleAsyncCalledWith);
        }

        [Fact]
        public async Task DownloadApplicationResourcePna_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.ApplicationResourcePnaPdf = new FileDto
            {
                FileName = "fileName.pdf",
                ContentType = MediaTypeNames.Application.Pdf,
                Content = Encoding.UTF8.GetBytes("content")
            };

            var controller = GetController(eServicesService: eServicesService);

            // Act
            var result = await controller.DownloadApplicationResourcePna(id);

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(eServicesService.GetApplicationResourcePnaPdfAsyncCalledWith, id);
        }

        [Fact]
        public async Task GetApplicationResourceExploitationRules_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.ApplicationResourceExploitationRules = "exploitationRules";

            var controller = GetController(eServicesService: eServicesService);

            // Act
            var result = await controller.GetApplicationResourceExploitationRules(id);

            // Assert
            Assert.Equal(eServicesService.ApplicationResourceExploitationRules, result.Value);
            Assert.Equal(eServicesService.GetApplicationResourceExploitationRulesAsyncCalledWith, id);
        }

        [Fact]
        public async Task GetApplicationResourceExploitationRulesSample_Succeeds()
        {
            // Assign
            const int eduInstId = 1;

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.ApplicationResourcePna = "pna";

            var controller = GetController(eServicesService: eServicesService);

            // Act
            var result = await controller.GetApplicationResourceExploitationRulesSample(eduInstId);

            // Assert
            Assert.Equal(eServicesService.ApplicationResourceDocumentSample, result.Value);
            Assert.Equal(DocumentType.ExploitationRules, eServicesService.GetDocumentSampleAsyncCalledWith);
        }

        [Fact]
        public async Task DownloadApplicationResourceExploitationRules_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.ApplicationResourceExploitationRulesPdf = new FileDto
            {
                FileName = "fileName.pdf",
                ContentType = MediaTypeNames.Application.Pdf,
                Content = Encoding.UTF8.GetBytes("content")
            };

            var controller = GetController(eServicesService: eServicesService);

            // Act
            var result = await controller.DownloadApplicationResourceExploitationRules(id);

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(eServicesService.GetApplicationResourceExploitationRulesPdfAsyncCalledWith, id);
        }

        [Fact]
        public async Task GetByType_Classifiers_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var eServicesService = ServiceFactory.CreateEServicesService();

            await db.Classifiers.AddRangeAsync(
                new Classifier
                {
                    Id = Guid.NewGuid(),
                    Type = ClassifierTypes.ClassifierType,
                    Code = string.Empty,
                    Payload = string.Empty,
                    Value = string.Empty,
                },
                new Classifier
                {
                    Id = Guid.NewGuid(),
                    Type = ClassifierTypes.ClassifierType,
                    Code = string.Empty,
                    Payload = string.Empty,
                    Value = string.Empty,
                });
            await db.SaveChangesAsync();

            eServicesService.Classifiers = db.Classifiers.AsQueryable();

            var controller = GetController(
                eServicesService: eServicesService);

            // Act
            var result = await controller.GetByType(Enumerable.Empty<string>());

            // Assert
            Assert.Equal(eServicesService.Classifiers.Count(), result.Value.Count());
        }

        [Fact]
        public async Task Withdraw_Succeeds()
        {
            // Assign
            var controller = GetController();

            // Act
            var result = await controller.WithdrawApplication(Guid.NewGuid());

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ChangeStatusToLost_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            var request = new EServiceChangeStatusRequest
            {
                FileToDeleteIds = new[] { Guid.NewGuid() },
                Files = new Base64File[] {
                    new Base64File {
                        Content = string.Empty,
                        ContentType = "application/pdf",
                        FileName = "input.pdf"
                    }
                },
                Notes = string.Empty
            };

            var eServicesService = ServiceFactory.CreateEServicesService();

            var controller = GetController(
                eServicesService: eServicesService
                );

            // Act
            var result = await controller.ChangeStatusToLost(id, request);

            var withId = eServicesService.ChangeStatusToLostAsyncCalledWithId;
            var withDto = eServicesService.ChangeStatusToLostAsyncCalledWithDto;

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(withId, id);
            Assert.Equal(withDto.FileToDeleteIds.Count(), request.FileToDeleteIds.Count());
            Assert.True(withDto.FileToDeleteIds.SequenceEqual(request.FileToDeleteIds));
            Assert.Equal(withDto.Files.Count(), request.Files.Count());
            Assert.Equal(FileSourceType.S3, withDto.Files.First().SourceType);
            Assert.Equal(withDto.Notes, request.Notes);
        }

        [Fact]
        public async Task ChangeStatusToStolen_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            var request = new EServiceChangeStatusRequest
            {
                FileToDeleteIds = new[] { Guid.NewGuid() },
                Files = new Base64File[] {
                    new Base64File {
                        Content = string.Empty,
                        ContentType = "application/pdf",
                        FileName = "input.pdf"
                    }
                },
                Notes = string.Empty
            };

            var eServicesService = ServiceFactory.CreateEServicesService();

            var controller = GetController(
                eServicesService: eServicesService
                );

            // Act
            var result = await controller.ChangeStatusToStolen(id, request);

            var withId = eServicesService.ChangeStatusToStolenAsyncCalledWithId;
            var withDto = eServicesService.ChangeStatusToStolenAsyncCalledWithDto;

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(withId, id);
            Assert.Equal(withDto.FileToDeleteIds.Count(), request.FileToDeleteIds.Count());
            Assert.True(withDto.FileToDeleteIds.SequenceEqual(request.FileToDeleteIds));
            Assert.Equal(withDto.Files.Count(), request.Files.Count());
            Assert.Equal(FileSourceType.S3, withDto.Files.First().SourceType);
            Assert.Equal(withDto.Notes, request.Notes);
        }

        [Fact]
        public async Task UpdateApplicationContactPersonInformation_Succeeds()
        {
            // Assign
            var applicationId = Guid.NewGuid();

            var request = new EServiceContactPersonUpdateRequest
            {
                ApplicationContactInformation = new List<EServiceContactPersonUpdateRequest.ContactInformation>
                {
                    new EServiceContactPersonUpdateRequest.ContactInformation
                    {
                        TypeId = Guid.NewGuid(),
                        Value = string.Empty
                    },
                    new EServiceContactPersonUpdateRequest.ContactInformation
                    {
                        TypeId = Guid.NewGuid(),
                        Value = string.Empty
                    }
                }
            };

            var eServicesService = ServiceFactory.CreateEServicesService();

            var controller = GetController(
                eServicesService: eServicesService
                );

            // Act
            var result = await controller.UpdateApplicationContactPersonInformation(applicationId, request);

            var with = eServicesService.ChangeSubmitterContactCalledWith;

            var contactWith = with.Item.ContactInformationData.First();
            var contactRequest = request.ApplicationContactInformation.First();

            // Assert
            Assert.Equal(with.Item.ContactInformationData.Count(), request.ApplicationContactInformation.Count());
            Assert.Equal(contactWith.TypeId, contactRequest.TypeId);
            Assert.Equal(contactWith.Value, contactRequest.Value);
            Assert.Equal(applicationId, with.Id);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateApplicationsContactPersonInformation_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;

            var pnaStatusId = Guid.NewGuid();
            var eduStatusId = Guid.NewGuid();
            var documentTypeId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTypeId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var personTechnicalId1 = Guid.NewGuid();
            var personTechnicalId2 = Guid.NewGuid();
            var applicationStatusId = Guid.NewGuid();

            var vraaUser = ServiceFactory.CreateVraaUser();

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = applicationStatusId,
                    Type = ClassifierTypes.ApplicationStatus,
                    Code = ApplicationStatus.Confirmed,
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
                    Id = resourceSubTypeId,
                    Type = ClassifierTypes.ResourceSubType,
                    Code = ResourceSubType.ChromebookLaptop,
                    Value = string.Empty,
                    Payload = JsonSerializer.Serialize(new ResourceSubTypePayload { ResourceType = ResourceType.Computer })
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
                    Id = documentTypeId,
                    Type = ClassifierTypes.DocumentType,
                    Code = DocumentType.PNA,
                    Value = string.Empty
                });

            db.SaveChanges();

            db.PersonTechnicals.AddRange(
                new PersonTechnical
                {
                    Id = personTechnicalId1
                },
                new PersonTechnical
                {
                    Id = personTechnicalId2
                });

            db.Persons.AddRange(
                new Person
                {
                    FirstName = vraaUser.FirstName,
                    LastName = vraaUser.LastName,
                    PrivatePersonalIdentifier = vraaUser.PrivatePersonalIdentifier,
                    PersonTechnicalId = personTechnicalId1
                },
                new Person
                {
                    FirstName = vraaUser.FirstName,
                    LastName = vraaUser.LastName,
                    PrivatePersonalIdentifier = vraaUser.PrivatePersonalIdentifier + "1",
                    PersonTechnicalId = personTechnicalId2
                });

            db.EducationalInstitutions.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = eduStatusId,
                SupervisorId = 1
            });

            db.SaveChanges();

            CreateApplication(
                db: db,
                id: Guid.NewGuid(),
                applicationStatusId: applicationStatusId,
                personTechnicalId: personTechnicalId1,
                resourceSubTypeId: resourceSubTypeId,
                eduInstId: educationalInstitutionId
                );

            CreateApplication(
                db: db,
                id: Guid.NewGuid(),
                applicationStatusId: applicationStatusId,
                personTechnicalId: personTechnicalId1,
                resourceSubTypeId: resourceSubTypeId,
                eduInstId: educationalInstitutionId
                );

            CreateApplication(
                db: db,
                id: Guid.NewGuid(),
                applicationStatusId: applicationStatusId,
                personTechnicalId: personTechnicalId2,
                resourceSubTypeId: resourceSubTypeId,
                eduInstId: educationalInstitutionId
                );

            var request = new EServiceApplicationsContactPersonUpdateRequest
            {
                ResourceTargetPersonId = personTechnicalId1,
                ContactPerson = new EServiceApplicationsContactPersonUpdateRequest.ContactPersonData
                {
                    ApplicationContactInformation = new List<EServiceApplicationsContactPersonUpdateRequest.ContactPersonData.ContactInformation>
                    {
                        new EServiceApplicationsContactPersonUpdateRequest.ContactPersonData.ContactInformation
                        {
                            TypeId = Guid.NewGuid(),
                            Value = string.Empty
                        },
                        new EServiceApplicationsContactPersonUpdateRequest.ContactPersonData.ContactInformation
                        {
                            TypeId = Guid.NewGuid(),
                            Value = string.Empty
                        }
                    }
                }
            };

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.Applications = db.Applications.AsQueryable();

            var controller = GetController(
                eServicesService: eServicesService
                );

            // Act
            var result = await controller.UpdateApplicationsContactPersonInformation(request);

            var with = eServicesService.ChangeSubmittersContactAsyncCalledWithIds;
            var withDto = eServicesService.ChangeSubmittersContactAsyncCalledWithDto;

            var contactWith = withDto.ContactInformationData.First();
            var contactRequest = request.ContactPerson.ApplicationContactInformation.First();

            // Assert
            Assert.Equal(withDto.ContactInformationData.Count(), request.ContactPerson.ApplicationContactInformation.Count());
            Assert.Equal(contactWith.TypeId, contactRequest.TypeId);
            Assert.Equal(contactWith.Value, contactRequest.Value);
            Assert.Equal(2, with.Count());
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Sign_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            var eServicesService = ServiceFactory.CreateEServicesService();
            var controller = GetController(
               eServicesService: eServicesService
               );

            // Act
            var result = await controller.SignApplicationResource(id);

            var with = eServicesService.SignApplicationResourceCalledWith;

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(with, id);
        }

        [Fact]
        public async Task GetDocumentTemplates_Succeeds()
        {
            // Assign
            var dto = new EServiceDocumentTemplatesRequest
            {
                ResourceTypeId = Guid.NewGuid(),
                EducationalInstitutionId = 1
            };

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.DocumentTemplates = new TestAsyncEnumerable<DocumentTemplate>(new List<DocumentTemplate>
            {
                new DocumentTemplate
                {
                    Id = 1,
                    Code = DocumentType.ExploitationRules,
                    Title = DocumentType.ExploitationRules
                }
            });

            var controller = GetController(eServicesService: eServicesService);

            // Act
            var result = await controller.GetDocumentTemplates(dto);

            // Assert
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task GetDocumentTemplateById_Succeeds()
        {
            // Assign
            const int id = 1;

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.DocumentTemplateHtml = "content";

            var controller = GetController(eServicesService: eServicesService);

            // Act
            var result = await controller.GetDocumentTemplateById(id);

            // Assert
            Assert.Equal(id, eServicesService.GetDocumentTemplateAsyncCalledWith);
        }

        [Fact]
        public async Task DownloadDocumentTemplateById_Succeeds()
        {
            // Assign
            const int id = 1;

            var eServicesService = ServiceFactory.CreateEServicesService();
            eServicesService.DocumentTemplatePdf = new FileDto
            {
                FileName = "fileName.pdf",
                ContentType = MediaTypeNames.Application.Pdf,
                Content = Encoding.UTF8.GetBytes("content")
            };

            var controller = GetController(eServicesService: eServicesService);

            // Act
            var result = await controller.DownloadDocumentTemplateById(id);

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(id, eServicesService.GetDocumentTemplatePdfAsyncCalledWith);
        }

        private EServicesController GetController(
                IViisService viisService = null,
                IEServicesService eServicesService = null,
                IVraaUser vraaUser = null,
                IClassifierService classifierService = null,
                IGdprAuditService gdprAuditService = null
                )
        {
            return new EServicesController(
                viisService: viisService ?? ServiceFactory.CreateViisService(),
                eServicesService: eServicesService ?? ServiceFactory.CreateEServicesService(),
                vraaUser: vraaUser ?? ServiceFactory.CreateVraaUser(),
                classifierService: classifierService ?? ServiceFactory.CreateClassifierService(),
                gdprAuditService: gdprAuditService ?? ServiceFactory.CreateGdprAuditService()
                );
        }

        private Domain.Entities.Application CreateApplication(
            AppDbContext db,
            Guid id,
            Guid applicationStatusId,
            Guid personTechnicalId,
            Guid resourceSubTypeId,
            int eduInstId = 1)
        {
            var submitterTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var resourceTargetPersonEducationalStatusId = Guid.NewGuid();

            db.Classifiers.AddRange(
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

            db.SaveChanges();

            var applications = new Domain.Entities.Application(applicationStatusId)
            {
                Id = id,
                ApplicationNumber = Guid.NewGuid().ToString().Substring(0, 10),
                ApplicationDate = DateTime.UtcNow,
                EducationalInstitutionId = eduInstId,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ContactPersonId = personTechnicalId,
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
