using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Helpers;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class ApplicationDuplicateServiceTests
    {
        [Theory]
        [InlineData(ApplicationStatus.Submitted, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Postponed, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Preparing)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Prepared)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Issued)]

        public async Task CheckApplicationsDuplicatesAsync_FoundDuplicate(string statusCode, string pnaStatusCode)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId1 = 1;
            const int educationalInstitutionId2 = 2;

            var pnaStatusId = Guid.NewGuid();
            var statusClassifierId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();

            var applicationId1 = Guid.NewGuid();
            var applicationId2 = Guid.NewGuid();

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
                    Code = pnaStatusCode,
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

            db.EducationalInstitutions.AddRange(
                new EducationalInstitution
                {
                    Id = educationalInstitutionId1,
                    Code = "c",
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = 1
                },
                new EducationalInstitution
                {
                    Id = educationalInstitutionId2,
                    Code = "c",
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = 1
                });

            db.SaveChanges();

            CreateApplication(
                db: db,
                id: applicationId1,
                resourceSubTypeId: resourceSubTypeId,
                resourceTargetPersonId: resourceTargetPersonId,
                statusCode: ApplicationStatus.Submitted,
                eduInstId: educationalInstitutionId1
                );

            CreateApplication(
                db: db, 
                id: applicationId2,
                resourceSubTypeId: resourceSubTypeId,
                resourceTargetPersonId: resourceTargetPersonId, 
                statusCode: statusCode, 
                eduInstId: educationalInstitutionId2
                );

            var applicationResource = new ApplicationResource
            {
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                ApplicationId = applicationId2
            };

            applicationResource.SetPnaStatus(pnaStatusId);

            db.ApplicationResources.Add(applicationResource);

            db.SaveChanges();

            var service = GetService(db);

            // Act
            await service.CheckApplicationsDuplicatesAsync(new[] { applicationId2 });

            var application1 = db.Applications.Find(new object[] { applicationId1 });
            var application2 = db.Applications.Find(new object[] { applicationId2 });

            // Assert
            Assert.Equal(applicationId2, application1.ApplicationDuplicateId);
            Assert.Equal(applicationId1, application2.ApplicationDuplicateId);
        }

        [Theory]
        [InlineData(ApplicationStatus.Declined, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Withdrawn, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Deleted, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Returned)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Stolen)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Cancelled)]
        public async Task CheckApplicationsDuplicatesAsync_Skipped(string statusCode, string pnaStatusCode)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId1 = 1;
            const int educationalInstitutionId2 = 2;

            var pnaStatusId = Guid.NewGuid();
            var statusClassifierId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();

            var applicationId1 = Guid.NewGuid();
            var applicationId2 = Guid.NewGuid();

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
                    Code = pnaStatusCode,
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

            db.EducationalInstitutions.AddRange(
                new EducationalInstitution
                {
                    Id = educationalInstitutionId1,
                    Code = "c",
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = 1
                },
                new EducationalInstitution
                {
                    Id = educationalInstitutionId2,
                    Code = "c",
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = 1
                });

            db.SaveChanges();

            CreateApplication(
                db: db,
                id: applicationId1,
                resourceSubTypeId: resourceSubTypeId,
                resourceTargetPersonId: resourceTargetPersonId,
                statusCode: ApplicationStatus.Submitted,
                eduInstId: educationalInstitutionId1
                );

            CreateApplication(
                db: db,
                id: applicationId2,
                resourceSubTypeId: resourceSubTypeId,
                resourceTargetPersonId: resourceTargetPersonId,
                statusCode: statusCode,
                eduInstId: educationalInstitutionId2
                );

            var applicationResource = new ApplicationResource
            {
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                ApplicationId = applicationId2
            };

            applicationResource.SetPnaStatus(pnaStatusId);

            db.ApplicationResources.Add(applicationResource);

            db.SaveChanges();

            var service = GetService(db);

            // Act
            await service.CheckApplicationsDuplicatesAsync(new[] { applicationId2 });

            var application1 = db.Applications.Find(new object[] { applicationId1 });
            var application2 = db.Applications.Find(new object[] { applicationId2 });

            // Assert
            Assert.Null(application1.ApplicationDuplicateId);
            Assert.Null(application2.ApplicationDuplicateId);
        }

        [Theory]
        [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Declined, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Withdrawn, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Deleted, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Confirmed, PnaStatus.Returned)]
        [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Confirmed, PnaStatus.Stolen)]
        [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Confirmed, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Confirmed, PnaStatus.Cancelled)]
        [InlineData(ApplicationStatus.Declined, ApplicationStatus.Submitted, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Declined, ApplicationStatus.Postponed, PnaStatus.Lost)]
        [InlineData(ApplicationStatus.Declined, ApplicationStatus.Confirmed, PnaStatus.Preparing)]
        [InlineData(ApplicationStatus.Declined, ApplicationStatus.Confirmed, PnaStatus.Prepared)]
        [InlineData(ApplicationStatus.Declined, ApplicationStatus.Confirmed, PnaStatus.Issued)]
        public async Task CheckApplicationsDuplicatesAsync_RemovedDuplicate(string statusCode1, string statusCode2, string pnaStatusCode)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId1 = 1;
            const int educationalInstitutionId2 = 2;

            var pnaStatusId = Guid.NewGuid();
            var statusClassifierId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceTargetPersonId = Guid.NewGuid();

            var applicationId1 = Guid.NewGuid();
            var applicationId2 = Guid.NewGuid();

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
                    Code = pnaStatusCode,
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

            db.EducationalInstitutions.AddRange(
                new EducationalInstitution
                {
                    Id = educationalInstitutionId1,
                    Code = "c",
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = 1
                },
                new EducationalInstitution
                {
                    Id = educationalInstitutionId2,
                    Code = "c",
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = 1
                });

            db.SaveChanges();

            CreateApplication(
                db: db,
                id: applicationId1,
                resourceSubTypeId: resourceSubTypeId,
                resourceTargetPersonId: resourceTargetPersonId,
                statusCode: statusCode1,
                eduInstId: educationalInstitutionId1,
                duplicateId: applicationId2
                );

            CreateApplication(
                db: db,
                id: applicationId2,
                resourceSubTypeId: resourceSubTypeId,
                resourceTargetPersonId: resourceTargetPersonId,
                statusCode: statusCode2,
                eduInstId: educationalInstitutionId2,
                duplicateId: applicationId1
                );

            var applicationResource = new ApplicationResource
            {
                PNANumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                ApplicationId = applicationId2
            };

            applicationResource.SetPnaStatus(pnaStatusId);

            db.ApplicationResources.Add(applicationResource);

            db.SaveChanges();

            var service = GetService(db);

            // Act
            await service.CheckApplicationsDuplicatesAsync(new[] { applicationId2 });

            var application1 = db.Applications.Find(new object[] { applicationId1 });
            var application2 = db.Applications.Find(new object[] { applicationId2 });

            // Assert
            Assert.Null(application1.ApplicationDuplicateId);
            Assert.Null(application2.ApplicationDuplicateId);
        }

        private ApplicationDuplicateService GetService(
            IAppDbContext db,
            IGdprAuditService gdprAuditService = null
            )
        {
            return new ApplicationDuplicateService(
                db,
                gdprAuditService: gdprAuditService ?? ServiceFactory.CreateGdprAuditService()
                );
        }

        private void CreateApplication(
            DbContextFake db, 
            Guid id, 
            Guid resourceSubTypeId, 
            Guid resourceTargetPersonId, 
            string statusCode, 
            int eduInstId, 
            Guid? duplicateId = null
            )
        {
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();

            SeedEducationalInstitution(db, eduInstId);

            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = applicationStatusId,
                    Type = ClassifierTypes.ApplicationStatus,
                    Code = statusCode,
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
                });


            db.SaveChanges();

            db.Applications.Add(new Domain.Entities.Application(applicationStatusId)
            {
                Id = id,
                ApplicationNumber = NumberingPatternHelper.ApplicationResourcesNumberFormat(),
                ApplicationDate = DateTime.UtcNow,
                EducationalInstitutionId = eduInstId,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId,
                ResourceTargetPersonId = resourceTargetPersonId,
                ApplicationDuplicateId = duplicateId
            });

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
    }
}
