using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Helpers;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class ApplicationServiceTests
    {
        [Fact]
        public async Task CheckApplicationSocialStatusAsync_Succeeds()
        {
            // Assign
            var applicationStatusId = Guid.NewGuid();
            var disability = "Invaliditāte";
            var id = Guid.NewGuid();
            var socialStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = applicationStatusId,
                Type = ClassifierTypes.ApplicationStatus,
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
                Id = resourceTargetPersonTypeId,
                Type = ClassifierTypes.ResourceTargetPersonType,
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
                Id = socialStatusId,
                Type = ClassifierTypes.SocialStatus,
                Code = SocialStatus.Disability,
                Value = disability
            });

            await db.Persons.AddAsync(new Person
            {
                ActiveFrom = DateTime.Now,
                PrivatePersonalIdentifier = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                PersonTechnical = new PersonTechnical
                {
                    Id = resourceTargetPersonId,
                }
            });

            await db.SaveChangesAsync();

            await db.Applications.AddAsync(new Domain.Entities.Application(applicationStatusId)
            {
                Id = id,
                ApplicationNumber = NumberingPatternHelper.ApplicationNumberFormat(),
                ApplicationDate = DateTime.UtcNow,
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonId = resourceTargetPersonId
            });

            await db.ApplicationSocialStatuses.AddAsync(new ApplicationSocialStatus
            {
                ApplicationId = id,
                SocialStatusId = socialStatusId,
                Id = Guid.NewGuid()
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.CheckApplicationSocialStatusAsync(id);

            var application = db.Applications.First();

            // Assert

            Assert.True(application.SocialStatusApproved);
        }

        [Fact]
        public async Task CheckApplicationSocialStatusAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.CheckApplicationSocialStatusAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateAsync_Succeeds_ExistingPerson_NewContactData()
        {
            // Assign
            var applicationStatusId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var contactTypeId = Guid.NewGuid();
            const string privatePersonlIdentifier = "00000000001";
            const string contactValue = "someValue";

            using var db = ServiceFactory.ConnectDb();

            SeedClassifiers(db, applicationStatusId, resourceSubTypeId, resourceTargetPersonTypeId, submitterTypeId, contactTypeId);

            SeedEducationalInstitution(db);

            db.Persons.Add(new Person
            {
                FirstName = "someName",
                LastName = "someName",
                PrivatePersonalIdentifier = privatePersonlIdentifier,
                PersonTechnical = new PersonTechnical()
            });

            await db.SaveChangesAsync();

            var model = new ApplicationCreateDto
            {
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = privatePersonlIdentifier
                },
                SubmitterPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = privatePersonlIdentifier,
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = contactTypeId,
                            Value = contactValue
                        }
                    }
                }
            };

            var validator = ServiceFactory.CreateApplicationValidator();
            var authorization = ServiceFactory.CreateAuthorizationService();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                validator: validator,
                authorizationService: authorization,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await service.CreateAsync(model);

            // Assert
            Assert.Single(db.Applications.ToArray());
            Assert.Equal(NumberingPatternHelper.ApplicationNumberFormat(), db.Applications.First().ApplicationNumber);
            Assert.Single(db.PersonTechnicals);
            Assert.Single(db.Persons);
            Assert.Single(db.PersonContacts);
            Assert.Equal(validator.ValidateAsyncCalledWith, model);
            Assert.Equal(authorization.AuthorizeAsyncRequestedPersonInstitutionCalledWith, model.ResourceTargetPerson.PrivatePersonalIdentifier + NumberingPatternHelper.DefaultInstitution);
            Assert.Equal(authorization.AuthorizeEducationalInstitutionCalledWith, model.EducationalInstitutionId);
            Assert.Equal(authorization.AuthorizeAsyncCalledWith, model.SubmitterPerson.PrivatePersonalIdentifier + model.ResourceTargetPerson.PrivatePersonalIdentifier);
            Assert.True(gdprAuditService.TraceRangeAsyncCalledWith.Any());
        }

        [Fact]
        public async Task CreateAsync_Succeeds_ExistingPerson_SameContactData()
        {
            // Assign
            var applicationStatusId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var contactTypeId = Guid.NewGuid();
            const string privatePersonlIdentifier = "00000000001";
            const string contactValue = "someValue";

            using var db = ServiceFactory.ConnectDb();

            SeedClassifiers(db, applicationStatusId, resourceSubTypeId, resourceTargetPersonTypeId, submitterTypeId, contactTypeId);

            db.Persons.Add(new Person
            {
                FirstName = "someName",
                LastName = "someName",
                PrivatePersonalIdentifier = privatePersonlIdentifier,
                PersonTechnical = new PersonTechnical
                {
                    PersonContacts = new List<PersonContact>
                    {
                        new PersonContact
                        {
                            ContactTypeId = contactTypeId,
                            ContactValue = contactValue
                        }
                    }
                },
            });

            await db.SaveChangesAsync();

            SeedEducationalInstitution(db);

            var model = new ApplicationCreateDto
            {
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = privatePersonlIdentifier
                },
                SubmitterPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = privatePersonlIdentifier,
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = contactTypeId,
                            Value = contactValue
                        }
                    }
                }
            };

            var validator = ServiceFactory.CreateApplicationValidator();
            var authorization = ServiceFactory.CreateAuthorizationService();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                validator: validator,
                authorizationService: authorization,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await service.CreateAsync(model);

            // Assert
            Assert.Single(db.Applications.ToArray());
            Assert.Equal(NumberingPatternHelper.ApplicationNumberFormat(), db.Applications.First().ApplicationNumber);
            Assert.Single(db.PersonTechnicals);
            Assert.Single(db.Persons);
            Assert.Single(db.PersonContacts);
            Assert.Equal(validator.ValidateAsyncCalledWith, model);
            Assert.Equal(authorization.AuthorizeAsyncRequestedPersonInstitutionCalledWith, model.ResourceTargetPerson.PrivatePersonalIdentifier + NumberingPatternHelper.DefaultInstitution);
            Assert.Equal(authorization.AuthorizeEducationalInstitutionCalledWith, model.EducationalInstitutionId);
            Assert.Equal(authorization.AuthorizeAsyncCalledWith, model.SubmitterPerson.PrivatePersonalIdentifier + model.ResourceTargetPerson.PrivatePersonalIdentifier);
            Assert.True(gdprAuditService.TraceRangeAsyncCalledWith.Any());
        }

        [Fact]
        public async Task CreateAsync_Succeeds_NewPersons()
        {
            // Assign
            var applicationStatusId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var contactTypeId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            SeedClassifiers(db, applicationStatusId, resourceSubTypeId, resourceTargetPersonTypeId, submitterTypeId, contactTypeId);

            await db.SaveChangesAsync();

            SeedEducationalInstitution(db);

            var model = new ApplicationCreateDto
            {
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = "00000000000",
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = contactTypeId,
                            Value = string.Empty
                        }
                    }
                },
                SubmitterPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = "00000000001",
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = contactTypeId,
                            Value = string.Empty
                        }
                    }
                }
            };

            var validator = ServiceFactory.CreateApplicationValidator();
            var authorization = ServiceFactory.CreateAuthorizationService();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                validator: validator,
                authorizationService: authorization,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await service.CreateAsync(model);

            var application = db.Applications.First();

            // Assert
            Assert.Single(db.Applications.ToArray());
            Assert.Equal(result.Id, application.Id);
            Assert.Equal(result.Number, application.ApplicationNumber);
            Assert.Equal(NumberingPatternHelper.ApplicationNumberFormat(), db.Applications.First().ApplicationNumber);
            Assert.Equal(2, db.PersonTechnicals.Count());
            Assert.Equal(2, db.Persons.Count());
            Assert.Equal(2, db.PersonContacts.Count());
            Assert.Equal(validator.ValidateAsyncCalledWith, model);
            Assert.Equal(authorization.AuthorizeAsyncRequestedPersonInstitutionCalledWith, model.ResourceTargetPerson.PrivatePersonalIdentifier + NumberingPatternHelper.DefaultInstitution);
            Assert.Equal(authorization.AuthorizeEducationalInstitutionCalledWith, model.EducationalInstitutionId);
            Assert.Equal(authorization.AuthorizeAsyncCalledWith, model.SubmitterPerson.PrivatePersonalIdentifier + model.ResourceTargetPerson.PrivatePersonalIdentifier);
            Assert.True(gdprAuditService.TraceRangeAsyncCalledWith.Any());
        }

        [Theory]
        [InlineData("employee")]
        [InlineData("")]
        public async Task CreateAsync_CheckData_NewPerson_ResourceTargetPersonTypeEmpty(string resourceTargetPersonType)
        {
            // Assign
            const int educationalInstitutionId = 1;
            const string contactValue = "someValue";
            var contactTypeId = Guid.NewGuid();

            var model = new ApplicationCreateDto
            {
                EducationalInstitutionId = educationalInstitutionId,
                ResourceSubTypeId = Guid.NewGuid(),
                SubmitterTypeId = Guid.NewGuid(),
                ResourceTargetPersonTypeId = Guid.NewGuid(),
                ResourceTargetPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = "00000000000",
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = contactTypeId,
                            Value = contactValue
                        }
                    }
                },
                SubmitterPerson = new PersonData
                {
                    FirstName = "someFirstName1",
                    LastName = "someLastName1",
                    PrivatePersonalIdentifier = "00000000001"
                }
            };

            using var db = ServiceFactory.ConnectDb();

            SeedEducationalInstitution(db);

            db.Classifiers.Add(new Classifier
            {
                Id = model.ResourceTargetPersonTypeId,
                Code = resourceTargetPersonType,
                Type = ClassifierTypes.ResourceTargetPersonType,
                Value = string.Empty
            });

            SeedClassifiers(
                db,
                Guid.NewGuid(),
                model.ResourceSubTypeId,
                Guid.NewGuid(),
                model.SubmitterTypeId,
                contactTypeId
                );

            var personDataService = ServiceFactory.CreatePersonDataService();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                personDataService: personDataService,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await service.CreateAsync(model);

            var application = db.Applications.First();
            var resourceTargetPerson = db.Persons.First(t => t.PrivatePersonalIdentifier == model.ResourceTargetPerson.PrivatePersonalIdentifier);
            var submitterPerson = db.Persons.First(t => t.PrivatePersonalIdentifier == model.SubmitterPerson.PrivatePersonalIdentifier);
            var contact = db.PersonContacts.First();

            // Assert
            Assert.Equal(NumberingPatternHelper.ApplicationNumberFormat(), application.ApplicationNumber);
            Assert.Equal(educationalInstitutionId, application.EducationalInstitutionId);
            Assert.Equal(model.ResourceSubTypeId, application.ResourceSubTypeId);
            Assert.Equal(model.SubmitterTypeId, application.SubmitterTypeId);
            Assert.Equal(model.ResourceTargetPersonTypeId, application.ResourceTargetPersonTypeId);
            Assert.Equal(ApplicationStatus.Submitted, application.ApplicationStatus.Code);

            Assert.Equal(model.ResourceTargetPerson.FirstName, resourceTargetPerson.FirstName);
            Assert.Equal(model.ResourceTargetPerson.LastName, resourceTargetPerson.LastName);
            Assert.Equal(model.ResourceTargetPerson.LastName, resourceTargetPerson.LastName);
            //Assert.Equal(model.ResourceTargetPerson.PrivatePersonalIdentifier, resourceTargetPerson.PrivatePersonalIdentifier);

            Assert.Equal(model.SubmitterPerson.FirstName, submitterPerson.FirstName);
            Assert.Equal(model.SubmitterPerson.LastName, submitterPerson.LastName);
            //Assert.Equal(model.SubmitterPerson.PrivatePersonalIdentifier, submitterPerson.PrivatePersonalIdentifier);

            Assert.Equal(contactTypeId, contact.ContactTypeId);
            Assert.Equal(contactValue, contact.ContactValue);

            Assert.Null(personDataService.GetBirthDataCalledWith);

            Assert.True(gdprAuditService.TraceRangeAsyncCalledWith.Any());
        }

        [Fact]
        public async Task CreateAsync_CheckData_NewPerson_ResourceTargetPersonTypeLearner()
        {
            // Assign
            const int educationalInstitutionId = 1;
            const string contactValue = "someValue";
            const string submitterTypeCode = "someValue1";
            var contactTypeId = Guid.NewGuid();

            var model = new ApplicationCreateDto
            {
                EducationalInstitutionId = educationalInstitutionId,
                ResourceSubTypeId = Guid.NewGuid(),
                SubmitterTypeId = Guid.NewGuid(),
                ResourceTargetPersonTypeId = Guid.NewGuid(),
                ResourceTargetPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = "00000000000",
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = contactTypeId,
                            Value = contactValue
                        }
                    }
                },
                SubmitterPerson = new PersonData
                {
                    FirstName = "someFirstName1",
                    LastName = "someLastName1",
                    PrivatePersonalIdentifier = "00000000001"
                }
            };

            using var db = ServiceFactory.ConnectDb();

            SeedEducationalInstitution(db);

            db.Classifiers.Add(new Classifier
            {
                Id = model.ResourceTargetPersonTypeId,
                Code = "learner",
                Type = ClassifierTypes.ResourceTargetPersonType,
                Value = string.Empty
            });

            db.Classifiers.Add(new Classifier
            {
                Id = model.SubmitterTypeId,
                Code = submitterTypeCode,
                Type = ClassifierTypes.ApplicantRole,
                Value = string.Empty
            });

            SeedClassifiers(
                db,
                Guid.NewGuid(),
                model.ResourceSubTypeId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                contactTypeId
                );

            var personDataService = ServiceFactory.CreatePersonDataService();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                personDataService: personDataService,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await service.CreateAsync(model);

            var application = db.Applications.First();
            var resourceTargetPerson = db.Persons.First(t => t.PrivatePersonalIdentifier == model.ResourceTargetPerson.PrivatePersonalIdentifier);
            var submitterPerson = db.Persons.First(t => t.PrivatePersonalIdentifier == model.SubmitterPerson.PrivatePersonalIdentifier);
            var contact = db.PersonContacts.First();

            // Assert
            Assert.Equal(NumberingPatternHelper.ApplicationNumberFormat(), application.ApplicationNumber);
            Assert.Equal(educationalInstitutionId, application.EducationalInstitutionId);
            Assert.Equal(model.ResourceSubTypeId, application.ResourceSubTypeId);
            Assert.Equal(model.SubmitterTypeId, application.SubmitterTypeId);
            Assert.Equal(model.ResourceTargetPersonTypeId, application.ResourceTargetPersonTypeId);
            Assert.Equal(ApplicationStatus.Submitted, application.ApplicationStatus.Code);

            Assert.Equal(model.ResourceTargetPerson.FirstName, resourceTargetPerson.FirstName);
            Assert.Equal(model.ResourceTargetPerson.LastName, resourceTargetPerson.LastName);
            Assert.Equal(model.ResourceTargetPerson.LastName, resourceTargetPerson.LastName);
            Assert.Equal(model.ResourceTargetPerson.PrivatePersonalIdentifier, resourceTargetPerson.PrivatePersonalIdentifier);

            Assert.Equal(model.SubmitterPerson.FirstName, submitterPerson.FirstName);
            Assert.Equal(model.SubmitterPerson.LastName, submitterPerson.LastName);
            Assert.Equal(model.SubmitterPerson.PrivatePersonalIdentifier, submitterPerson.PrivatePersonalIdentifier);

            Assert.Equal(contactTypeId, contact.ContactTypeId);
            Assert.Equal(contactValue, contact.ContactValue);

            Assert.Equal(model.ResourceTargetPerson.PrivatePersonalIdentifier, personDataService.GetBirthDataCalledWith.TargetPrivatePersonalIdentifier);
            Assert.Equal(model.SubmitterPerson.PrivatePersonalIdentifier, personDataService.GetBirthDataCalledWith.SubmitterPersonalIdentifier);
            Assert.Equal(submitterTypeCode, personDataService.GetBirthDataCalledWith.Type);

            Assert.True(gdprAuditService.TraceRangeAsyncCalledWith.Any());
        }

        //[Fact]
        //public async Task DeleteAsync_Throws_EntityNotFound()
        //{
        //    // Assing
        //    using var db = ServiceFactory.ConnectDb();

        //    var service = GetService(db);

        //    // Act & Assert
        //    await Assert.ThrowsAsync<EntityNotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
        //}

        [Fact]
        public void Get_NoData()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act
            var result = service.Get();

            // Assert
            Assert.False(result.Any());
        }

        [Fact]
        public async Task Get_ReturnsData()
        {
            // Assign
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            using var db = ServiceFactory.ConnectDb();

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = applicationStatusId,
                Type = ClassifierTypes.ApplicationStatus,
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
                Id = resourceTargetPersonTypeId,
                Type = ClassifierTypes.ResourceTargetPersonType,
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

            await db.SaveChangesAsync();

            await db.Applications.AddAsync(new Domain.Entities.Application(applicationStatusId)
            {
                ApplicationNumber = NumberingPatternHelper.ApplicationNumberFormat(),
                ApplicationDate = DateTime.UtcNow,
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonId = Guid.NewGuid()
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var result = service.Get();

            // Assert
            Assert.True(result.Any());
        }

        [Theory]
        [InlineData(UserProfileType.Country, 3)]
        [InlineData(UserProfileType.Supervisor, 2)]
        [InlineData(UserProfileType.EducationalInstitution, 1)]
        public async Task Get_Authorized_ReturnsData(UserProfileType currentUserProfileType, int resultCount)
        {
            // Assign
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var statusClassifierId = Guid.NewGuid();
            const int educationalInstitutionId = 1;
            const int supervisorId = 1;

            using var db = ServiceFactory.ConnectDb();

            await db.Classifiers.AddRangeAsync(
                new Classifier
                {
                    Id = statusClassifierId,
                    Code = "c",
                    Value = "v",
                    Type = ClassifierTypes.EducationalInstitutionStatus
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
                    Id = resourceSubTypeId,
                    Type = ClassifierTypes.ResourceSubType,
                    Code = string.Empty,
                    Value = string.Empty
                });

            await db.SaveChangesAsync();

            await db.Applications.AddRangeAsync(
                new Domain.Entities.Application(applicationStatusId)
                {
                    ApplicationNumber = NumberingPatternHelper.ApplicationNumberFormat(),
                    ApplicationDate = DateTime.UtcNow,
                    EducationalInstitutionId = educationalInstitutionId,
                    ResourceSubTypeId = resourceSubTypeId,
                    SubmitterTypeId = submitterTypeId,
                    ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                    ResourceTargetPersonId = Guid.NewGuid()
                },
                new Domain.Entities.Application(applicationStatusId)
                {
                    ApplicationNumber = NumberingPatternHelper.ApplicationNumberFormat(),
                    ApplicationDate = DateTime.UtcNow,
                    EducationalInstitutionId = 2,
                    ResourceSubTypeId = resourceSubTypeId,
                    SubmitterTypeId = submitterTypeId,
                    ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                    ResourceTargetPersonId = Guid.NewGuid()
                },
                new Domain.Entities.Application(applicationStatusId)
                {
                    ApplicationNumber = NumberingPatternHelper.ApplicationNumberFormat(),
                    ApplicationDate = DateTime.UtcNow,
                    EducationalInstitutionId = 3,
                    ResourceSubTypeId = resourceSubTypeId,
                    SubmitterTypeId = submitterTypeId,
                    ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                    ResourceTargetPersonId = Guid.NewGuid()
                });

            await db.Supervisors.AddAsync(new Supervisor
            {
                Id = supervisorId,
                Code = "c",
                Name = "n"
            });

            await db.EducationalInstitutions.AddRangeAsync(
                new EducationalInstitution
                {
                    Id = educationalInstitutionId,
                    Code = "c",
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = supervisorId
                },
                new EducationalInstitution
                {
                    Id = 2,
                    Code = "c",
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = supervisorId
                });

            await db.SaveChangesAsync();

            var currentUserProfile = new CurrentUserProfileServiceFake();
            currentUserProfile.Type = currentUserProfileType;

            if (currentUserProfileType == UserProfileType.EducationalInstitution)
                currentUserProfile.EducationalInstitutionId = educationalInstitutionId;

            if (currentUserProfileType == UserProfileType.Supervisor)
                currentUserProfile.SupervisorId = supervisorId;

            var service = GetService(
                db: db,
                currentUserProfile: currentUserProfile);

            // Act
            var result = service.Get();

            // Assert
            Assert.Equal(resultCount, result.Count());
        }

        [Theory]
        [InlineData(true, true, ApplicationStatus.Submitted, PnaStatus.Prepared, 1)]
        [InlineData(true, true, ApplicationStatus.Postponed, PnaStatus.Prepared, 1)]
        [InlineData(true, true, ApplicationStatus.Confirmed, PnaStatus.Preparing, 1)]
        [InlineData(true, true, ApplicationStatus.Confirmed, PnaStatus.Prepared, 1)]
        [InlineData(true, true, ApplicationStatus.Confirmed, PnaStatus.Issued, 1)]
        [InlineData(true, true, ApplicationStatus.Declined, PnaStatus.Lost, 0)]
        [InlineData(true, true, ApplicationStatus.Withdrawn, PnaStatus.Lost, 0)]
        [InlineData(true, true, ApplicationStatus.Deleted, PnaStatus.Lost, 0)]
        [InlineData(true, true, ApplicationStatus.Confirmed, PnaStatus.Returned, 0)]
        [InlineData(true, true, ApplicationStatus.Confirmed, PnaStatus.Stolen, 0)]
        [InlineData(true, true, ApplicationStatus.Confirmed, PnaStatus.Lost, 0)]
        [InlineData(true, true, ApplicationStatus.Confirmed, PnaStatus.Cancelled, 0)]
        [InlineData(true, false, ApplicationStatus.Submitted, PnaStatus.Prepared, 0)]
        [InlineData(false, true, ApplicationStatus.Submitted, PnaStatus.Prepared, 0)]
        public async Task CheckApplicationsDuplicateAsync_Succeeds(
            bool equalPersIdent,
            bool equalResourceSubType,
            string statusCode,
            string pnaStatusCode,
            int expectedCount)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();
            const string privatePersonalIdentifier = "ppi";
            var resourceSubTypeId = Guid.NewGuid();
            var resourceSubTypeId2 = Guid.NewGuid();
            var personTechnicalsId = Guid.NewGuid();

            if (equalResourceSubType)
                resourceSubTypeId = resourceSubTypeId2;

            var dto = new ApplicationCheckDuplicateDto
            {
                PrivatePersonalIdentifier = privatePersonalIdentifier,
                ResourceSubTypeId = resourceSubTypeId
            };

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
                PrivatePersonalIdentifier = equalPersIdent ? privatePersonalIdentifier : privatePersonalIdentifier + "1"
            });

            if (resourceSubTypeId != resourceSubTypeId2)
            {
                db.Classifiers.Add(new Classifier
                {
                    Id = resourceSubTypeId,
                    Type = ClassifierTypes.ResourceSubType,
                    Code = string.Empty,
                    Value = string.Empty
                });
            }

            await db.SaveChangesAsync();

            CreateApplication(db, Guid.NewGuid(), personTechnicalsId, resourceSubTypeId2, statusCode, statusCode, pnaStatusCode);

            var service = GetService(db);

            // Act
            var result = service.GetApplicationDuplicates(dto);

            // Assert
            Assert.Equal(expectedCount, result.Count());
        }

        [Fact]
        public async Task UpdateAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var dto = new ApplicationUpdateDto
            {
                ApplicationStatusId = applicationStatusId,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonId = Guid.NewGuid(),
            };

            using var db = ServiceFactory.ConnectDb();

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = applicationStatusId,
                Type = ClassifierTypes.ApplicationStatus,
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
                Id = resourceTargetPersonTypeId,
                Type = ClassifierTypes.ResourceTargetPersonType,
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

            await db.SaveChangesAsync();

            await db.Applications.AddAsync(new Domain.Entities.Application(applicationStatusId)
            {
                Id = id,
                ApplicationNumber = NumberingPatternHelper.ApplicationNumberFormat(),
                ApplicationDate = DateTime.UtcNow,
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonId = Guid.NewGuid()
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.UpdateAsync(id, dto);

            var application = db.Applications.First();

            // Assert
            Assert.Equal(dto.ResourceTargetPersonId, application.ResourceTargetPersonId);
        }

        [Fact]
        public async Task UpdateAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.UpdateAsync(Guid.NewGuid(), new ApplicationUpdateDto()));
        }

        [Fact]
        public async Task DeclineAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();
            const string reason = "reason";

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, id, Guid.NewGuid(), Guid.NewGuid(), ApplicationStatus.Declined, ApplicationStatus.Submitted);

            var dto = new ApplicationDeclineDto
            {
                ApplicationIds = new[] { id },
                Reason = reason
            };

            var validator = ServiceFactory.CreateApplicationValidator();
            var service = GetService(db, validator);

            // Act
            await service.DeclineAsync(dto);

            var application = db.Applications.First();

            // Assert
            Assert.Equal(ApplicationStatus.Declined, application.ApplicationStatus.Code);
            Assert.Equal(ApplicationStatus.Submitted + ApplicationStatus.Declined, validator.ValidateCalledWith);
            Assert.Equal(reason, application.DeclineReason);
        }

        [Fact]
        public async Task DeclineAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                service.DeclineAsync(new ApplicationDeclineDto { ApplicationIds = new[] { Guid.NewGuid() } }));
        }

        [Fact]
        public async Task DeclineAsync_DoesntUpdateOnError()
        {
            // Assign
            var id = Guid.NewGuid();
            const string reason = "reason";

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, id, Guid.NewGuid(), Guid.NewGuid(), ApplicationStatus.Declined, ApplicationStatus.Submitted);

            var dto = new ApplicationDeclineDto
            {
                ApplicationIds = new[] { id },
                Reason = reason
            };

            var validator = ServiceFactory.CreateApplicationValidator();
            validator.ValidateThrowsException = true;

            var service = GetService(db, validator);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => service.DeclineAsync(dto));

            var application = db.Applications.First();

            // Assert
            Assert.Equal(ApplicationStatus.Submitted, application.ApplicationStatus.Code);
            Assert.Equal(ApplicationStatus.Submitted + ApplicationStatus.Declined, validator.ValidateCalledWith);
        }

        [Fact]
        public async Task DeleteAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, id, Guid.NewGuid(), Guid.NewGuid(), ApplicationStatus.Deleted, ApplicationStatus.Submitted);

            var validator = ServiceFactory.CreateApplicationValidator();
            var service = GetService(db, validator);

            // Act
            await service.DeleteAsync(new[] { id });

            var application = db.Applications.First();

            // Assert
            Assert.Equal(ApplicationStatus.Deleted, application.ApplicationStatus.Code);
            Assert.Equal(ApplicationStatus.Submitted + ApplicationStatus.Deleted, validator.ValidateCalledWith);
        }

        [Fact]
        public async Task DeleteAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                service.DeleteAsync(new[] { Guid.NewGuid() }));
        }

        [Fact]
        public async Task DeleteAsync_DoesntUpdateOnError()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, id, Guid.NewGuid(), Guid.NewGuid(), ApplicationStatus.Deleted, ApplicationStatus.Submitted);

            var validator = ServiceFactory.CreateApplicationValidator();
            validator.ValidateThrowsException = true;

            var service = GetService(db, validator);

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exceptions.ValidationException>(() => service.DeleteAsync(new[] { id }));

            var application = db.Applications.First();

            // Assert
            Assert.Equal(ApplicationStatus.Submitted, application.ApplicationStatus.Code);
            Assert.Equal(ApplicationStatus.Submitted + ApplicationStatus.Deleted, validator.ValidateCalledWith);
        }

        [Fact]
        public async Task PostponeAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, id, Guid.NewGuid(), Guid.NewGuid(), ApplicationStatus.Postponed, ApplicationStatus.Submitted);

            var validator = ServiceFactory.CreateApplicationValidator();
            var service = GetService(db, validator);

            // Act
            await service.PostponeAsync(id);

            var application = db.Applications.First();

            // Assert
            Assert.Equal(ApplicationStatus.Postponed, application.ApplicationStatus.Code);
            Assert.Equal(ApplicationStatus.Submitted + ApplicationStatus.Postponed, validator.ValidateCalledWith);
        }

        [Fact]
        public async Task PostponeAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                service.PostponeAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task PostponeAsync_DoesntUpdateOnError()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, id, Guid.NewGuid(), Guid.NewGuid(), ApplicationStatus.Postponed, ApplicationStatus.Submitted);

            var validator = ServiceFactory.CreateApplicationValidator();
            validator.ValidateThrowsException = true;

            var service = GetService(db, validator);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => service.PostponeAsync(id));

            var application = db.Applications.First();

            // Assert
            Assert.Equal(ApplicationStatus.Submitted, application.ApplicationStatus.Code);
            Assert.Equal(ApplicationStatus.Submitted + ApplicationStatus.Postponed, validator.ValidateCalledWith);
        }

        [Fact]
        public async Task WithdrawAsync_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, id, Guid.NewGuid(), Guid.NewGuid(), ApplicationStatus.Withdrawn, ApplicationStatus.Submitted);

            var validator = ServiceFactory.CreateApplicationValidator();
            var service = GetService(db, validator);

            // Act
            await service.WithdrawAsync(id);

            var application = db.Applications.First();

            // Assert
            Assert.Equal(ApplicationStatus.Withdrawn, application.ApplicationStatus.Code);
            Assert.Equal(ApplicationStatus.Submitted + ApplicationStatus.Withdrawn, validator.ValidateCalledWith);
        }

        [Fact]
        public async Task WithdrawAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                service.WithdrawAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task WithdrawAsync_DoesntUpdateOnError()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            CreateApplication(db, id, Guid.NewGuid(), Guid.NewGuid(), ApplicationStatus.Withdrawn, ApplicationStatus.Submitted);

            var validator = ServiceFactory.CreateApplicationValidator();
            validator.ValidateThrowsException = true;

            var service = GetService(db, validator);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => service.WithdrawAsync(id));

            var application = db.Applications.First();

            // Assert
            Assert.Equal(ApplicationStatus.Submitted, application.ApplicationStatus.Code);
            Assert.Equal(ApplicationStatus.Submitted + ApplicationStatus.Withdrawn, validator.ValidateCalledWith);
        }

        [Fact]
        public async Task ChangeSubmitterContactAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.ChangeSubmitterContactAsync(Guid.NewGuid(), new ApplicationContactInformationUpdateDto()));
        }

        [Fact]
        public async Task ChangeSubmitterContactAsync_UpdateContactPerson_Succeeds()
        {
            // Assign
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var initialPersonId = Guid.NewGuid();
            var updatedPersonId = Guid.NewGuid();
            var contactTypeId = Guid.NewGuid();
            const string privatePersonlIdentifier = "00000000001";
            const string contactValue = "someValue";

            using var db = ServiceFactory.ConnectDb();

            SeedClassifiers(db, applicationStatusId, resourceSubTypeId, resourceTargetPersonTypeId, submitterTypeId, contactTypeId);

            SeedEducationalInstitution(db);

            await db.Persons.AddAsync(new Person
            {
                FirstName = "someName",
                LastName = "someName",
                PrivatePersonalIdentifier = privatePersonlIdentifier,
                PersonTechnical = new PersonTechnical
                {
                    Id = initialPersonId
                }
            });

            await db.Persons.AddAsync(new Person
            {
                FirstName = "someName",
                LastName = "someName",
                PrivatePersonalIdentifier = privatePersonlIdentifier,
                PersonTechnical = new PersonTechnical
                {
                    Id = updatedPersonId
                }
            });

            await db.SaveChangesAsync();

            var dto = new ApplicationCreateDto
            {
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = privatePersonlIdentifier
                },
                SubmitterPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = privatePersonlIdentifier,
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = contactTypeId,
                            Value = contactValue
                        }
                    }
                }
            };

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            var result = await service.CreateAsync(dto);

            var request = new ApplicationContactInformationUpdateDto
            {
                Person = new PersonData
                {
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = db.Classifiers.First(t => t.Type == ClassifierTypes.ContactType).Id,
                            Value = string.Empty
                        },
                        new PersonData.ContactData
                        {
                            TypeId = db.Classifiers.First(t => t.Type == ClassifierTypes.ContactType).Id,
                            Value = string.Empty
                        }
                    },
                    PrivatePersonalIdentifier = privatePersonlIdentifier,
                    FirstName = db.Persons.First().FirstName,
                    LastName = db.Persons.First().LastName,
                }
            };

            // Act
            await service.ChangeSubmitterContactAsync(result.Id, request);

            var application = db.Applications.First();
            var contact = application.ContactPerson.PersonContacts.First();
            var contactRequest = request.Person.ContactInformation.First();

            // Assert
            Assert.Equal(result.Id, application.Id);
            Assert.Equal(updatedPersonId, application.ContactPersonId);
            Assert.NotEqual(contact.ContactValue, contactRequest.Value);
            Assert.Equal(contact.ContactType.Id, contactRequest.TypeId);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task ChangeSubmitterContactAsync_CreateContactPerson_Succeeds()
        {
            // Assign
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var initialPersonId = Guid.NewGuid();
            var contactTypeId = Guid.NewGuid();
            const string privatePersonlIdentifier = "00000000001";
            const string contactValue = "someValue";

            using var db = ServiceFactory.ConnectDb();

            SeedClassifiers(db, applicationStatusId, resourceSubTypeId, resourceTargetPersonTypeId, submitterTypeId, contactTypeId);

            SeedEducationalInstitution(db);

            await db.Persons.AddAsync(new Person
            {
                FirstName = "someName",
                LastName = "someName",
                PrivatePersonalIdentifier = privatePersonlIdentifier,
                PersonTechnical = new PersonTechnical
                {
                    Id = initialPersonId
                }
            });

            await db.SaveChangesAsync();

            var dto = new ApplicationCreateDto
            {
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = privatePersonlIdentifier
                },
                SubmitterPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = privatePersonlIdentifier,
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = contactTypeId,
                            Value = contactValue
                        }
                    }
                }
            };

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            var result = await service.CreateAsync(dto);

            var request = new ApplicationContactInformationUpdateDto
            {
                Person = new PersonData
                {
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = db.Classifiers.First(t => t.Type == ClassifierTypes.ContactType).Id,
                            Value = string.Empty
                        },
                        new PersonData.ContactData
                        {
                            TypeId = db.Classifiers.First(t => t.Type == ClassifierTypes.ContactType).Id,
                            Value = string.Empty
                        }
                    },
                    PrivatePersonalIdentifier = privatePersonlIdentifier,
                    FirstName = db.Persons.First().FirstName,
                    LastName = db.Persons.First().LastName,
                }
            };

            // Act
            await service.ChangeSubmitterContactAsync(result.Id, request);

            var application = db.Applications.First();
            var contact = application.ContactPerson.PersonContacts.First();
            var contactRequest = request.Person.ContactInformation.First();
            var newPerson = db.Persons
                .First(t => t.PrivatePersonalIdentifier == privatePersonlIdentifier);

            // Assert
            Assert.Equal(result.Id, application.Id);
            Assert.Equal(newPerson.PersonTechnicalId, application.ContactPersonId);
            Assert.Equal(initialPersonId, application.ContactPersonId);
            Assert.NotEqual(contact.ContactValue, contactRequest.Value);
            Assert.Equal(contact.ContactType.Id, contactRequest.TypeId);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task ChangeSubmittersContactAsync_UpdateContactPerson_Succeeds()
        {
            // Assign
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var initialPersonId = Guid.NewGuid();
            var updatedPersonId = Guid.NewGuid();
            var contactTypeId = Guid.NewGuid();
            const string privatePersonlIdentifier = "00000000001";
            const string contactValue = "someValue";

            using var db = ServiceFactory.ConnectDb();

            SeedClassifiers(db, applicationStatusId, resourceSubTypeId, resourceTargetPersonTypeId, submitterTypeId, contactTypeId);

            SeedEducationalInstitution(db);

            await db.Persons.AddAsync(new Person
            {
                FirstName = "someName",
                LastName = "someName",
                PrivatePersonalIdentifier = privatePersonlIdentifier,
                PersonTechnical = new PersonTechnical
                {
                    Id = initialPersonId
                }
            });

            await db.Persons.AddAsync(new Person
            {
                FirstName = "someName",
                LastName = "someName",
                PrivatePersonalIdentifier = privatePersonlIdentifier,
                PersonTechnical = new PersonTechnical
                {
                    Id = updatedPersonId
                }
            });

            await db.SaveChangesAsync();

            var dto = new ApplicationCreateDto
            {
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = privatePersonlIdentifier
                },
                SubmitterPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = privatePersonlIdentifier,
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = contactTypeId,
                            Value = contactValue
                        }
                    }
                }
            };

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            var result = await service.CreateAsync(dto);

            var request = new ApplicationsContactInformationUpdateDto
            {
                Person = new PersonData
                {
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = db.Classifiers.First(t => t.Type == ClassifierTypes.ContactType).Id,
                            Value = string.Empty
                        },
                        new PersonData.ContactData
                        {
                            TypeId = db.Classifiers.First(t => t.Type == ClassifierTypes.ContactType).Id,
                            Value = string.Empty
                        }
                    },
                    PrivatePersonalIdentifier = privatePersonlIdentifier,
                    FirstName = db.Persons.First().FirstName,
                    LastName = db.Persons.First().LastName,
                }
            };

            // Act
            await service.ChangeSubmittersContactAsync(new[] { result.Id }, request);

            var application = db.Applications.First();
            var contact = application.ContactPerson.PersonContacts.First();
            var contactRequest = request.Person.ContactInformation.First();

            // Assert
            Assert.Equal(result.Id, application.Id);
            Assert.Equal(updatedPersonId, application.ContactPersonId);
            Assert.NotEqual(contact.ContactValue, contactRequest.Value);
            Assert.Equal(contact.ContactType.Id, contactRequest.TypeId);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task ChangeSubmittersContactAsync_CreateContactPerson_Succeeds()
        {
            // Assign
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var initialPersonId = Guid.NewGuid();
            var contactTypeId = Guid.NewGuid();
            const string privatePersonlIdentifier = "00000000001";
            const string contactValue = "someValue";

            using var db = ServiceFactory.ConnectDb();

            SeedClassifiers(db, applicationStatusId, resourceSubTypeId, resourceTargetPersonTypeId, submitterTypeId, contactTypeId);

            SeedEducationalInstitution(db);

            await db.Persons.AddAsync(new Person
            {
                FirstName = "someName",
                LastName = "someName",
                PrivatePersonalIdentifier = privatePersonlIdentifier,
                PersonTechnical = new PersonTechnical
                {
                    Id = initialPersonId
                }
            });

            await db.SaveChangesAsync();

            var dto = new ApplicationCreateDto
            {
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = privatePersonlIdentifier
                },
                SubmitterPerson = new PersonData
                {
                    FirstName = "someFirstName",
                    LastName = "someLastName",
                    PrivatePersonalIdentifier = privatePersonlIdentifier,
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = contactTypeId,
                            Value = contactValue
                        }
                    }
                }
            };

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            var result = await service.CreateAsync(dto);

            var request = new ApplicationsContactInformationUpdateDto
            {
                Person = new PersonData
                {
                    ContactInformation = new List<PersonData.ContactData>
                    {
                        new PersonData.ContactData
                        {
                            TypeId = db.Classifiers.First(t => t.Type == ClassifierTypes.ContactType).Id,
                            Value = string.Empty
                        },
                        new PersonData.ContactData
                        {
                            TypeId = db.Classifiers.First(t => t.Type == ClassifierTypes.ContactType).Id,
                            Value = string.Empty
                        }
                    },
                    PrivatePersonalIdentifier = privatePersonlIdentifier,
                    FirstName = db.Persons.First().FirstName,
                    LastName = db.Persons.First().LastName,
                }
            };

            // Act
            await service.ChangeSubmittersContactAsync(new[] { result.Id }, request);

            var application = db.Applications.First();
            var contact = application.ContactPerson.PersonContacts.First();
            var contactRequest = request.Person.ContactInformation.First();
            var newPerson = db.Persons
                .First(t => t.PrivatePersonalIdentifier == privatePersonlIdentifier);

            // Assert
            Assert.Equal(result.Id, application.Id);
            Assert.Equal(newPerson.PersonTechnicalId, application.ContactPersonId);
            Assert.Equal(initialPersonId, application.ContactPersonId);
            Assert.NotEqual(contact.ContactValue, contactRequest.Value);
            Assert.Equal(contact.ContactType.Id, contactRequest.TypeId);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        private ApplicationService GetService(
            IAppDbContext db,
            IApplicationValidator validator = null,
            ICurrentUserProfileService currentUserProfile = null,
            IAuthorizationService authorizationService = null,
            IApplicationSocialStatusCheckService applicationSocialStatusCheckService = null,
            IPersonDataService personDataService = null,
            ISequenceService sequenceService = null,
            IGdprAuditService gdprAuditService = null,
            IApplicationDuplicateService applicationDuplicateService = null)
        {
            return new ApplicationService(
                db: db,
                validator: validator ?? ServiceFactory.CreateApplicationValidator(),
                currentUserProfile: currentUserProfile ?? ServiceFactory.CreateCurrentUserProfileService(),
                authorizationService: authorizationService ?? ServiceFactory.CreateAuthorizationService(),
                applicationSocialStatusCheckService: applicationSocialStatusCheckService ?? ServiceFactory.CreateApplicationSocialStatusCheckService(),
                sequenceService: sequenceService ?? ServiceFactory.CreateSequenceService(),
                personDataService: personDataService ?? ServiceFactory.CreatePersonDataService(),
                gdprAuditService: gdprAuditService ?? ServiceFactory.CreateGdprAuditService(),
                applicationDuplicateService: applicationDuplicateService ?? ServiceFactory.CreateApplicationDuplicateService()
                );
        }

        private void SeedClassifiers(DbContextFake db, Guid applicationStatusId, Guid resourceSubTypeId, Guid resourceTargetPersonTypeId, Guid submitterTypeId, Guid contactTypeId)
        {
            db.Classifiers.Add(new Classifier
            {
                Id = applicationStatusId,
                Type = ClassifierTypes.ApplicationStatus,
                Code = ApplicationStatus.Submitted,
                Value = string.Empty
            });

            db.Classifiers.Add(new Classifier
            {
                Id = submitterTypeId,
                Type = ClassifierTypes.ApplicantRole,
                Code = ApplicantRole.ParentGuardian,
                Value = string.Empty
            });

            db.Classifiers.Add(new Classifier
            {
                Id = resourceTargetPersonTypeId,
                Type = ClassifierTypes.ResourceTargetPersonType,
                Code = string.Empty,
                Value = string.Empty
            });

            db.Classifiers.Add(new Classifier
            {
                Id = resourceSubTypeId,
                Type = ClassifierTypes.ResourceSubType,
                Code = string.Empty,
                Value = string.Empty
            });

            db.Classifiers.Add(new Classifier
            {
                Id = contactTypeId,
                Type = ClassifierTypes.ContactType,
                Code = string.Empty,
                Value = string.Empty
            });

            db.SaveChanges();
        }

        private void SeedEducationalInstitution(DbContextFake db)
        {
            var educationalInstitutionStatusId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = educationalInstitutionStatusId,
                Type = ClassifierTypes.EducationalInstitutionStatus,
                Code = EducationalInstitutionStatus.Active,
                Value = string.Empty
            });

            db.SaveChanges();

            db.EducationalInstitutions.Add(new EducationalInstitution
            {
                Id = 1,
                Code = NumberingPatternHelper.DefaultInstitution,
                Name = string.Empty,
                StatusId = educationalInstitutionStatusId
            });

            db.SaveChanges();
        }

        private void CreateApplication(
            IAppDbContext db,
            Guid id,
            Guid resourceTargetPersonId,
            Guid resourceSubTypeId,
            string statusChangeCode = ApplicationStatus.Submitted,
            string applicationStatusCode = ApplicationStatus.Confirmed,
            string pnaStatusCode = PnaStatus.Lost)
        {
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var pnaStatusId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();

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
                    Id = Guid.NewGuid(),
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
                },
                new Classifier
                {
                    Id = pnaStatusId,
                    Type = ClassifierTypes.PnaStatus,
                    Code = pnaStatusCode,
                    Value = string.Empty
                });


            db.SaveChanges();

            db.Applications.Add(new Domain.Entities.Application(applicationStatusId)
            {
                Id = id,
                ApplicationNumber = NumberingPatternHelper.ApplicationNumberFormat(),
                ApplicationDate = DateTime.UtcNow,
                EducationalInstitutionId = 1,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonId = resourceTargetPersonId
            });

            var resource = new ApplicationResource
            {
                ApplicationId = id,
                Id = applicationResourceId,
                PNANumber = string.Empty
            };

            resource.SetPnaStatus(pnaStatusId);

            db.ApplicationResources.Add(resource);

            db.SaveChanges();
        }
    }
}
