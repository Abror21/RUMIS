using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.EServices.Dtos;
using Izm.Rumis.Infrastructure.Services;
using Izm.Rumis.Infrastructure.Tests.Common;
using Izm.Rumis.Infrastructure.Viis;
using Izm.Rumis.Infrastructure.Vraa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Infrastructure.Tests
{
    public sealed class EServicesServiceTests
    {
        [Fact]
        public async Task GetApplicationDuplicatesAsync_Succeeds()
        {
            using var db = ServiceFactory.ConnectDb();

            // Assign
            var dto = new EServiceApplicationCheckDuplicateDto
            {
                PrivatePersonalIdentifier = "PrivatePersonalIdentifier",
                ResourceSubTypeId = Guid.NewGuid()
            };

            CreateApplication(db, Guid.NewGuid(), Guid.NewGuid());

            var applicationService = ServiceFactory.CreateApplicationService();
            applicationService.Applications = db.Applications.AsQueryable();

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var vraaUser = ServiceFactory.CreateVraaUser();

            var service = GetService(
                db,
                vraaUser: vraaUser,
                applicationService: applicationService,
                authorizationService: authorizationService
                );

            // Act
            _ = await service.GetApplicationDuplicatesAsync(dto);

            var calledWith = applicationService.GetApplicationDuplicatesCalledWith;

            // Assert
            Assert.Equal(vraaUser.PrivatePersonalIdentifier + dto.PrivatePersonalIdentifier, authorizationService.AuthorizeAsyncCalledWith);
            Assert.Equal(dto.PrivatePersonalIdentifier, calledWith.PrivatePersonalIdentifier);
            Assert.Equal(dto.ResourceSubTypeId, calledWith.ResourceSubTypeId);
        }

        [Fact]
        public async Task CreateApplicationAsync_CheckData()
        {
            // Assign
            var dto = new EServiceApplicationCreateDto
            {
                ApplicationSocialStatuses = new Guid[]
                {
                    Guid.NewGuid(),
                    Guid.NewGuid()
                },
                ApplicationStatusHistory = "ApplicationStatusHistory",
                EducationalInstitutionId = 1,
                Notes = "Notes",
                ResourceSubTypeId = Guid.NewGuid(),
                ResourceTargetPerson = new EServiceApplicationCreateDto.PersonData
                {
                    ContactInformation = new EServiceApplicationCreateDto.PersonData.ContactData[]
                    {
                        new EServiceApplicationCreateDto.PersonData.ContactData
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
                SubmitterContactData = new EServiceApplicationCreateDto.PersonData.ContactData[]
                {
                    new EServiceApplicationCreateDto.PersonData.ContactData
                        {
                            TypeId = Guid.NewGuid(),
                            Value = "Value1"
                        }
                },
                SubmitterTypeId = Guid.NewGuid()
            };

            using var db = ServiceFactory.ConnectDb();

            var applicationService = ServiceFactory.CreateApplicationService();

            var vraaUser = ServiceFactory.CreateVraaUser();

            var service = GetService(
                db: db,
                applicationService: applicationService,
                vraaUser: vraaUser
                );

            // Act
            _ = await service.CreateApplicationAsync(dto);

            var with = applicationService.CreateAsyncCalledWith;

            var resourceTargetContactWith = applicationService.CreateAsyncCalledWith.ResourceTargetPerson.ContactInformation.First();
            var resourceTargetContact = dto.ResourceTargetPerson.ContactInformation.First();

            var submitterContactWith = applicationService.CreateAsyncCalledWith.SubmitterPerson.ContactInformation.First();
            var submitterContact = dto.SubmitterContactData.First();

            // Assert
            Assert.Equal(with.ApplicationSocialStatuses.Count(), dto.ApplicationSocialStatuses.Count());
            Assert.True(Enumerable.SequenceEqual(with.ApplicationSocialStatuses, dto.ApplicationSocialStatuses));
            Assert.Equal(with.ApplicationStatusHistory, dto.ApplicationStatusHistory);
            Assert.Equal(with.EducationalInstitutionId, dto.EducationalInstitutionId);
            Assert.Equal(with.Notes, dto.Notes);
            Assert.Equal(with.ResourceSubTypeId, dto.ResourceSubTypeId);
            Assert.Equal(with.ResourceTargetPerson.ContactInformation.Count(), dto.ResourceTargetPerson.ContactInformation.Count());
            Assert.Equal(resourceTargetContactWith.TypeId, resourceTargetContact.TypeId);
            Assert.Equal(resourceTargetContactWith.Value, resourceTargetContact.Value);
            Assert.Equal(with.ResourceTargetPerson.FirstName, dto.ResourceTargetPerson.FirstName);
            Assert.Equal(with.ResourceTargetPerson.LastName, dto.ResourceTargetPerson.LastName);
            Assert.Equal(with.ResourceTargetPerson.PrivatePersonalIdentifier, dto.ResourceTargetPerson.PrivatePersonalIdentifier);
            Assert.Equal(with.ResourceTargetPersonClassGrade, dto.ResourceTargetPersonClassGrade);
            Assert.Equal(with.ResourceTargetPersonClassParallel, dto.ResourceTargetPersonClassParallel);
            Assert.Equal(with.ResourceTargetPersonEducationalProgram, dto.ResourceTargetPersonEducationalProgram);
            Assert.Equal(with.ResourceTargetPersonEducationalStatusId, dto.ResourceTargetPersonEducationalStatusId);
            Assert.Equal(with.ResourceTargetPersonEducationalSubStatusId, dto.ResourceTargetPersonEducationalSubStatusId);
            Assert.Equal(with.ResourceTargetPersonGroup, dto.ResourceTargetPersonGroup);
            Assert.Equal(with.ResourceTargetPersonTypeId, dto.ResourceTargetPersonTypeId);
            Assert.Equal(with.ResourceTargetPersonWorkStatusId, dto.ResourceTargetPersonWorkStatusId);
            Assert.Equal(with.SocialStatus, dto.SocialStatus);
            Assert.Equal(with.SocialStatusApproved, dto.SocialStatusApproved);
            Assert.Equal(with.SubmitterPerson.ContactInformation.Count(), dto.SubmitterContactData.Count());
            Assert.Equal(submitterContactWith.TypeId, submitterContact.TypeId);
            Assert.Equal(submitterContactWith.Value, submitterContact.Value);
            Assert.Equal(with.SubmitterPerson.FirstName, vraaUser.FirstName);
            Assert.Equal(with.SubmitterPerson.LastName, vraaUser.LastName);
            Assert.Equal(with.SubmitterPerson.PrivatePersonalIdentifier, vraaUser.PrivatePersonalIdentifier);
            Assert.Equal(with.SubmitterTypeId, dto.SubmitterTypeId);
        }

        [Fact]
        public async Task CreateApplicationAsync_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var applicationService = ServiceFactory.CreateApplicationService();

            var service = GetService(
                db: db,
                applicationService: applicationService
                );

            // Act
            _ = await service.CreateApplicationAsync(new EServiceApplicationCreateDto());

            // Assert
            Assert.NotNull(applicationService.CreateAsyncCalledWith);
        }

        [Fact]
        public async Task GetClassifiers_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();
            var types = new[] { ClassifierTypes.SocialStatus };

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = Guid.NewGuid(),
                    Code = ClassifierTypes.SocialStatus,
                    Value = "v",
                    Payload = "{'InEServices': true}",
                    Type = ClassifierTypes.ClassifierType
                },
                new Classifier
                {
                    Id = Guid.NewGuid(),
                    Code = "c",
                    Value = "v",
                    Type = ClassifierTypes.SocialStatus
                });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var result = service.GetClassifiers(types);

            // Assert
            Assert.Single(result.List());
        }

        [Fact]
        public async Task GetClassifiers_Empty()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();
            var types = new[] { ClassifierTypes.SocialStatus };

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = Guid.NewGuid(),
                    Code = ClassifierTypes.SocialStatus,
                    Value = "v",
                    Type = ClassifierTypes.ClassifierType
                },
                new Classifier
                {
                    Id = Guid.NewGuid(),
                    Code = "c",
                    Value = "v",
                    Type = ClassifierTypes.SocialStatus
                });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var result = service.GetClassifiers(types);

            // Assert
            Assert.Empty(result.List());
        }

        [Fact]
        public async Task GetApplications_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();
            var privatePersonalIdentifiers = new[] { "123" };
            var personTechnicalsId = Guid.NewGuid();

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = personTechnicalsId,
                UserId = Guid.NewGuid()
            });

            db.Persons.Add(new Person
            {
                PersonTechnicalId = personTechnicalsId,
                FirstName = "f",
                LastName = "l",
                PrivatePersonalIdentifier = privatePersonalIdentifiers.First()
            });

            await db.SaveChangesAsync();

            CreateApplication(db, Guid.NewGuid(), personTechnicalsId);

            var service = GetService(db);

            // Act
            var result = service.GetApplications(privatePersonalIdentifiers);

            // Assert
            Assert.Single(result.List());
        }

        [Fact]
        public async Task GetApplications_Empty()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();
            var privatePersonalIdentifiers = new[] { "123" };
            var personTechnicalsId = Guid.NewGuid();

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = personTechnicalsId,
                UserId = Guid.NewGuid()
            });

            db.Persons.Add(new Person
            {
                PersonTechnicalId = personTechnicalsId,
                FirstName = "f",
                LastName = "l",
                PrivatePersonalIdentifier = "p"
            });

            await db.SaveChangesAsync();

            CreateApplication(db, Guid.NewGuid(), personTechnicalsId);

            var service = GetService(db);

            // Act
            var result = service.GetApplications(privatePersonalIdentifiers);

            // Assert
            Assert.Empty(result.List());
        }

        [Fact]
        public async Task GetEducationalInstitutions_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();
            var regNrs = new[] { "c" };
            var statusClassifierId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            await db.SaveChangesAsync();

            db.EducationalInstitutions.Add(new EducationalInstitution
            {
                Id = 1,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                Supervisor = new Supervisor
                {
                    Id = 1,
                    Code = "c",
                    Name = "n"
                }
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var result = service.GetEducationalInstitutions(regNrs);

            // Assert
            Assert.Single(result.List());
        }

        [Fact]
        public async Task GetEducationalInstitutions_Empty()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();
            var regNrs = new[] { "c" };
            var statusClassifierId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            await db.SaveChangesAsync();

            db.EducationalInstitutions.Add(new EducationalInstitution
            {
                Id = 1,
                Code = "cdqdq",
                Name = "n",
                StatusId = statusClassifierId,
                Supervisor = new Supervisor
                {
                    Id = 1,
                    Code = "c",
                    Name = "n"
                }
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var result = service.GetEducationalInstitutions(regNrs);

            // Assert
            Assert.Empty(result.List());
        }

        [Fact]
        public async Task ChangeSubmitterContact_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var applicationId = Guid.NewGuid();

            var request = new EServiceApplicationChangeContactPersonDto
            {
                ContactInformationData = new List<EServiceApplicationChangeContactPersonDto.ContactInformation>
                {
                    new EServiceApplicationChangeContactPersonDto.ContactInformation
                    {
                        TypeId = Guid.NewGuid(),
                        Value = string.Empty
                    },
                    new EServiceApplicationChangeContactPersonDto.ContactInformation
                    {
                        TypeId = Guid.NewGuid(),
                        Value = string.Empty
                    }
                }
            };

            var applicationService = ServiceFactory.CreateApplicationService();

            var service = GetService(
                db: db,
                applicationService: applicationService
                );

            // Act
            await service.ChangeSubmitterContactAsync(applicationId, request);

            var with = applicationService.ChangeSubmitterContactCalledWith;

            var contactWith = with.Item.Person.ContactInformation.First();
            var contactRequest = request.ContactInformationData.First();

            // Assert
            Assert.Equal(with.Item.Person.ContactInformation.Count(), request.ContactInformationData.Count());
            Assert.Equal(contactWith.TypeId, contactRequest.TypeId);
            Assert.Equal(contactWith.Value, contactRequest.Value);
            Assert.Equal(applicationId, with.Id);
        }

        [Fact]
        public async Task ChangeSubmittersContact_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var applicationId = Guid.NewGuid();

            var request = new EServiceApplicationsChangeContactPersonDto
            {
                ContactInformationData = new List<EServiceApplicationsChangeContactPersonDto.ContactInformation>
                {
                    new EServiceApplicationsChangeContactPersonDto.ContactInformation
                    {
                        TypeId = Guid.NewGuid(),
                        Value = string.Empty
                    },
                    new EServiceApplicationsChangeContactPersonDto.ContactInformation
                    {
                        TypeId = Guid.NewGuid(),
                        Value = string.Empty
                    }
                }
            };

            var applicationService = ServiceFactory.CreateApplicationService();

            var service = GetService(
                db: db,
                applicationService: applicationService
                );

            // Act
            await service.ChangeSubmittersContactAsync(new[] { applicationId }, request);

            var with = applicationService.ChangeSubmitterContactAsyncCalledWithIds;
            var withDto = applicationService.ChangeSubmitterContactAsyncCalledWithDto;

            var contactWith = withDto.Person.ContactInformation.First();
            var contactRequest = request.ContactInformationData.First();

            // Assert
            Assert.Equal(withDto.Person.ContactInformation.Count(), request.ContactInformationData.Count());
            Assert.Equal(contactWith.TypeId, contactRequest.TypeId);
            Assert.Equal(contactWith.Value, contactRequest.Value);
            Assert.Single(with);
        }

        [Fact]
        public async Task ChangeStatusToLostAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            var dto = new EServiceChangeStatusDto
            {
                FileToDeleteIds = new[] { fileId },
                Files = new[] { FakeFileDto("a.docx") },
                Notes = "Notes"
            };

            using var db = ServiceFactory.ConnectDb();

            var applicationResourceService = ServiceFactory.CreateApplicationResourceService();

            var service = GetService(db, null, applicationResourceService);

            // Act
            await service.ChangeStatusToLostAsync(id, dto);

            var withId = applicationResourceService.ChangeStatusToLostAsyncCalledWithId;
            var withDto = applicationResourceService.ChangeStatusToLostAsyncCalledWithDto;

            // Assert
            Assert.Equal(withId, id);
            Assert.Equal(withDto.FileToDeleteIds.Count(), dto.FileToDeleteIds.Count());
            Assert.True(Enumerable.SequenceEqual(withDto.FileToDeleteIds, dto.FileToDeleteIds));
            Assert.Equal(withDto.Notes, dto.Notes);
        }

        [Fact]
        public async Task ChangeStatusToStolenAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            var dto = new EServiceChangeStatusDto
            {
                FileToDeleteIds = new[] { fileId },
                Files = new[] { FakeFileDto("a.docx") },
                Notes = "Notes"
            };

            using var db = ServiceFactory.ConnectDb();

            var applicationResourceService = ServiceFactory.CreateApplicationResourceService();

            var service = GetService(db, null, applicationResourceService);

            // Act
            await service.ChangeStatusToStolenAsync(id, dto);

            var withId = applicationResourceService.ChangeStatusToStolenAsyncCalledWithId;
            var withDto = applicationResourceService.ChangeStatusToStolenAsyncCalledWithDto;

            // Assert
            Assert.Equal(withId, id);
            Assert.Equal(withDto.FileToDeleteIds.Count(), dto.FileToDeleteIds.Count());
            Assert.True(Enumerable.SequenceEqual(withDto.FileToDeleteIds, dto.FileToDeleteIds));
            Assert.Equal(withDto.Notes, dto.Notes);
        }

        [Fact]
        public async Task SignApplicationResourceAsync_Succeeds()
        {
            // Assign 
            var id = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = resourceTargetPersonId
            });

            db.Persons.Add(new Person
            {
                PersonTechnicalId = resourceTargetPersonId,
                PrivatePersonalIdentifier = "00000000001"
            });

            CreateApplication(
                db: db,
                id: id,
                resourceTargetPersonId: resourceTargetPersonId,
                applicationResourceId: resourceId
                );

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationResourceService = ServiceFactory.CreateApplicationResourceService();

            var service = GetService(
                db: db,
                authorizationService: authorizationService,
                applicationResourceService: applicationResourceService
                );

            // Act
            await service.SignApplicationResourceAsync(resourceId);

            // Assert
            Assert.Equal(applicationResourceService.SignAsyncCalledWith, resourceId);
            Assert.NotNull(authorizationService.AuthorizeAsyncCalledWith);
        }

        [Fact]
        public async Task SignApplicationResourceAsync_Throws_NotFound()
        {
            // Assign 
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.SignApplicationResourceAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task WithdrawApplicationAsync_Succeeds()
        {
            // Assign 
            var id = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = resourceTargetPersonId
            });

            db.Persons.Add(new Person
            {
                PersonTechnicalId = resourceTargetPersonId,
                PrivatePersonalIdentifier = "00000000001"
            });

            CreateApplication(
                db: db,
                id: id,
                resourceTargetPersonId: resourceTargetPersonId,
                applicationResourceId: resourceId
                );

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationService = ServiceFactory.CreateApplicationService();

            var service = GetService(
                db: db,
                authorizationService: authorizationService,
                applicationService: applicationService
                );

            // Act
            await service.WithdrawApplicationAsync(id);

            // Assert
            Assert.Equal(applicationService.WithdrawAsyncCalledWith, id);
            Assert.NotNull(authorizationService.AuthorizeAsyncCalledWith);
        }

        [Fact]
        public async Task WithdrawApplicationAsync_Throws_NotFound()
        {
            // Assign 
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.WithdrawApplicationAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetApplicationResourcePnaAsync_Succeeds()
        {
            // Assign
            var applicationId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = resourceTargetPersonId
            });

            db.Persons.Add(new Person
            {
                PersonTechnicalId = resourceTargetPersonId,
                PrivatePersonalIdentifier = "00000000001"
            });

            CreateApplication(
                db: db,
                id: applicationId,
                resourceTargetPersonId: resourceTargetPersonId,
                applicationResourceId: applicationResourceId
                );

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationResourceService = ServiceFactory.CreateApplicationResourceService();

            var service = GetService(
                db: db,
                authorizationService: authorizationService,
                applicationResourceService: applicationResourceService);

            // Act
            await service.GetApplicationResourcePnaAsync(applicationResourceId);

            // Assert
            Assert.Equal(applicationResourceService.GetPnaAsyncCalledWith, applicationResourceId);
            Assert.NotNull(authorizationService.AuthorizeAsyncCalledWith);
        }

        [Fact]
        public async Task GetApplicationResourcePnaAsync_Throws_NotFound()
        {
            // Assign 
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetApplicationResourcePnaAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetApplicationResourcePnaPdfAsync_Succeeds()
        {
            // Assign
            var applicationId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = resourceTargetPersonId
            });

            db.Persons.Add(new Person
            {
                PersonTechnicalId = resourceTargetPersonId,
                PrivatePersonalIdentifier = "00000000001"
            });

            CreateApplication(
                db: db,
                id: applicationId,
                resourceTargetPersonId: resourceTargetPersonId,
                applicationResourceId: applicationResourceId
                );

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationResourceService = ServiceFactory.CreateApplicationResourceService();

            var service = GetService(
                db: db,
                authorizationService: authorizationService,
                applicationResourceService: applicationResourceService);

            // Act
            await service.GetApplicationResourcePnaPdfAsync(applicationResourceId);

            // Assert
            Assert.Equal(applicationResourceService.GetPnaPdfAsyncCalledWith, applicationResourceId);
            Assert.NotNull(authorizationService.AuthorizeAsyncCalledWith);
        }

        [Fact]
        public async Task GetApplicationResourcePnaPdfAsync_Throws_NotFound()
        {
            // Assign 
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetApplicationResourcePnaPdfAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetApplicationResourceExploitationRulesAsync_Succeeds()
        {
            // Assign
            var applicationId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = resourceTargetPersonId
            });

            db.Persons.Add(new Person
            {
                PersonTechnicalId = resourceTargetPersonId,
                PrivatePersonalIdentifier = "00000000001"
            });

            CreateApplication(
                db: db,
                id: applicationId,
                resourceTargetPersonId: resourceTargetPersonId,
                applicationResourceId: applicationResourceId
                );

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationResourceService = ServiceFactory.CreateApplicationResourceService();

            var service = GetService(
                db: db,
                authorizationService: authorizationService,
                applicationResourceService: applicationResourceService);

            // Act
            await service.GetApplicationResourceExploitationRulesAsync(applicationResourceId);

            // Assert
            Assert.Equal(applicationResourceService.GetExploitationRulesAsyncCalledWith, applicationResourceId);
            Assert.NotNull(authorizationService.AuthorizeAsyncCalledWith);
        }

        [Fact]
        public async Task GetApplicationResourceExploitationRulesAsync_Throws_NotFound()
        {
            // Assign 
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetApplicationResourceExploitationRulesAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetApplicationResourceExploitationRulesPdfAsync_Succeeds()
        {
            // Assign
            var applicationId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = resourceTargetPersonId
            });

            db.Persons.Add(new Person
            {
                PersonTechnicalId = resourceTargetPersonId,
                PrivatePersonalIdentifier = "00000000001"
            });

            CreateApplication(
                db: db,
                id: applicationId,
                resourceTargetPersonId: resourceTargetPersonId,
                applicationResourceId: applicationResourceId
                );

            var authorizationService = ServiceFactory.CreateAuthorizationService();
            var applicationResourceService = ServiceFactory.CreateApplicationResourceService();

            var service = GetService(
                db: db,
                authorizationService: authorizationService,
                applicationResourceService: applicationResourceService);

            // Act
            await service.GetApplicationResourceExploitationRulesPdfAsync(applicationResourceId);

            // Assert
            Assert.Equal(applicationResourceService.GetExploitationRulesPdfAsyncCalledWith, applicationResourceId);
            Assert.NotNull(authorizationService.AuthorizeAsyncCalledWith);
        }

        [Fact]
        public async Task GetApplicationResourceExploitationRulesPdfAsync_Throws_NotFound()
        {
            // Assign 
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetApplicationResourceExploitationRulesPdfAsync(Guid.NewGuid()));
        }

        [Theory]
        [InlineData(DocumentType.PNA)]
        [InlineData(DocumentType.ExploitationRules)]
        public async Task GetApplicationResourceDocumentSampleAsync_Succeeds(string docTempCode)
        {
            // Assign
            var eduStatusId = Guid.NewGuid();
            var fileId = Guid.NewGuid();
            var resourceTypeId = Guid.NewGuid();

            const int educationalInstitutionId = 1;
            const string content = "content";

            using var db = ServiceFactory.ConnectDb();

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceTypeId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.ResourceType
            });

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = eduStatusId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            await db.SaveChangesAsync();

            await db.EducationalInstitutions.AddAsync(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = eduStatusId,
            });

            await db.DocumentTemplates.AddAsync(new DocumentTemplate
            {
                Code = docTempCode,
                Title = "Title",
                FileId = fileId,
                ResourceTypeId = resourceTypeId
            });

            var fileService = ServiceFactory.CreateFileService();

            await fileService.AddOrUpdateAsync(fileId, new FileDto
            {
                FileName = "template.html",
                ContentType = MediaTypeNames.Text.Html,
                Content = Encoding.UTF8.GetBytes(content)
            });

            await db.SaveChangesAsync();

            var documentTemplateService = ServiceFactory.CreateDocumentTemplateService();
            documentTemplateService.DocumentTemplates = db.DocumentTemplates.AsQueryable();

            var service = GetService(
                db: db,
                documentTemplateService: documentTemplateService,
                fileService: fileService);

            // Act
            var result = await service.GetDocumentSampleAsync(educationalInstitutionId, docTempCode);

            // Assert
            Assert.Equal(educationalInstitutionId, documentTemplateService.GetByEducationalInstitutionCalledWith);
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task GetApplicationResourceDocumentSampleAsync_Throws_NotFound()
        {
            // Assign 
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetDocumentSampleAsync(1, DocumentType.PNA));
        }

        [Fact]
        public void GetDocumentTemplates_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;

            var resourceTypeId1 = Guid.NewGuid();
            var resourceTypeId2 = Guid.NewGuid();

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = resourceTypeId1,
                    Type = ClassifierTypes.ResourceType,
                    Code = "c",
                    Value = "v"
                },
                new Classifier
                {
                    Id = resourceTypeId2,
                    Type = ClassifierTypes.ResourceType,
                    Code = "c",
                    Value = "v"
                });

            db.SaveChanges();

            db.DocumentTemplates.AddRange(
                new DocumentTemplate
                {
                    Code = DocumentType.PNA,
                    Title = "t",
                    ResourceTypeId = resourceTypeId1
                },
                new DocumentTemplate
                {
                    Code = DocumentType.ExploitationRules,
                    Title = "t",
                    ResourceTypeId = resourceTypeId1
                },
                new DocumentTemplate
                {
                    Code = DocumentType.Hyperlink,
                    Title = "t",
                    ResourceTypeId = resourceTypeId2
                });

            db.SaveChanges();

            var dto = new EServiceDocumentTemplatesDto
            {
                ResourceTypeId = resourceTypeId1,
                EducationalInstitutionId = educationalInstitutionId
            };

            var documentTemplateService = ServiceFactory.CreateDocumentTemplateService();
            documentTemplateService.DocumentTemplates = db.DocumentTemplates.AsQueryable();

            var service = GetService(
                db: db,
                documentTemplateService: documentTemplateService
                );

            // Act
            var result = service.GetDocumentTemplates(dto);

            // Assert
            Assert.Single(result.List());
            Assert.Equal(educationalInstitutionId, documentTemplateService.GetByEducationalInstitutionCalledWith);
        }

        [Fact]
        public async Task GetDocumentTemplateAsync_Succeeds()
        {
            // Assign
            const int id = 1;
            const string content = "content";

            var fileId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.DocumentTemplates.Add(new DocumentTemplate
            {
                Id = id,
                FileId = fileId
            });

            var fileService = ServiceFactory.CreateFileService();

            await fileService.AddOrUpdateAsync(fileId, new FileDto
            {
                FileName = "template.html",
                ContentType = MediaTypeNames.Text.Html,
                Content = Encoding.UTF8.GetBytes(content)
            });

            var service = GetService(
                db: db,
                fileService: fileService);

            // Act
            var result = await service.GetDocumentTemplateAsync(id);

            // Assert
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task GetDocumentTemplateAsync_Throws_NotFound()
        {
            // Assign 
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetDocumentTemplateAsync(1));
        }

        [Fact]
        public async Task GetDocumentTemplatePdfAsync_Succeeds()
        {
            // Assign
            const int id = 1;
            const string content = "content";

            var fileId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.DocumentTemplates.Add(new DocumentTemplate
            {
                Id = id,
                FileId = fileId
            });

            var fileService = ServiceFactory.CreateFileService();

            await fileService.AddOrUpdateAsync(fileId, new FileDto
            {
                FileName = "template.html",
                ContentType = MediaTypeNames.Text.Html,
                Content = Encoding.UTF8.GetBytes(content)
            });

            var service = GetService(
                db: db,
                fileService: fileService);

            // Act
            var result = await service.GetDocumentTemplatePdfAsync(id);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetDocumentTemplatePdfAsync_Throws_NotFound()
        {
            // Assign 
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetDocumentTemplatePdfAsync(1));
        }

        private EServicesService GetService(
            IAppDbContext db,
            IApplicationService applicationService = null,
            IApplicationResourceService applicationResourceService = null,
            IAuthorizationService authorizationService = null,
            IVraaUser vraaUser = null,
            IViisService viisService = null,
            IGdprAuditService gdprAuditService = null,
            IFileService fileService = null,
            IDocumentTemplateService documentTemplateService = null)
        {
            return new EServicesService(
                db: db,
                applicationService: applicationService ?? ServiceFactory.CreateApplicationService(),
                applicationResourceService: applicationResourceService ?? ServiceFactory.CreateApplicationResourceService(),
                authorizationService: authorizationService ?? ServiceFactory.CreateAuthorizationService(),
                vraaUser: vraaUser ?? ServiceFactory.CreateVraaUser(),
                viisService: viisService ?? ServiceFactory.CreateViisService(),
                gdprAuditService: gdprAuditService ?? ServiceFactory.CreateGdprAuditService(),
                fileService: fileService ?? ServiceFactory.CreateFileService(),
                documentTemplateService: documentTemplateService ?? ServiceFactory.CreateDocumentTemplateService()
                );
        }

        private void CreateApplication(
            IAppDbContext db,
            Guid id,
            Guid resourceTargetPersonId,
            Guid? applicationResourceId = null,
            Guid? resourceSubTypeId = null,
            string applicationStatusCode = ApplicationStatus.Confirmed,
            string pnaStatusCode = PnaStatus.Lost)
        {
            var applicationStatusId = Guid.NewGuid();
            var pnaStatusId = Guid.NewGuid();
            var disability = "Invaliditāte";
            var socialStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var documentTypeId = Guid.NewGuid();
            applicationResourceId ??= Guid.NewGuid();

            if (!resourceSubTypeId.HasValue)
            {
                resourceSubTypeId = Guid.NewGuid();

                db.Classifiers.Add(new Classifier
                {
                    Id = resourceSubTypeId.Value,
                    Type = ClassifierTypes.ResourceSubType,
                    Code = string.Empty,
                    Value = string.Empty
                });
            }

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
                    Id = pnaStatusId,
                    Type = ClassifierTypes.PnaStatus,
                    Code = pnaStatusCode,
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
                    Id = socialStatusId,
                    Type = ClassifierTypes.SocialStatus,
                    Code = string.Empty,
                    Value = disability
                },
                new Classifier
                {
                    Id = documentTypeId,
                    Type = ClassifierTypes.DocumentType,
                    Code = string.Empty,
                    Value = string.Empty
                });


            db.SaveChanges();

            db.Applications.Add(new Domain.Entities.Application(applicationStatusId)
            {
                Id = id,
                ApplicationNumber = Guid.NewGuid().ToString().Substring(0, 10),
                ApplicationDate = DateTime.UtcNow,
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId.Value,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonId = resourceTargetPersonId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId
            });

            db.ApplicationSocialStatuses.Add(new ApplicationSocialStatus
            {
                ApplicationId = id,
                SocialStatusId = socialStatusId,
                Id = Guid.NewGuid()
            });

            var resource = new ApplicationResource
            {
                ApplicationId = id,
                Id = applicationResourceId.Value,
                PNANumber = string.Empty
            };

            resource.SetPnaStatus(pnaStatusId);

            db.ApplicationResources.Add(resource);

            db.ApplicationResourceAttachments.Add(new ApplicationResourceAttachment
            {
                ApplicationResourceId = applicationResourceId.Value,
                Id = Guid.NewGuid(),
                DocumentTypeId = documentTypeId,
            });

            db.SaveChanges();
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
    }
}
