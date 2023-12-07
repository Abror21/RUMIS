using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class ApplicationValidatorTests
    {
        [Theory]
        [InlineData("00000000002", ApplicationStatus.Submitted, PnaStatus.Prepared)]
        [InlineData("00000000001", ApplicationStatus.Declined, PnaStatus.Prepared)]
        [InlineData("00000000001", ApplicationStatus.Withdrawn, PnaStatus.Prepared)]
        [InlineData("00000000001", ApplicationStatus.Deleted, PnaStatus.Prepared)]
        [InlineData("00000000001", ApplicationStatus.Confirmed, PnaStatus.Returned)]
        [InlineData("00000000001", ApplicationStatus.Confirmed, PnaStatus.Stolen)]
        [InlineData("00000000001", ApplicationStatus.Confirmed, PnaStatus.Lost)]
        [InlineData("00000000001", ApplicationStatus.Confirmed, PnaStatus.Cancelled)]
        public async Task ValidateAsync_Succeeds(string personalIdentifier, string applicationStatusCode, string pnaStatusCode)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();
            var personTechnicalsId = Guid.NewGuid();
            var model = CreateValidaCreateDto();

            db.Classifiers.Add(new Classifier
            {
                Id = model.ResourceSubTypeId,
                Type = ClassifierTypes.ResourceSubType,
                Code = string.Empty,
                Value = string.Empty
            });

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = personTechnicalsId,
                UserId = Guid.NewGuid()
            });

            db.Persons.Add(new Person
            {
                PersonTechnicalId = personTechnicalsId,
                FirstName = model.ResourceTargetPerson.FirstName,
                LastName = model.ResourceTargetPerson.LastName,
                PrivatePersonalIdentifier = personalIdentifier
            });

            CreateApplication(db, personTechnicalsId, model.ResourceSubTypeId, applicationStatusCode, pnaStatusCode);

            var validator = GetValidator(db);

            // Act
            await validator.ValidateAsync(model);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task ValidateAsync_Throws_ResourceTargetPersonRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var model = CreateValidaCreateDto();

            model.ResourceTargetPerson = null;

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(model));

            // Assert
            Assert.Equal(ApplicationValidator.Error.ResourceTargetPersonRequired, result.Message);
        }

        //[Fact]
        //public void Validate_Throws_ResourceTargetPersonInvalidPrivatePersonalIdentifier()
        //{
        //    // Assign
        //    var model = CreateValidaCreateDto();

        //    model.ResourceTargetPerson.PrivatePersonalIdentifier = "someIncorrectIdentifier";

        //    var validator = GetValidator();

        //    // Act & Assert
        //    var result = Assert.Throws<ValidationException>(() => validator.Validate(model));

        //    // Assert
        //    Assert.Equal(ApplicationValidator.Error.ResourceTargetPersonInvalidPrivatePersonalIdentifier, result.Message);
        //}

        [Fact]
        public async Task ValidateAsync_Throws_SubmitterPersonRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var model = CreateValidaCreateDto();

            model.SubmitterPerson = null;

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(model));

            // Assert
            Assert.Equal(ApplicationValidator.Error.SubmitterPersonRequired, result.Message);
        }

        //[Fact]
        //public void Validate_Throws_SubmitterPersonInvalidPrivatePersonalIdentifier()
        //{
        //    // Assign
        //    var model = CreateValidaCreateDto();

        //    model.SubmitterPerson.PrivatePersonalIdentifier = "someIncorrectIdentifier";

        //    var validator = GetValidator();

        //    // Act & Assert
        //    var result = Assert.Throws<ValidationException>(() => validator.Validate(model));

        //    // Assert
        //    Assert.Equal(ApplicationValidator.Error.SubmitterPersonInvalidPrivatePersonalIdentifier, result.Message);
        //}

        [Theory]
        [InlineData(ApplicationStatus.Submitted, PnaStatus.Prepared)]
        [InlineData(ApplicationStatus.Postponed, PnaStatus.Prepared)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Issued)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Prepared)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Preparing)]
        public async Task ValidateAsync_Throws_AlreadyExists(string applicationStatusCode, string pnaStatusCode)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();
            var personTechnicalsId = Guid.NewGuid();
            var model = CreateValidaCreateDto();
            model.EducationalInstitutionId = 1;

            db.Classifiers.Add(new Classifier
            {
                Id = model.ResourceSubTypeId,
                Type = ClassifierTypes.ResourceSubType,
                Code = string.Empty,
                Value = string.Empty
            });

            db.PersonTechnicals.Add(new PersonTechnical
            {
                Id = personTechnicalsId,
                UserId = Guid.NewGuid()
            });

            db.Persons.Add(new Person
            {
                PersonTechnicalId = personTechnicalsId,
                FirstName = model.ResourceTargetPerson.FirstName,
                LastName = model.ResourceTargetPerson.LastName,
                PrivatePersonalIdentifier = model.ResourceTargetPerson.PrivatePersonalIdentifier
            });

            CreateApplication(db, personTechnicalsId, model.ResourceSubTypeId, applicationStatusCode, pnaStatusCode);

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(model));

            // Assert
            Assert.Equal(ApplicationValidator.Error.AlreadyExists, result.Message);
        }

        public static IEnumerable<object[]> GenerateValidateSucceedsData()
        {
            var allStatuses = typeof(ApplicationStatus).GetFields().Select(t => (string)t.GetValue(null));

            var statuses = new List<string>
            {
                ApplicationStatus.Submitted,
                ApplicationStatus.Postponed
            };

            foreach (var entityStatus in statuses)
            {
                foreach (var itemStatus in allStatuses)
                {
                    if (entityStatus == ApplicationStatus.Postponed
                        && itemStatus == ApplicationStatus.Submitted)
                        continue;

                    if (entityStatus.ToString() != itemStatus.ToString())
                        yield return new object[] { entityStatus, itemStatus };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GenerateValidateSucceedsData))]
        public void Validate_Succeeds(string entityStatusCode, string itemStatusCode)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var validator = GetValidator(db);

            // Act
            validator.Validate(entityStatusCode, itemStatusCode);

            // Assert
            Assert.True(true);
        }

        public static IEnumerable<object[]> GenerateValidateStatusChangeForbiddenData()
        {
            var allStatuses = typeof(ApplicationStatus).GetFields().Select(t => t.GetValue(null));

            var statuses = new List<string>
            {
                ApplicationStatus.Confirmed,
                ApplicationStatus.Declined,
                ApplicationStatus.Withdrawn,
                ApplicationStatus.Deleted
            };

            foreach (var entityStatus in statuses)
            {
                foreach (var itemStatus in allStatuses)
                {
                    if (entityStatus.ToString() != itemStatus.ToString())
                        yield return new object[] { entityStatus, itemStatus };
                }
            }
        }

        [Theory]
        [InlineData(ApplicationStatus.Postponed, ApplicationStatus.Submitted)]
        [MemberData(nameof(GenerateValidateStatusChangeForbiddenData))]
        public void Validate_Throws_StatusChangeForbidden(string entityStatusCode, string itemStatusCode)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var validator = GetValidator(db);

            // Act & Assert
            var result = Assert.Throws<ValidationException>(() => validator.Validate(entityStatusCode, itemStatusCode));

            // Assert
            Assert.Equal(ApplicationValidator.Error.StatusChangeForbidden, result.Message);
        }


        private ApplicationCreateDto CreateValidaCreateDto()
        {
            return new ApplicationCreateDto
            {
                ResourceSubTypeId = Guid.NewGuid(),
                ResourceTargetPerson = new PersonData
                {
                    FirstName = "someName",
                    LastName = "someName",
                    PrivatePersonalIdentifier = "00000000001"
                },
                SubmitterPerson = new PersonData
                {
                    FirstName = "someName",
                    LastName = "someName",
                    PrivatePersonalIdentifier = "00000000001"
                },
                EducationalInstitutionId = 0,
            };
        }

        private ApplicationValidator GetValidator(IAppDbContext db)
        {
            return new ApplicationValidator(db);
        }

        private void CreateApplication(
            IAppDbContext db,
            Guid? resourceTargetPersonId = null,
            Guid? resourceSubTypeId = null,
            string applicationStatusCode = ApplicationStatus.Confirmed,
            string pnaStatusCode = PnaStatus.Lost)
        {
            var applicationStatusId = Guid.NewGuid();
            var PNAStatusId = Guid.NewGuid();
            var disability = "Invaliditāte";
            var id = Guid.NewGuid();
            var socialStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var applicationResourceId = Guid.NewGuid();
            var documentTypeId = Guid.NewGuid();

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
                    Id = PNAStatusId,
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
                ResourceTargetPersonId = resourceTargetPersonId ?? Guid.NewGuid(),
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
                Id = applicationResourceId,
                PNANumber = string.Empty
            };

            resource.SetPnaStatus(PNAStatusId);

            db.ApplicationResources.Add(resource);

            db.ApplicationResourceAttachments.Add(new ApplicationResourceAttachment
            {
                ApplicationResourceId = applicationResourceId,
                Id = Guid.NewGuid(),
                DocumentTypeId = documentTypeId,
            });

            db.SaveChanges();
        }
    }
}
