using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using Izm.Rumis.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public sealed class ApplicationsControllerTests
    {
        private ApplicationsController controller;
        private ApplicationServiceFake applicationServiceFake;
        private ClassifierServiceFake classifierServiceFake;
        private GdprAuditServiceFake gdprAuditServiceFake;

        public ApplicationsControllerTests()
        {
            applicationServiceFake = new ApplicationServiceFake();
            classifierServiceFake = new ClassifierServiceFake();
            gdprAuditServiceFake = new GdprAuditServiceFake();

            controller = new ApplicationsController(applicationServiceFake, classifierServiceFake, gdprAuditServiceFake);
        }

        [Fact]
        public async Task CheckApplicationSocialStatus_Succeeds()
        {
            // Assign
            const int educationalInstitutionId = 1;
            const int supervisorId = 1;

            var applicationStatusId = Guid.NewGuid();
            var educationalInstitutionStatusId = Guid.NewGuid();
            var contactPersonId = Guid.NewGuid();
            var id = Guid.NewGuid();
            var socialStatusId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var submitterPersonId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = educationalInstitutionStatusId,
                Type = ClassifierTypes.EducationalInstitutionStatus,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            await db.PersonTechnicals.AddAsync(new PersonTechnical
            {
                Id = contactPersonId
            });

            await db.PersonTechnicals.AddAsync(new PersonTechnical
            {
                Id = resourceTargetPersonId
            });

            await db.PersonTechnicals.AddAsync(new PersonTechnical
            {
                Id = submitterPersonId
            });

            await db.Persons.AddAsync(new Person
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                PrivatePersonalIdentifier = string.Empty,
                PersonTechnicalId = contactPersonId
            });

            await db.Persons.AddAsync(new Person
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                PrivatePersonalIdentifier = string.Empty,
                PersonTechnicalId = resourceTargetPersonId
            });

            await db.Persons.AddAsync(new Person
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                PrivatePersonalIdentifier = string.Empty,
                PersonTechnicalId = submitterPersonId
            });

            await db.Supervisors.AddAsync(new Supervisor
            {
                Id = supervisorId,
                Code = string.Empty,
                Name = string.Empty
            });

            await db.EducationalInstitutions.AddAsync(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                StatusId = educationalInstitutionStatusId,
                SupervisorId = supervisorId,
                Code = string.Empty,
                Name = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = applicationStatusId,
                Type = ClassifierTypes.ApplicationStatus,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceSubTypeId,
                Type = ClassifierTypes.ResourceSubType,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceTargetPersonTypeId,
                Type = ClassifierTypes.ResourceTargetPersonType,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = submitterTypeId,
                Type = ClassifierTypes.ApplicantRole,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = socialStatusId,
                Type = ClassifierTypes.SocialStatus,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            await db.Applications.AddAsync(new Domain.Entities.Application(applicationStatusId)
            {
                ApplicationDate = DateTime.UtcNow,
                ApplicationNumber = Guid.NewGuid().ToString().Substring(0, 10),
                ContactPersonId = contactPersonId,
                Id = id,
                ResourceSubTypeId = resourceSubTypeId,
                ResourceTargetPersonId = resourceTargetPersonId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                SubmitterPersonId = submitterPersonId,
                SubmitterTypeId = submitterTypeId
            });

            await db.ApplicationSocialStatuses.AddAsync(new ApplicationSocialStatus
            {
                ApplicationId = id,
                SocialStatusId = socialStatusId,
                Id = Guid.NewGuid()
            });

            await db.SaveChangesAsync();

            applicationServiceFake.Applications = db.Applications.AsQueryable();

            // Act
            var result = await controller.CheckApplicationSocialStatus(id);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            // Act
            var result = await controller.Create(new ApplicationCreateRequest());

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Get_NoData()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            applicationServiceFake.Applications = db.Applications.AsQueryable();

            // Act
            var result = await controller.Get();

            // Assert
            Assert.Empty(result.Value.Items);
        }

        [Fact]
        public async Task Get_ReturnsData_NoPaging_NoFilter()
        {
            // Assign
            const int educationalInstitutionId = 1;
            const int supervisorId = 1;

            var applicationStatusId = Guid.NewGuid();
            var contactPersonId = Guid.NewGuid();
            var resourceTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonEducationalStatusId = Guid.NewGuid();
            var resourceTargetPersonEducationalSubStatusId = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var resourceTargetPersonWorkStatusId = Guid.NewGuid();
            var submitterPersonId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
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

            await db.PersonTechnicals.AddRangeAsync(
                new PersonTechnical { Id = contactPersonId },
                new PersonTechnical { Id = resourceTargetPersonId },
                new PersonTechnical { Id = submitterPersonId });

            await db.Persons.AddRangeAsync(
                new Person
                {
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    PrivatePersonalIdentifier = string.Empty,
                    PersonTechnicalId = contactPersonId
                },
                new Person
                {
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    PrivatePersonalIdentifier = string.Empty,
                    PersonTechnicalId = resourceTargetPersonId
                },
                new Person
                {
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    PrivatePersonalIdentifier = string.Empty,
                    PersonTechnicalId = submitterPersonId
                });

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

            await db.Classifiers.AddRangeAsync(
                new Classifier
                {
                    Id = applicationStatusId,
                    Type = ClassifierTypes.ApplicationStatus,
                    Code = string.Empty,
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
                    Id = resourceTargetPersonEducationalStatusId,
                    Type = ClassifierTypes.ResourceTargetPersonEducationalStatus,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = resourceTargetPersonEducationalSubStatusId,
                    Type = ClassifierTypes.ResourceTargetPersonEducationalSubStatus,
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
                    Id = resourceTargetPersonWorkStatusId,
                    Type = ClassifierTypes.ResourceTargetPersonWorkStatus,
                    Code = string.Empty,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = submitterTypeId,
                    Type = ClassifierTypes.ApplicantRole,
                    Code = string.Empty,
                    Value = string.Empty
                });

            await db.SaveChangesAsync();

            await db.Applications.AddAsync(new Domain.Entities.Application(applicationStatusId)
            {
                ApplicationDate = DateTime.UtcNow,
                ApplicationNumber = Guid.NewGuid().ToString().Substring(0, 10),
                ContactPersonId = contactPersonId,
                CreatedById = Guid.NewGuid(),
                EducationalInstitutionId = educationalInstitutionId,
                Id = Guid.NewGuid(),
                ModifiedById = Guid.NewGuid(),
                ResourceSubTypeId = resourceSubTypeId,
                ResourceTargetPersonEducationalStatusId = resourceTargetPersonEducationalStatusId,
                ResourceTargetPersonEducationalSubStatusId = resourceTargetPersonEducationalSubStatusId,
                ResourceTargetPersonId = resourceTargetPersonId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonWorkStatusId = resourceTargetPersonWorkStatusId,
                SubmitterPersonId = submitterPersonId,
                SubmitterTypeId = submitterTypeId
            });

            await db.SaveChangesAsync();

            applicationServiceFake.Applications = db.Applications.AsQueryable();
            classifierServiceFake.Data = db.Classifiers.AsQueryable();

            // Act
            var result = await controller.Get();

            // Assert
            Assert.Single(result.Value.Items);
            Assert.Equal(applicationServiceFake.Applications.Count(), result.Value.Total);
            Assert.NotNull(gdprAuditServiceFake.TraceRangeAsyncCalledWith);
        }

        [Fact]
        public async Task Get_ReturnsData_NoFilter()
        {
            // Assign
            const int educationalInstitutionId = 1;

            var applicationStatusId = Guid.NewGuid();
            var contactPersonId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonEducationalStatusId = Guid.NewGuid();
            var resourceTargetPersonEducationalSubStatusId = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var resourceTargetPersonWorkStatusId = Guid.NewGuid();
            var submitterPersonId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
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

            await db.PersonTechnicals.AddAsync(new PersonTechnical
            {
                Id = contactPersonId
            });

            await db.PersonTechnicals.AddAsync(new PersonTechnical
            {
                Id = resourceTargetPersonId
            });

            await db.PersonTechnicals.AddAsync(new PersonTechnical
            {
                Id = submitterPersonId
            });

            await db.Persons.AddAsync(new Person
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                PrivatePersonalIdentifier = string.Empty,
                PersonTechnicalId = contactPersonId
            });

            await db.Persons.AddAsync(new Person
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                PrivatePersonalIdentifier = string.Empty,
                PersonTechnicalId = resourceTargetPersonId
            });

            await db.Persons.AddAsync(new Person
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                PrivatePersonalIdentifier = string.Empty,
                PersonTechnicalId = submitterPersonId
            });

            await db.EducationalInstitutions.AddAsync(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                StatusId = statusClassifierId,
                SupervisorId = 1,
                Code = string.Empty,
                Name = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = applicationStatusId,
                Type = ClassifierTypes.ApplicationStatus,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceSubTypeId,
                Type = ClassifierTypes.ResourceSubType,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceTargetPersonEducationalStatusId,
                Type = ClassifierTypes.ResourceTargetPersonEducationalStatus,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceTargetPersonEducationalSubStatusId,
                Type = ClassifierTypes.ResourceTargetPersonEducationalSubStatus,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceTargetPersonTypeId,
                Type = ClassifierTypes.ResourceTargetPersonType,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceTargetPersonWorkStatusId,
                Type = ClassifierTypes.ResourceTargetPersonWorkStatus,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = submitterTypeId,
                Type = ClassifierTypes.ApplicantRole,
                Code = string.Empty,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            await db.Applications.AddAsync(new Domain.Entities.Application(applicationStatusId)
            {
                ApplicationDate = DateTime.UtcNow,
                ApplicationNumber = Guid.NewGuid().ToString().Substring(0, 10),
                ContactPersonId = contactPersonId,
                CreatedById = Guid.NewGuid(),
                EducationalInstitutionId = educationalInstitutionId,
                Id = Guid.NewGuid(),
                ModifiedById = Guid.NewGuid(),
                ResourceSubTypeId = resourceSubTypeId,
                ResourceTargetPersonEducationalStatusId = resourceTargetPersonEducationalStatusId,
                ResourceTargetPersonEducationalSubStatusId = resourceTargetPersonEducationalSubStatusId,
                ResourceTargetPersonId = resourceTargetPersonId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonWorkStatusId = resourceTargetPersonWorkStatusId,
                SubmitterPersonId = submitterPersonId,
                SubmitterTypeId = submitterTypeId
            });
            await db.Applications.AddAsync(new Domain.Entities.Application(applicationStatusId)
            {
                ApplicationDate = DateTime.UtcNow,
                ApplicationNumber = Guid.NewGuid().ToString().Substring(0, 10),
                ContactPersonId = contactPersonId,
                CreatedById = Guid.NewGuid(),
                EducationalInstitutionId = educationalInstitutionId,
                Id = Guid.NewGuid(),
                ModifiedById = Guid.NewGuid(),
                ResourceSubTypeId = resourceSubTypeId,
                ResourceTargetPersonEducationalStatusId = resourceTargetPersonEducationalStatusId,
                ResourceTargetPersonEducationalSubStatusId = resourceTargetPersonEducationalSubStatusId,
                ResourceTargetPersonId = resourceTargetPersonId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonWorkStatusId = resourceTargetPersonWorkStatusId,
                SubmitterPersonId = submitterPersonId,
                SubmitterTypeId = submitterTypeId
            });

            await db.SaveChangesAsync();

            applicationServiceFake.Applications = db.Applications.AsQueryable();

            var pagingRequest = new PagingRequest
            {
                Page = 1,
                Take = 1
            };

            // Act
            var result = await controller.Get(paging: pagingRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(applicationServiceFake.Applications.Count(), result.Value.Total);
            Assert.NotNull(gdprAuditServiceFake.TraceRangeAsyncCalledWith);
        }

        [Fact]
        public async Task CheckDuplicate_Applications_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, Guid.NewGuid());

            applicationServiceFake.Applications = db.Applications.AsQueryable();

            var model = new ApplicationCheckDuplicateRequest
            {
                PrivatePersonalIdentifier = "00000000002",
                ResourceSubTypeId = Guid.NewGuid()
            };

            // Act
            _ = await controller.CheckApplicationDuplicate(model);

            // Assert
            Assert.NotNull(applicationServiceFake.GetApplicationDuplicatesCalledWith);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            // Act
            var result = await controller.Update(Guid.NewGuid(), new ApplicationUpdateRequest());

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateApplicationContactPersonInformation_Succeeds()
        {
            // Assign
            var applicationId = Guid.NewGuid();

            var request = new ApplicationContactPersonUpdateRequest
            {
                ApplicationContactInformation = new List<ApplicationContactPersonUpdateRequest.ContactInformation>
                {
                    new ApplicationContactPersonUpdateRequest.ContactInformation
                    {
                        TypeId = Guid.NewGuid(),
                        Value = string.Empty
                    },
                    new ApplicationContactPersonUpdateRequest.ContactInformation
                    {
                        TypeId = Guid.NewGuid(),
                        Value = string.Empty
                    }
                }
            };

            // Act
            var result = await controller.UpdateApplicationContactPersonInformation(applicationId, request);

            var with = applicationServiceFake.ChangeSubmitterContactCalledWith;

            var contactWith = with.Item.Person.ContactInformation.First();
            var contactRequest = request.ApplicationContactInformation.First();

            // Assert
            Assert.Equal(with.Item.Person.ContactInformation.Count(), request.ApplicationContactInformation.Count());
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

            var resourceTargetPersonId = Guid.NewGuid();

            var app1 = CreateApplication(db, Guid.NewGuid());
            var app2 = CreateApplication(db, Guid.NewGuid());
            var app3 = CreateApplication(db, Guid.NewGuid());

            app1.ResourceTargetPersonId 
                = app2.ResourceTargetPersonId
                = resourceTargetPersonId;

            db.SaveChanges();

            var request = new ApplicationsContactPersonUpdateRequest
            {
                ResourceTargetPersonId = resourceTargetPersonId,
                ContactPerson = new ApplicationsContactPersonUpdateRequest.ContactPersonData
                {
                    ApplicationContactInformation = new List<ApplicationsContactPersonUpdateRequest.ContactPersonData.ContactInformation>
                    {
                        new ApplicationsContactPersonUpdateRequest.ContactPersonData.ContactInformation
                        {
                            TypeId = Guid.NewGuid(),
                            Value = string.Empty
                        },
                        new ApplicationsContactPersonUpdateRequest.ContactPersonData.ContactInformation
                        {
                            TypeId = Guid.NewGuid(),
                            Value = string.Empty
                        }
                    }
                }
            };

            applicationServiceFake.Applications = db.Applications.AsQueryable();

            // Act
            var result = await controller.UpdateApplicationsContactPersonInformation(request);

            var with = applicationServiceFake.ChangeSubmitterContactAsyncCalledWithIds;
            var withDto = applicationServiceFake.ChangeSubmitterContactAsyncCalledWithDto;

            var contactWith = withDto.Person.ContactInformation.First();
            var contactRequest = request.ContactPerson.ApplicationContactInformation.First();

            // Assert
            Assert.Equal(withDto.Person.ContactInformation.Count(), request.ContactPerson.ApplicationContactInformation.Count());
            Assert.Equal(contactWith.TypeId, contactRequest.TypeId);
            Assert.Equal(contactWith.Value, contactRequest.Value);
            Assert.Equal(2, with.Count());
            Assert.IsType<NoContentResult>(result);
        }

        private Domain.Entities.Application CreateApplication(AppDbContext db, Guid id)
        {
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var resourceTargetPersonEducationalStatusId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = resourceSubTypeId,
                    Type = ClassifierTypes.ResourceSubType,
                    Code = string.Empty,
                    Value = string.Empty
                },
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

            db.SaveChanges();

            var applications = new Domain.Entities.Application(applicationStatusId)
            {
                Id = id,
                ApplicationNumber = Guid.NewGuid().ToString().Substring(0, 10),
                ApplicationDate = DateTime.UtcNow,
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ContactPersonId = Guid.NewGuid(),
                ResourceTargetPersonGroup = "group",
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonId = Guid.NewGuid(),
                ResourceTargetPersonEducationalStatusId = resourceTargetPersonEducationalStatusId
            };

            db.Applications.Add(applications);

            db.SaveChanges();

            return applications;
        }
    }
}
