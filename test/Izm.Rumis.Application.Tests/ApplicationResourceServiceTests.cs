using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Helpers;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class ApplicationResourceServiceTests
    {
        [Theory]
        [InlineData(UserProfileType.Country, 3)]
        [InlineData(UserProfileType.Supervisor, 2)]
        [InlineData(UserProfileType.EducationalInstitution, 1)]
        public void Get_ReturnsData(UserProfileType currentUserProfileType, int resultCount)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId1 = 1;
            const int educationalInstitutionId2 = 2;
            const int educationalInstitutionId3 = 3;
            const int supervisorId1 = 1;
            const int supervisorId2 = 2;

            var pnaStatusId = Guid.NewGuid();
            var statusClassifierId = Guid.NewGuid();

            var applicationId1 = Guid.NewGuid();
            var applicationId2 = Guid.NewGuid();
            var applicationId3 = Guid.NewGuid();

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = statusClassifierId,
                    Code = "c",
                    Value = "v",
                    Type = ClassifierTypes.EducationalInstitutionStatus
                },
                new Classifier
                {
                    Id = pnaStatusId,
                    Type = ClassifierTypes.PnaStatus,
                    Code = PnaStatus.Issued,
                    Value = string.Empty
                });

            db.SaveChanges();

            db.Supervisors.Add(new Supervisor
            {
                Id = supervisorId1,
                Code = "c",
                Name = "n"
            });

            db.EducationalInstitutions.AddRange(
                new EducationalInstitution
                {
                    Id = educationalInstitutionId1,
                    Code = "c",
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = supervisorId1
                },
                new EducationalInstitution
                {
                    Id = educationalInstitutionId2,
                    Code = "c",
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = supervisorId1
                },
                new EducationalInstitution
                {
                    Id = educationalInstitutionId3,
                    Code = "c",
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = supervisorId2
                });

            db.SaveChanges();

            CreateApplication(db, applicationId1, ApplicationStatus.Submitted, educationalInstitutionId1);
            CreateApplication(db, applicationId2, ApplicationStatus.Submitted, educationalInstitutionId2);
            CreateApplication(db, applicationId3, ApplicationStatus.Submitted, educationalInstitutionId3);

            var list = new List<ApplicationResource>
            {
                new ApplicationResource
                {
                    PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                    ApplicationId = applicationId1
                },
                new ApplicationResource
                {
                    PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                    ApplicationId = applicationId2
                },
                new ApplicationResource
                {
                    PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                    ApplicationId = applicationId3
                }
            };

            foreach (var item in list)
                item.SetPnaStatus(pnaStatusId);

            db.ApplicationResources.AddRange(list);

            db.SaveChanges();

            var currentUserProfile = new CurrentUserProfileServiceFake();
            currentUserProfile.Type = currentUserProfileType;
            currentUserProfile.EducationalInstitutionId = educationalInstitutionId1;
            currentUserProfile.SupervisorId = supervisorId1;

            // Act
            var data = GetService(db, currentUserProfile: currentUserProfile).Get().List();

            // Assert
            Assert.Equal(resultCount, data.Count());
        }

        [Fact]
        public async Task ChangeStatusToLostAsync_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.ChangeStatusToLostAsync(Guid.NewGuid(), new ApplicationResourceChangeStatusDto()));
        }

        [Fact]
        public async Task ChangeStatusToLostAsync_IncorrectPNAStatus()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var draftStatusId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = draftStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Preparing,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = Guid.NewGuid().ToString(),
            };

            resource.SetPnaStatus(draftStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangeStatusToLostAsync(id, new ApplicationResourceChangeStatusDto()));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectPNAStatus, result.Message);
        }

        [Fact]
        public async Task ChangeStatusToLostAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationResourceAttachmentId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var issuedStatusId = Guid.NewGuid();
            var lostStatusId = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            var dto = new ApplicationResourceChangeStatusDto
            {
                FileToDeleteIds = new[] { fileId },
                Files = new[] { FakeFileDto("a.docx") },
                Notes = "Notes"
            };

            using var db = ServiceFactory.ConnectDb();

            var fileService = ServiceFactory.CreateFileService();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = issuedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Issued,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = lostStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Lost,
                Value = string.Empty
            });

            await db.Files.AddAsync(new File
            {
                Id = fileId,
                Name = string.Empty,
                SourceType = FileSourceType.S3,
                Length = 0
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = Guid.NewGuid().ToString(),
            };

            resource.SetPnaStatus(issuedStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.ApplicationResourceAttachments.AddAsync(new ApplicationResourceAttachment
            {
                Id = applicationResourceAttachmentId,
                ApplicationResourceId = id,
                DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                FileId = fileId
            });

            await db.SaveChangesAsync();

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationDuplicateService = ServiceFactory.CreateApplicationDuplicateService();

            var service = GetService(
                db: db,
                fileService: fileService,
                authorizationService: authorizationService,
                applicationDuplicateService: applicationDuplicateService
                );

            // Act
            await service.ChangeStatusToLostAsync(id, dto);

            var applicationResource = db.ApplicationResources.First();

            var applicationResourceAttachment = db.ApplicationResourceAttachments;

            var file = fileService.File;

            // Assert
            Assert.Equal(lostStatusId, applicationResource.PNAStatusId);
            Assert.Equal(dto.Notes, applicationResource.Notes);
            Assert.NotNull(applicationResourceAttachment);
            Assert.Single(applicationResourceAttachment);
            Assert.NotEqual(fileId, applicationResourceAttachment.First().FileId);
            Assert.Equal(dto.Files.First().FileName, file.FileName);
            Assert.Equal(dto.Files.First().Content, file.Content);
            Assert.Equal(dto.Files.First().ContentType, file.ContentType);
            Assert.NotNull(authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.NotNull(applicationDuplicateService.CheckApplicationsDuplicatesAsyncCalledWith);
        }

        [Fact]
        public async Task ChangeStatusToPreparedAsync_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.ChangeStatusToPreparedAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task ChangeStatusToPreparedAsync_IncorrectPNAStatus()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var preparedStatusId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = preparedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Prepared,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat()
            };

            resource.SetPnaStatus(preparedStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangeStatusToPreparedAsync(id));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectPNAStatus, result.Message);
        }

        [Fact]
        public async Task ChangeStatusToPreparedAsync_IncorrectResourceStatus()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var draftStatusId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);

            CreateResource(db, assignedResourceId, ResourceStatus.Lost);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = draftStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Preparing,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(draftStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangeStatusToPreparedAsync(id));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectResourceStatus, result.Message);
        }

        [Fact]
        public async Task ChangeStatusToPreparedAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var draftStatusId = Guid.NewGuid();
            var preparedStatusId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();
            var reservedResourceStatusId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);

            CreateResource(db, assignedResourceId, ResourceStatus.Reserved);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = draftStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Preparing,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = preparedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Prepared,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(draftStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationDuplicateService = ServiceFactory.CreateApplicationDuplicateService();

            var service = GetService(
                db: db,
                authorizationService: authorizationService, 
                applicationDuplicateService: applicationDuplicateService
                );

            // Act
            await service.ChangeStatusToPreparedAsync(id);

            var applicationResource = db.ApplicationResources.First();

            // Assert
            Assert.Equal(preparedStatusId, applicationResource.PNAStatusId);
            Assert.NotNull(authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.NotNull(applicationDuplicateService.CheckApplicationsDuplicatesAsyncCalledWith);
        }

        [Fact]
        public async Task ChangeStatusToStolenAsync_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.ChangeStatusToStolenAsync(Guid.NewGuid(), new ApplicationResourceChangeStatusDto()));
        }

        [Fact]
        public async Task ChangeStatusToStolenAsync_IncorrectPNAStatus()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var draftStatusId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = draftStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Preparing,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = Guid.NewGuid().ToString()
            };

            resource.SetPnaStatus(draftStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangeStatusToStolenAsync(id, new ApplicationResourceChangeStatusDto()));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectPNAStatus, result.Message);
        }

        [Fact]
        public async Task ChangeStatusToStolenAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationResourceAttachmentId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var issuedStatusId = Guid.NewGuid();
            var stolenStatusId = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            var dto = new ApplicationResourceChangeStatusDto
            {
                FileToDeleteIds = new[] { fileId },
                Files = new[] { FakeFileDto("a.docx") },
                Notes = "Notes"
            };

            using var db = ServiceFactory.ConnectDb();

            var fileService = ServiceFactory.CreateFileService();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = issuedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Issued,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = stolenStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Stolen,
                Value = string.Empty
            });

            await db.Files.AddAsync(new File
            {
                Id = fileId,
                Name = string.Empty,
                SourceType = FileSourceType.Database,
                Length = 0
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = Guid.NewGuid().ToString()
            };

            resource.SetPnaStatus(issuedStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.ApplicationResourceAttachments.AddAsync(new ApplicationResourceAttachment
            {
                Id = applicationResourceAttachmentId,
                ApplicationResourceId = id,
                DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                FileId = fileId
            });

            await db.SaveChangesAsync();

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationDuplicateService = ServiceFactory.CreateApplicationDuplicateService();

            var service = GetService(
                db: db,
                fileService: fileService,
                authorizationService: authorizationService,
                applicationDuplicateService: applicationDuplicateService
                );

            // Act
            await service.ChangeStatusToStolenAsync(id, dto);

            var applicationResource = db.ApplicationResources.First();

            var applicationResourceAttachment = db.ApplicationResourceAttachments;

            var file = fileService.File;

            // Assert
            Assert.Equal(stolenStatusId, applicationResource.PNAStatusId);
            Assert.Equal(dto.Notes, applicationResource.Notes);
            Assert.NotNull(applicationResourceAttachment);
            Assert.Single(applicationResourceAttachment);
            Assert.NotEqual(fileId, applicationResourceAttachment.First().FileId);
            Assert.Equal(dto.Files.First().FileName, file.FileName);
            Assert.Equal(dto.Files.First().Content, file.Content);
            Assert.Equal(dto.Files.First().ContentType, file.ContentType);
            Assert.NotNull(authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.NotNull(applicationDuplicateService.CheckApplicationsDuplicatesAsyncCalledWith);
        }

        [Fact]
        public async Task CreateWithDraftStatusAsync_IncorrectApplicationStatus()
        {
            // Assign
            var applicationId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Deleted);

            await db.SaveChangesAsync();

            var item = new ApplicationResourceCreateDto
            {
                ApplicationId = applicationId,
                AssignedResourceId = Guid.NewGuid(),
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateWithDraftStatusAsync(item));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectApplicationStatus, result.Message);
        }

        [Fact]
        public async Task CreateWithDraftStatusAsync_IncorrectResourceStatus()
        {
            // Assign
            var applicationId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Postponed);
            CreateResource(db, assignedResourceId, ResourceStatus.UnderRepair);

            await db.SaveChangesAsync();

            var item = new ApplicationResourceCreateDto
            {
                ApplicationId = applicationId,
                AssignedResourceId = assignedResourceId,
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateWithDraftStatusAsync(item));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectResourceStatus, result.Message);
        }

        [Fact]
        public async Task CreateWithDraftStatusAsync_ApplicationAlereadyExists()
        {
            // Assign
            var draftStatusId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.Available);


            await db.Classifiers.AddAsync(new Classifier
            {
                Id = draftStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Preparing,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(draftStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var item = new ApplicationResourceCreateDto
            {
                ApplicationId = applicationId,
                AssignedResourceId = assignedResourceId,
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateWithDraftStatusAsync(item));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.ApplicationAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateWithDraftStatusAsync_Succeeds()
        {
            // Assign
            var applicationId = Guid.NewGuid();
            var draftStatusId = Guid.NewGuid();
            var confirmedApplicationStatusId = Guid.NewGuid();
            var reservedResourceStatusId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.Available);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = draftStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Preparing,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = confirmedApplicationStatusId,
                Type = ClassifierTypes.ApplicationStatus,
                Code = ApplicationStatus.Confirmed,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = reservedResourceStatusId,
                Type = ClassifierTypes.ResourceStatus,
                Code = ResourceStatus.Reserved,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var item = new ApplicationResourceCreateDto
            {
                ApplicationId = applicationId,
                AssignedResourceId = assignedResourceId,
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            var validator = ServiceFactory.CreateApplicationResourceValidator();
            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationDuplicateService = ServiceFactory.CreateApplicationDuplicateService();

            var service = GetService(
                db: db,
                validator: validator,
                authorizationService: authorizationService,
                applicationDuplicateService: applicationDuplicateService
                );

            // Act
            var result = await service.CreateWithDraftStatusAsync(item);

            // Assert
            Assert.Single(db.ApplicationResources.ToArray());
            var applicationResource = db.ApplicationResources.First();
            Assert.Equal(NumberingPatternHelper.ApplicationResourcesNumberFormat(), applicationResource.PNANumber);
            Assert.Equal(draftStatusId, applicationResource.PNAStatusId);
            Assert.Equal(confirmedApplicationStatusId, applicationResource.Application.ApplicationStatusId);
            Assert.Equal(reservedResourceStatusId, applicationResource.AssignedResource.ResourceStatusId);
            Assert.Equal(validator.ValidateAsyncCalledWith, item);
            Assert.NotNull(authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.NotNull(applicationDuplicateService.CheckApplicationsDuplicatesAsyncCalledWith);
        }

        [Fact]
        public async Task CreateWithDraftStatusAsync_SucceedsConfirmed()
        {
            // Assign
            var applicationId = Guid.NewGuid();
            var draftStatusId = Guid.NewGuid();
            var reservedResourceStatusId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Confirmed);
            CreateResource(db, assignedResourceId, ResourceStatus.Available);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = draftStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Preparing,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = reservedResourceStatusId,
                Type = ClassifierTypes.ResourceStatus,
                Code = ResourceStatus.Reserved,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var item = new ApplicationResourceCreateDto
            {
                ApplicationId = applicationId,
                AssignedResourceId = assignedResourceId,
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            var validator = ServiceFactory.CreateApplicationResourceValidator();

            var service = GetService(db, validator: validator);

            // Act
            var result = await service.CreateWithDraftStatusAsync(item);

            // Assert
            Assert.Single(db.ApplicationResources.ToArray());
            var applicationResource = db.ApplicationResources.First();
            Assert.Equal(NumberingPatternHelper.ApplicationResourcesNumberFormat(), applicationResource.PNANumber);
            Assert.Equal(draftStatusId, applicationResource.PNAStatusId);
            Assert.Equal(ApplicationStatus.Confirmed, applicationResource.Application.ApplicationStatus.Code);
            Assert.Equal(reservedResourceStatusId, applicationResource.AssignedResource.ResourceStatusId);
            Assert.Equal(validator.ValidateAsyncCalledWith, item);
        }

        [Fact]
        public async Task CreateWithPreparedStatusAsync_IncorrectApplicationStatus()
        {
            // Assign
            var applicationId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Deleted);

            await db.SaveChangesAsync();

            var item = new ApplicationResourceCreateDto
            {
                ApplicationId = applicationId,
                AssignedResourceId = Guid.NewGuid(),
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateWithPreparedStatusAsync(item));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectApplicationStatus, result.Message);
        }

        [Fact]
        public async Task CreateWithPreparedStatusAsync_IncorrectResourceStatus()
        {
            // Assign
            var applicationId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Postponed);
            CreateResource(db, assignedResourceId, ResourceStatus.UnderRepair);

            await db.SaveChangesAsync();

            var item = new ApplicationResourceCreateDto
            {
                ApplicationId = applicationId,
                AssignedResourceId = assignedResourceId,
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateWithPreparedStatusAsync(item));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectResourceStatus, result.Message);
        }

        [Fact]
        public async Task CreateWithPreparedStatusAsync_ApplicationAlereadyExists()
        {
            // Assign
            var draftStatusId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.Available);


            await db.Classifiers.AddAsync(new Classifier
            {
                Id = draftStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Preparing,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(draftStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var item = new ApplicationResourceCreateDto
            {
                ApplicationId = applicationId,
                AssignedResourceId = assignedResourceId,
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateWithPreparedStatusAsync(item));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.ApplicationAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateWithPreparedStatusAsync_Succeeds()
        {
            // Assign
            var applicationId = Guid.NewGuid();
            var preparedStatusId = Guid.NewGuid();
            var confirmedApplicationStatusId = Guid.NewGuid();
            var reservedResourceStatusId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.Available);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = preparedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Prepared,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = confirmedApplicationStatusId,
                Type = ClassifierTypes.ApplicationStatus,
                Code = ApplicationStatus.Confirmed,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = reservedResourceStatusId,
                Type = ClassifierTypes.ResourceStatus,
                Code = ResourceStatus.Reserved,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var item = new ApplicationResourceCreateDto
            {
                ApplicationId = applicationId,
                AssignedResourceId = assignedResourceId,
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            var validator = ServiceFactory.CreateApplicationResourceValidator();
            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationDuplicateService = ServiceFactory.CreateApplicationDuplicateService();

            var service = GetService(
                db: db,
                validator: validator,
                authorizationService: authorizationService, 
                applicationDuplicateService: applicationDuplicateService
                );

            // Act
            var result = await service.CreateWithPreparedStatusAsync(item);

            // Assert
            Assert.Single(db.ApplicationResources.ToArray());
            var applicationResource = db.ApplicationResources.First();
            Assert.Equal(NumberingPatternHelper.ApplicationResourcesNumberFormat(), applicationResource.PNANumber);
            Assert.Equal(preparedStatusId, applicationResource.PNAStatusId);
            Assert.Equal(confirmedApplicationStatusId, applicationResource.Application.ApplicationStatusId);
            Assert.Equal(reservedResourceStatusId, applicationResource.AssignedResource.ResourceStatusId);
            Assert.Equal(validator.ValidateAsyncCalledWith, item);
            Assert.NotNull(authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.NotNull(applicationDuplicateService.CheckApplicationsDuplicatesAsyncCalledWith);
        }


        [Fact]
        public async Task CreateWithPreparedStatusAsync_SucceedsConfirmed()
        {
            // Assign
            var applicationId = Guid.NewGuid();
            var preparedStatusId = Guid.NewGuid();
            var reservedResourceStatusId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Confirmed);
            CreateResource(db, assignedResourceId, ResourceStatus.Available);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = preparedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Prepared,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = reservedResourceStatusId,
                Type = ClassifierTypes.ResourceStatus,
                Code = ResourceStatus.Reserved,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var item = new ApplicationResourceCreateDto
            {
                ApplicationId = applicationId,
                AssignedResourceId = assignedResourceId,
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = string.Empty
            };

            var validator = ServiceFactory.CreateApplicationResourceValidator();

            var service = GetService(db, validator: validator);

            // Act
            var result = await service.CreateWithPreparedStatusAsync(item);

            // Assert
            Assert.Single(db.ApplicationResources.ToArray());
            var applicationResource = db.ApplicationResources.First();
            Assert.Equal(NumberingPatternHelper.ApplicationResourcesNumberFormat(), applicationResource.PNANumber);
            Assert.Equal(preparedStatusId, applicationResource.PNAStatusId);
            Assert.Equal(ApplicationStatus.Confirmed, applicationResource.Application.ApplicationStatus.Code);
            Assert.Equal(reservedResourceStatusId, applicationResource.AssignedResource.ResourceStatusId);
            Assert.Equal(validator.ValidateAsyncCalledWith, item);
        }

        [Fact]
        public async Task SetReturnDeadlineAsync_EntityNotFound()
        {
            // Assing
            var dto = new ApplicationResourceReturnDeadlineDto()
            {
                ApplicationResourceIds = new List<Guid> { Guid.NewGuid() }
            };

            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.SetReturnDeadlineAsync(dto));
        }

        [Fact]
        public async Task SetReturnDeadlineAsync_IncorrectPNAStatus()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var returnedStatusId = Guid.NewGuid();

            var dto = new ApplicationResourceReturnDeadlineDto()
            {
                ApplicationResourceIds = new List<Guid> { id }
            };

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = returnedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Returned,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
            };

            resource.SetPnaStatus(returnedStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SetReturnDeadlineAsync(dto));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectPNAStatus, result.Message);
        }

        [Fact]
        public async Task SetReturnDeadlineAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var issuedStatusId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            var dto = new ApplicationResourceReturnDeadlineDto()
            {
                ApplicationResourceIds = new List<Guid> { id },
                AssignedResourceReturnDate = DateTime.UtcNow
            };

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.InUse);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = issuedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Issued,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var entity = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId,
            };

            entity.SetApplicationResourceReturnDeadline(new DateTime(2020, 1, 1));
            entity.SetPnaStatus(issuedStatusId);

            await db.ApplicationResources.AddAsync(entity);

            await db.SaveChangesAsync();

            var service = GetService(
                db: db);

            // Act
            await service.SetReturnDeadlineAsync(dto);

            var applicationResource = db.ApplicationResources.First();

            // Assert
            Assert.Equal(dto.AssignedResourceReturnDate, applicationResource.AssignedResourceReturnDate);
        }

        [Fact]
        public async Task ReturnAsync_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.ReturnAsync(Guid.NewGuid(), new ApplicationResourceReturnEditDto()));
        }

        [Fact]
        public async Task ReturnAsync_IncorrectPNAStatus()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var draftStatusId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = draftStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Preparing,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat()
            };

            resource.SetPnaStatus(draftStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReturnAsync(id, new ApplicationResourceReturnEditDto()));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectPNAStatus, result.Message);
        }

        [Fact]
        public async Task ReturnAsync_IncorrectResourceStatus()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var issuedStatusId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.UnderRepair);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = issuedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Issued,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(issuedStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReturnAsync(id, new ApplicationResourceReturnEditDto()));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectResourceStatus, result.Message);
        }

        [Fact]
        public async Task ReturnAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var issuedStatusId = Guid.NewGuid();
            var returnedStatusId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();
            var inUseResourceStatusId = Guid.NewGuid();
            var availableResourceStatusId = Guid.NewGuid();
            var returnedWorkingOrderResourceReturnStatusId = Guid.NewGuid();

            var dto = new ApplicationResourceReturnEditDto
            {
                ResourceStatusId = availableResourceStatusId,
                ReturnResourceStateId = returnedWorkingOrderResourceReturnStatusId,
                ReturnResourceDate = DateTime.UtcNow,
                Notes = "Notes"
            };

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.InUse);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = issuedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Issued,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = returnedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Returned,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = returnedWorkingOrderResourceReturnStatusId,
                Type = ClassifierTypes.ResourceReturnStatus,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = inUseResourceStatusId,
                Type = ClassifierTypes.ResourceStatus,
                Code = ResourceStatus.InUse,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = availableResourceStatusId,
                Type = ClassifierTypes.ResourceStatus,
                Code = ResourceStatus.Available,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(issuedStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationDuplicateService = ServiceFactory.CreateApplicationDuplicateService();

            var service = GetService(
                db: db,
                authorizationService: authorizationService,
                applicationDuplicateService: applicationDuplicateService
                );

            // Act
            await service.ReturnAsync(id, dto);

            var applicationResource = db.ApplicationResources.First();

            // Assert
            Assert.Equal(returnedStatusId, applicationResource.PNAStatusId);
            Assert.Equal(returnedWorkingOrderResourceReturnStatusId, applicationResource.ReturnResourceStateId);
            Assert.Equal(dto.ReturnResourceDate, applicationResource.ReturnResourceDate);
            Assert.Equal(dto.Notes, applicationResource.Notes);
            Assert.Equal(availableResourceStatusId, applicationResource.AssignedResource.ResourceStatusId);
            Assert.Equal(dto.Notes, applicationResource.AssignedResource.Notes);
            Assert.NotNull(authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.NotNull(applicationDuplicateService.CheckApplicationsDuplicatesAsyncCalledWith);
        }

        [Fact]
        public async Task SignAsync_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.SignAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task SignAsync_IncorrectPNAStatus()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var draftStatusId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = draftStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Preparing,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat()
            };

            resource.SetPnaStatus(draftStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SignAsync(id));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectPNAStatus, result.Message);
        }

        [Fact]
        public async Task SignAsync_IncorrectResourceStatus()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var preparedStatusId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.UnderRepair);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = preparedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Prepared,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(preparedStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SignAsync(id));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectResourceStatus, result.Message);
        }

        [Fact]
        public async Task SignAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var issuedStatusId = Guid.NewGuid();
            var preparedStatusId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();
            var inUseResourceStatusId = Guid.NewGuid();
            var resourceTypeId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.Reserved);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = issuedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Issued,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = preparedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Prepared,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = inUseResourceStatusId,
                Type = ClassifierTypes.ResourceStatus,
                Code = ResourceStatus.InUse,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Type = ClassifierTypes.DocumentType,
                Code = DocumentType.PNA,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceTypeId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.ResourceType
            });

            await db.SaveChangesAsync();

            await db.DocumentTemplates.AddAsync(new DocumentTemplate
            {
                Code = DocumentType.PNA,
                Title = string.Empty,
                FileId = Guid.NewGuid(),
                ResourceTypeId = resourceTypeId
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(preparedStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var documentTemplateService = ServiceFactory.CreateDocumentTemplateService();
            var applicationDuplicateService = ServiceFactory.CreateApplicationDuplicateService();

            documentTemplateService.DocumentTemplates = db.DocumentTemplates.AsQueryable();

            var service = GetService(
                db: db,
                authorizationService: authorizationService,
                documentTemplateService: documentTemplateService,
                applicationDuplicateService: applicationDuplicateService
                );

            await db.SaveChangesAsync();

            // Act
            await service.SignAsync(id);

            var applicationResource = db.ApplicationResources.First();

            var applicationResourceAttachment = db.ApplicationResourceAttachments;

            // Assert
            Assert.Equal(issuedStatusId, applicationResource.PNAStatusId);
            Assert.Equal(inUseResourceStatusId, applicationResource.AssignedResource.ResourceStatusId);
            Assert.NotNull(authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.NotNull(applicationResourceAttachment);
            Assert.NotNull(applicationDuplicateService.CheckApplicationsDuplicatesAsyncCalledWith);
        }

        [Fact]
        public async Task CancelAsync_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.CancelAsync(Guid.NewGuid(), null));
        }

        [Fact]
        public async Task CancelAsync_IncorrectPNAStatus()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var id = Guid.NewGuid();

            var lostStatusId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.Reserved);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = lostStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Lost,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(lostStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelAsync(id, null));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectPNAStatus, result.Message);
        }

        [Fact]
        public async Task CancelAsync_IncorrectModelData()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var id = Guid.NewGuid();
            var preparedStatusId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.Reserved);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = preparedStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Prepared,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(preparedStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            ApplicationResourceCancelDto item = new ApplicationResourceCancelDto
            {
                ReasonId = Guid.NewGuid(),
                Description = string.Empty,
                ChangeApplicationStatusToWithdrawn = false
            };

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelAsync(id, item));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectModelData, result.Message);
        }

        [Fact]
        public async Task CancelAsync_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var id = Guid.NewGuid();
            var draftStatusId = Guid.NewGuid();
            var submitterRefusedStatusId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.Reserved);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = draftStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Preparing,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = submitterRefusedStatusId,
                Type = ClassifierTypes.PnaCancelingReason,
                Code = PnaCancelingReason.SubmitterRefused,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(draftStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            ApplicationResourceCancelDto item = new ApplicationResourceCancelDto
            {
                ReasonId = submitterRefusedStatusId,
                Description = string.Empty,
                ChangeApplicationStatusToWithdrawn = false
            };

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationDuplicateService = ServiceFactory.CreateApplicationDuplicateService();

            var service = GetService(
                db: db,
                authorizationService: authorizationService,
                applicationDuplicateService: applicationDuplicateService
                );

            await db.SaveChangesAsync();

            // Act & Assert
            await service.CancelAsync(id, item);

            var applicationResource = db.ApplicationResources.First();

            // Assert
            Assert.Equal(draftStatusId, applicationResource.PNAStatusId);
            Assert.Equal(submitterRefusedStatusId, applicationResource.CancelingReasonId);
            Assert.Equal(item.Description, applicationResource.CancelingDescription);
            Assert.NotNull(authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.NotNull(applicationDuplicateService.CheckApplicationsDuplicatesAsyncCalledWith);
        }

        [Fact]
        public async Task CancelAsync_SuccedesWithdrawn()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var id = Guid.NewGuid();
            var draftStatusId = Guid.NewGuid();
            var submitterRefusedStatusId = Guid.NewGuid();
            var availableStatusId = Guid.NewGuid();
            var withdrawndStatusId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.Reserved);

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = draftStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Preparing,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = availableStatusId,
                Type = ClassifierTypes.ResourceStatus,
                Code = ResourceStatus.Available,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = withdrawndStatusId,
                Type = ClassifierTypes.ApplicationStatus,
                Code = ApplicationStatus.Withdrawn,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = submitterRefusedStatusId,
                Type = ClassifierTypes.PnaCancelingReason,
                Code = PnaCancelingReason.SubmitterRefused,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(draftStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            ApplicationResourceCancelDto item = new ApplicationResourceCancelDto
            {
                ReasonId = submitterRefusedStatusId,
                Description = string.Empty,
                ChangeApplicationStatusToWithdrawn = true
            };


            var service = GetService(db);

            // Act & Assert
            await service.CancelAsync(id, item);

            var applicationResource = db.ApplicationResources
                .Include(t => t.Application)
                .Include(t => t.AssignedResource)
                .First();

            // Assert
            Assert.Equal(draftStatusId, applicationResource.PNAStatusId);
            Assert.Equal(submitterRefusedStatusId, applicationResource.CancelingReasonId);
            Assert.Equal(item.Description, applicationResource.CancelingDescription);
            Assert.Equal(availableStatusId, applicationResource.AssignedResource.ResourceStatusId);
            Assert.Equal(withdrawndStatusId, applicationResource.Application.ApplicationStatusId);
        }

        [Fact]
        public async Task GetExploitationRulesAsync_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var applicationId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var applicationResourceAttachmentId = Guid.NewGuid();
            var personTechnicalId = Guid.NewGuid();
            const int documentTemplateId = 1;
            const int educationalInstitutionId = 1;
            const string template = "template";

            CreateApplication(db, applicationId, ApplicationStatus.Submitted, educationalInstitutionId);
            CreateResource(db, resourceId, ResourceStatus.Reserved);
            CreateApplicationResource(db, applicationResourceId, applicationId, resourceId);

            var fileService = ServiceFactory.CreateFileService();
            await CreateDocumentTemplateAsync(db, fileService, documentTemplateId, template);

            var authorizationService = ServiceFactory.CreateAuthorizationService();

            var documentTemplateService = ServiceFactory.CreateDocumentTemplateService();
            documentTemplateService.DocumentTemplates = db.DocumentTemplates.AsQueryable();

            var service = GetService(
                db: db,
                fileService: fileService,
                authorizationService: authorizationService,
                documentTemplateService: documentTemplateService);

            // Act
            var data = await service.GetExploitationRulesAsync(applicationResourceId);

            // Assert
            Assert.Equal(educationalInstitutionId, authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.Equal(template, data);
        }

        [Fact]
        public async Task GetExploitationRulesAsync_Throws_NotFound()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetExploitationRulesAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetExploitationRulesPdfAsync_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var applicationId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var applicationResourceAttachmentId = Guid.NewGuid();
            var personTechnicalId = Guid.NewGuid();
            const int documentTemplateId = 1;
            const int educationalInstitutionId = 1;
            const string template = "template";

            CreateApplication(db, applicationId, ApplicationStatus.Submitted, educationalInstitutionId);
            CreateResource(db, resourceId, ResourceStatus.Reserved);
            CreateApplicationResource(db, applicationResourceId, applicationId, resourceId);

            var fileService = ServiceFactory.CreateFileService();
            await CreateDocumentTemplateAsync(db, fileService, documentTemplateId, template);

            var authorizationService = ServiceFactory.CreateAuthorizationService();

            var documentTemplateService = ServiceFactory.CreateDocumentTemplateService();
            documentTemplateService.DocumentTemplates = db.DocumentTemplates.AsQueryable();

            var service = GetService(
                db: db,
                fileService: fileService,
                authorizationService: authorizationService,
                documentTemplateService: documentTemplateService);

            // Act
            var data = await service.GetExploitationRulesPdfAsync(applicationResourceId);

            // Assert
            Assert.Equal(educationalInstitutionId, authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.Equal(MediaTypeNames.Application.Pdf, data.ContentType);
            Assert.NotNull(data.Content);
            Assert.Equal($"{applicationResourceId}.pdf", data.FileName);
        }

        [Fact]
        public async Task GetExploitationRulesPdfAsync_Throws_NotFound()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetExploitationRulesPdfAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var preparedStatusId = Guid.NewGuid();
            var existingAssignedResourceId = Guid.NewGuid();
            var newAssignedResourceId = Guid.NewGuid();
            var educationalInstitutionContactPersonId1 = Guid.NewGuid();
            var educationalInstitutionContactPersonId2 = Guid.NewGuid();
            var educationalInstitutionContactPersonId3 = Guid.NewGuid();

            var dto = new ApplicationResourceUpdateDto
            {
                AssignedResourceId = newAssignedResourceId,
                AssignedResourceReturnDate = DateTime.UtcNow,
                EducationalInstitutionContactPersonIds = new List<Guid>
                {
                    educationalInstitutionContactPersonId1,
                    educationalInstitutionContactPersonId2
                },
                Notes = "Notes"
            };

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, existingAssignedResourceId, ResourceStatus.Reserved);
            CreateResource(db, newAssignedResourceId, ResourceStatus.Available);

            await db.Classifiers.AddRangeAsync(
                new Classifier
                {
                    Id = preparedStatusId,
                    Type = ClassifierTypes.PnaStatus,
                    Code = PnaStatus.Prepared,
                    Value = string.Empty
                });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = existingAssignedResourceId
            };

            resource.SetPnaStatus(preparedStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.ApplicationResourceContactPersons.AddRangeAsync(
                new ApplicationResourceContactPerson
                {
                    ApplicationResourceId = id,
                    EducationalInstitutionContactPersonId = educationalInstitutionContactPersonId2
                },
                new ApplicationResourceContactPerson
                {
                    ApplicationResourceId = id,
                    EducationalInstitutionContactPersonId = educationalInstitutionContactPersonId3
                });

            await db.SaveChangesAsync();

            var authorizationService = ServiceFactory.CreateAuthorizationService();

            var service = GetService(
                db: db,
                authorizationService: authorizationService);

            // Act
            await service.UpdateAsync(id, dto);

            var applicationResource = db.ApplicationResources.First();
            var applicationResourceConPersIds = applicationResource.ApplicationResourceContactPersons.Select(t => t.EducationalInstitutionContactPersonId);
            var existingAssignedResource = db.Resources.First(t => t.Id == existingAssignedResourceId);
            var reservedResourceStatus = db.Classifiers.First(t => t.Code == ResourceStatus.Reserved);
            var availableResourceStatus = db.Classifiers.First(t => t.Code == ResourceStatus.Available);

            // Assert
            Assert.Equal(dto.AssignedResourceReturnDate, applicationResource.AssignedResourceReturnDate);
            Assert.Equal(reservedResourceStatus.Id, applicationResource.AssignedResource.ResourceStatusId);
            Assert.Equal(dto.Notes, applicationResource.AssignedResource.Notes);
            Assert.Equal(availableResourceStatus.Id, existingAssignedResource.ResourceStatusId);
            Assert.Null(existingAssignedResource.Notes);
            Assert.Contains(educationalInstitutionContactPersonId1, applicationResourceConPersIds);
            Assert.Contains(educationalInstitutionContactPersonId2, applicationResourceConPersIds);
            Assert.DoesNotContain(educationalInstitutionContactPersonId3, applicationResourceConPersIds);
            Assert.NotNull(authorizationService.AuthorizeEducationalInstitutionCalledWith);
        }

        public static IEnumerable<object[]> GetUpdateAsyncIncorrectPNAStatuses()
        {
            var pnaStatusFields = typeof(PnaStatus).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.Name != "Preparing" && f.Name != "Prepared")
                .Select(f => f.GetValue(null).ToString())
                .ToList();

            foreach (var status in pnaStatusFields)
                yield return new object[] { status };
        }

        [Theory]
        [MemberData(nameof(GetUpdateAsyncIncorrectPNAStatuses))]
        public async Task UpdateAsync_Throws_IncorrectPNAStatus(string pnaStatusCode)
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var pnaStatusId = Guid.NewGuid();
            var assignedResourceId = Guid.NewGuid();

            var dto = new ApplicationResourceUpdateDto
            {
                AssignedResourceId = assignedResourceId,
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = "Notes"
            };

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, assignedResourceId, ResourceStatus.Reserved);

            await db.Classifiers.AddRangeAsync(
                new Classifier
                {
                    Id = pnaStatusId,
                    Type = ClassifierTypes.PnaStatus,
                    Code = pnaStatusCode,
                    Value = string.Empty
                });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = assignedResourceId
            };

            resource.SetPnaStatus(pnaStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(id, dto));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectPNAStatus, result.Message);
        }

        public static IEnumerable<object[]> GetUpdateAsyncIncorrectResourceStatuses()
        {
            var resourceStatusFields = typeof(ResourceStatus).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.Name != "Available")
                .Select(f => f.GetValue(null).ToString())
                .ToList();

            foreach (var status in resourceStatusFields)
                yield return new object[] { status };
        }

        [Theory]
        [MemberData(nameof(GetUpdateAsyncIncorrectResourceStatuses))]
        public async Task UpdateAsync_Throws_IncorrectResourceStatus(string resourceStatusCode)
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var preparedStatusId = Guid.NewGuid();
            var existingAssignedResourceId = Guid.NewGuid();
            var newAssignedResourceId = Guid.NewGuid();

            var dto = new ApplicationResourceUpdateDto
            {
                AssignedResourceId = newAssignedResourceId,
                AssignedResourceReturnDate = DateTime.UtcNow,
                Notes = "Notes"
            };

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, applicationId, ApplicationStatus.Submitted);
            CreateResource(db, existingAssignedResourceId, ResourceStatus.Reserved);
            CreateResource(db, newAssignedResourceId, resourceStatusCode);

            await db.Classifiers.AddRangeAsync(
                new Classifier
                {
                    Id = preparedStatusId,
                    Type = ClassifierTypes.PnaStatus,
                    Code = PnaStatus.Prepared,
                    Value = string.Empty
                });

            await db.SaveChangesAsync();

            var resource = new ApplicationResource
            {
                Id = id,
                ApplicationId = applicationId,
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                AssignedResourceId = existingAssignedResourceId
            };

            resource.SetPnaStatus(preparedStatusId);

            await db.ApplicationResources.AddAsync(resource);

            await db.SaveChangesAsync();

            var authorizationService = ServiceFactory.CreateAuthorizationService();

            var service = GetService(
                db: db,
                authorizationService: authorizationService);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(id, dto));

            //Assert
            Assert.Equal(ApplicationResourceService.Error.IncorrectResourceStatus, result.Message);
        }

        [Fact]
        public async Task GetPnaAsync_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var applicationId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var applicationResourceAttachmentId = Guid.NewGuid();
            var personTechnicalId = Guid.NewGuid();
            const int documentTemplateId = 1;
            const int educationalInstitutionId = 1;

            var placeholders = new[] {
                Placeholder.ResourceAcquisitionsValue, Placeholder.DueDate, Placeholder.Recipient,
                Placeholder.IssuedDate, Placeholder.NotesIssued, Placeholder.ResourceSerialNumber,
                Placeholder.EducationalInstitution, Placeholder.ResourceName, Placeholder.User,
                Placeholder.ResourceInventoryNumber, Placeholder.ResourceModelIdentifier, Placeholder.PnaNumber
            };

            var template = string.Join("", placeholders.Select(t => "{{" + t + "}}"));

            CreateApplication(db, applicationId, ApplicationStatus.Confirmed, educationalInstitutionId);
            CreateResource(db, resourceId, ResourceStatus.New, educationalInstitutionId);
            CreateApplicationResource(db, applicationResourceId, applicationId, resourceId);

            var fileService = ServiceFactory.CreateFileService();
            await CreateDocumentTemplateAsync(db, fileService, documentTemplateId, template, DocumentType.PNA);

            var attachment = CreateApplicationResourceAttachment(
                db,
                applicationResourceAttachmentId,
                applicationResourceId,
                documentTemplateId);

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var documentTemplateService = ServiceFactory.CreateDocumentTemplateService();

            var service = GetService(
                db: db,
                fileService: fileService,
                authorizationService: authorizationService,
                documentTemplateService: documentTemplateService
                );

            // Act
            var data = await service.GetPnaAsync(applicationResourceId);

            // Assert
            Assert.NotNull(attachment.FileId);
            Assert.Equal(educationalInstitutionId, authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.True(!(data.Contains('{') || data.Contains('}')));
            Assert.Null(documentTemplateService.GetByEducationalInstitutionCalledWith);
        }

        [Fact]
        public async Task GetPnaAsync_Succeeds_NoPna()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var applicationId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var applicationResourceAttachmentId = Guid.NewGuid();
            var personTechnicalId = Guid.NewGuid();
            const int documentTemplateId = 1;
            const int educationalInstitutionId = 1;

            var placeholders = new[] {
                Placeholder.ResourceAcquisitionsValue, Placeholder.DueDate, Placeholder.Recipient,
                Placeholder.IssuedDate, Placeholder.NotesIssued, Placeholder.ResourceSerialNumber,
                Placeholder.EducationalInstitution, Placeholder.ResourceName, Placeholder.User,
                Placeholder.ResourceInventoryNumber, Placeholder.ResourceModelIdentifier, Placeholder.PnaNumber
            };

            var template = string.Join("", placeholders.Select(t => "{{" + t + "}}"));

            CreateApplication(db, applicationId, ApplicationStatus.Confirmed, educationalInstitutionId);
            CreateResource(db, resourceId, ResourceStatus.New, educationalInstitutionId);
            CreateApplicationResource(db, applicationResourceId, applicationId, resourceId);

            var fileService = ServiceFactory.CreateFileService();
            await CreateDocumentTemplateAsync(db, fileService, documentTemplateId, template, DocumentType.PNA);

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var documentTemplateService = ServiceFactory.CreateDocumentTemplateService();

            documentTemplateService.DocumentTemplates = db.DocumentTemplates.AsQueryable();

            var service = GetService(
                db: db,
                fileService: fileService,
                authorizationService: authorizationService,
                documentTemplateService: documentTemplateService
                );

            // Act
            var data = await service.GetPnaAsync(applicationResourceId);

            // Assert
            Assert.Equal(educationalInstitutionId, authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.True(!(data.Contains('{') || data.Contains('}')));
            Assert.Equal(educationalInstitutionId, documentTemplateService.GetByEducationalInstitutionCalledWith);
        }

        [Fact]
        public async Task GetPnaAsync_Throws_NotFound()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetPnaAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetPnaPdfAsync_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var applicationId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var applicationResourceAttachmentId = Guid.NewGuid();
            var personTechnicalId = Guid.NewGuid();
            const int documentTemplateId = 1;
            const int educationalInstitutionId = 1;
            const string template = "{{SubmitterPerson}}";
            const string pnaNumber = "pnaNumber";

            CreateApplication(db, applicationId, ApplicationStatus.Confirmed, educationalInstitutionId);
            CreateResource(db, resourceId, ResourceStatus.New, educationalInstitutionId);
            CreateApplicationResource(db, applicationResourceId, applicationId, resourceId, pnaNumber);

            var fileService = ServiceFactory.CreateFileService();
            await CreateDocumentTemplateAsync(db, fileService, documentTemplateId, template, DocumentType.PNA);

            var attachment = CreateApplicationResourceAttachment(
                db,
                applicationResourceAttachmentId,
                applicationResourceId,
                documentTemplateId);

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var documentTemplateService = ServiceFactory.CreateDocumentTemplateService();

            var service = GetService(
                db: db,
                fileService: fileService,
                authorizationService: authorizationService,
                documentTemplateService: documentTemplateService
                );

            // Act
            var data = await service.GetPnaPdfAsync(applicationResourceId);

            // Assert
            Assert.NotNull(attachment.FileId);
            Assert.Equal(educationalInstitutionId, authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.Null(documentTemplateService.GetByEducationalInstitutionCalledWith);
            Assert.Equal(MediaTypeNames.Application.Pdf, data.ContentType);
            Assert.NotNull(data.Content);
            Assert.Equal($"{pnaNumber}.pdf", data.FileName);
        }

        [Fact]
        public async Task GetPnaPdfAsync_Succeeds_NoPna()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var applicationId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var applicationResourceAttachmentId = Guid.NewGuid();
            var personTechnicalId = Guid.NewGuid();
            const int documentTemplateId = 1;
            const int educationalInstitutionId = 1;
            const string template = "{{SubmitterPerson}}";
            const string pnaNumber = "pnaNumber";

            CreateApplication(db, applicationId, ApplicationStatus.Confirmed, educationalInstitutionId);
            CreateResource(db, resourceId, ResourceStatus.New, educationalInstitutionId);
            CreateApplicationResource(db, applicationResourceId, applicationId, resourceId, pnaNumber);

            var fileService = ServiceFactory.CreateFileService();
            await CreateDocumentTemplateAsync(db, fileService, documentTemplateId, template, DocumentType.PNA);
            
            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var documentTemplateService = ServiceFactory.CreateDocumentTemplateService();

            documentTemplateService.DocumentTemplates = db.DocumentTemplates.AsQueryable();

            var service = GetService(
                db: db,
                fileService: fileService,
                authorizationService: authorizationService,
                documentTemplateService: documentTemplateService
                );

            // Act
            var data = await service.GetPnaPdfAsync(applicationResourceId);

            // Assert
            Assert.Equal(educationalInstitutionId, authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.Equal(educationalInstitutionId, documentTemplateService.GetByEducationalInstitutionCalledWith);
            Assert.Equal(MediaTypeNames.Application.Pdf, data.ContentType);
            Assert.NotNull(data.Content);
            Assert.Equal($"{pnaNumber}.pdf", data.FileName);
        }

        [Fact]
        public async Task GetPnaPdfAsync_Throws_NotFound()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetPnaPdfAsync(Guid.NewGuid()));
        }

        private ApplicationResourceService GetService(
            IAppDbContext db,
            ICurrentUserProfileService currentUserProfile = null,
            IApplicationResourceValidator validator = null,
            IFileService fileService = null,
            ISequenceService sequenceService = null,
            IAuthorizationService authorizationService = null,
            IDocumentTemplateService documentTemplateService = null,
            IApplicationDuplicateService applicationDuplicateService = null
            )
        {
            return new ApplicationResourceService(
                db,
                currentUserProfile: currentUserProfile ?? ServiceFactory.CreateCurrentUserProfileService(),
                validator: validator ?? ServiceFactory.CreateApplicationResourceValidator(),
                fileService: fileService ?? ServiceFactory.CreateFileService(),
                sequenceService: sequenceService ?? ServiceFactory.CreateSequenceService(),
                authorizationService: authorizationService ?? ServiceFactory.CreateAuthorizationService(),
                documentTemplateService: documentTemplateService ?? ServiceFactory.CreateDocumentTemplateService(),
                applicationDuplicateService: applicationDuplicateService ?? ServiceFactory.CreateApplicationDuplicateService()
                );
        }

        private void CreateApplication(DbContextFake db, Guid id, string statusChangeCode, int educationalInstitutionId = 1)
        {
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var personId = Guid.NewGuid();

            SeedEducationalInstitution(db, educationalInstitutionId);

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = personId,
                UserId = Guid.NewGuid()
            });

            db.Persons.Add(new Person
            {
                PersonTechnicalId = personId,
                FirstName = "FirstName",
                LastName = "LastName",
                PrivatePersonalIdentifier = "PrivatePersonalIdentifier"
            });

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = applicationStatusId,
                    Type = ClassifierTypes.ApplicationStatus,
                    Code = statusChangeCode,
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
                    Id = resourceSubTypeId,
                    Type = ClassifierTypes.ResourceSubType,
                    Code = string.Empty,
                    Value = string.Empty
                });


            db.SaveChanges();

            db.Applications.Add(new Domain.Entities.Application(applicationStatusId)
            {
                Id = id,
                ApplicationNumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                ApplicationDate = DateTime.UtcNow,
                EducationalInstitutionId = educationalInstitutionId,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonId = personId,
                SubmitterPersonId = personId
            });

            db.SaveChanges();
        }

        private void CreateResource(DbContextFake db, Guid id, string statusChangeCode, int educationalInstitutionId = 1)
        {
            var resourceLocationId = Guid.NewGuid();
            var targetGroupId = Guid.NewGuid();
            var usagePurposeTypeId = Guid.NewGuid();
            var acquisitionTypeId = Guid.NewGuid();
            var manufacturerId = Guid.NewGuid();
            var resourceStatusId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceModelId = Guid.NewGuid();

            SeedEducationalInstitution(db, educationalInstitutionId);

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
                    Id = resourceSubTypeId,
                    Type = ClassifierTypes.ResourceSubType,
                    Code = ResourceSubType.WindowsLaptop,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = resourceStatusId,
                    Type = ClassifierTypes.ResourceStatus,
                    Code = statusChangeCode,
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
                    Id = manufacturerId,
                    Type = ClassifierTypes.ResourceManufacturer,
                    Code = string.Empty,
                    Value = string.Empty
                });

            db.SaveChanges();

            db.Resources.Add(new Resource
            {
                Id = id,
                ModelNameId = resourceModelId,
                ResourceSubTypeId = resourceSubTypeId,
                ResourceNumber = NumberingPatternHelper.ResourceNumberFormat(),
                ResourceName = "ResourceName",
                InventoryNumber = "InventoryNumber",
                SerialNumber = "SerialNumber",
                AcquisitionTypeId = acquisitionTypeId,
                UsagePurposeTypeId = usagePurposeTypeId,
                TargetGroupId = targetGroupId,
                ResourceLocationId = resourceLocationId,
                ManufacturerId = manufacturerId,
                EducationalInstitutionId = educationalInstitutionId,
                ResourceStatusId = resourceStatusId
            });

            db.SaveChanges();
        }

        private async Task CreateDocumentTemplateAsync(DbContextFake db, IFileService fileService, int id, string template, string code = DocumentType.ExploitationRules)
        {
            var fileId = Guid.NewGuid();

            var resourceTypeId = Guid.NewGuid();

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceTypeId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.ResourceType
            });

            await db.SaveChangesAsync();

            await db.DocumentTemplates.AddAsync(new DocumentTemplate
            {
                Id = id,
                Code = code,
                Title = "Title",
                FileId = fileId,
                ResourceTypeId = resourceTypeId
            });

            await fileService.AddOrUpdateAsync(fileId, new FileDto
            {
                FileName = "template.html",
                ContentType = MediaTypeNames.Text.Html,
                Content = Encoding.UTF8.GetBytes(template)
            });

            await db.SaveChangesAsync();
        }

        private void CreateApplicationResource(DbContextFake db, Guid id, Guid applicationId, Guid resourceId, string pnaNumber = "PNANumber")
        {
            var pnaStatusId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = pnaStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Issued,
                Value = string.Empty
            });

            db.SaveChanges();

            var entity = new ApplicationResource
            {
                Id = id,
                PNANumber = pnaNumber,
                ApplicationId = applicationId,
                AssignedResourceId = resourceId
            };

            entity.SetApplicationResourceReturnDeadline(new DateTime(2020, 1, 1));

            entity.SetPnaStatus(pnaStatusId);

            db.ApplicationResources.Add(entity);

            db.SaveChanges();
        }

        private void SeedEducationalInstitution(DbContextFake db, int educationalInstitutionId = 1)
        {
            if (!db.EducationalInstitutions.Any(t => t.Id == educationalInstitutionId))
            {
                var educationalInstitutionStatusId = db.Classifiers
                    .Where(t => t.Type == ClassifierTypes.EducationalInstitutionStatus && t.Code == EducationalInstitutionStatus.Active)
                    .Select(t => t.Id)
                    .FirstOrDefault();

                if (educationalInstitutionStatusId == Guid.Empty)
                {
                    educationalInstitutionStatusId = Guid.NewGuid();
                    db.Classifiers.Add(new Classifier
                    {
                        Id = educationalInstitutionStatusId,
                        Type = ClassifierTypes.EducationalInstitutionStatus,
                        Code = EducationalInstitutionStatus.Active,
                        Value = string.Empty
                    });

                    db.SaveChanges();
                }

                db.EducationalInstitutions.Add(new EducationalInstitution
                {
                    Id = educationalInstitutionId,
                    Code = NumberingPatternHelper.DefaultInstitution,
                    Name = string.Empty,
                    StatusId = educationalInstitutionStatusId
                });

                db.SaveChanges();
            }
        }

        private byte[] FakeContent()
        {
            return Encoding.UTF8.GetBytes("test");
        }

        private FileDto FakeFileDto(string filename)
        {
            return new FileDto
            {
                FileName = filename,
                Content = FakeContent(),
            };
        }

        private ApplicationResourceAttachment CreateApplicationResourceAttachment(IAppDbContext db, Guid id, Guid applicationResourceId, int documentTemplateId)
        {
            var documentTypeId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = documentTypeId,
                Type = ClassifierTypes.DocumentType,
                Code = DocumentType.PNA,
                Value = string.Empty
            });

            db.SaveChanges();

            var entity = new ApplicationResourceAttachment
            {
                Id = id,
                ApplicationResourceId = applicationResourceId,
                DocumentTypeId = documentTypeId,
                DocumentDate = new DateOnly(2020, 1, 1),
                DocumentTemplateId = documentTemplateId
            };

            db.ApplicationResourceAttachments.Add(entity);

            db.SaveChanges();

            return entity;
        }
    }
}
