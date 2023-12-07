using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class ApplicationResourceValidatorTests
    {
        [Theory]
        [InlineData(ApplicationStatus.Submitted, PnaStatus.Preparing, false)]
        [InlineData(ApplicationStatus.Postponed, PnaStatus.Preparing, false)]
        [InlineData(ApplicationStatus.Submitted, PnaStatus.Preparing, true)]
        [InlineData(ApplicationStatus.Postponed, PnaStatus.Preparing, true)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Returned, true)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Stolen, true)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Lost, true)]
        [InlineData(ApplicationStatus.Confirmed, PnaStatus.Cancelled, true)]
        public async Task ValidateAsync_Succeeds(string applicationStatusCode, string pnaStatusCode, bool hasPermissions)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidCreateDto();

            CreateApplication(db, item.ApplicationId, applicationStatusCode);
            CreateResource(db, item.AssignedResourceId.Value);

            if (applicationStatusCode == ApplicationStatus.Confirmed)
            {
                var pnaStatusId = Guid.NewGuid();

                db.Classifiers.Add(new Classifier
                {
                    Id = pnaStatusId,
                    Type = ClassifierTypes.PnaStatus,
                    Code = pnaStatusCode,
                    Value = string.Empty
                });

                db.SaveChanges();

                var resource = new ApplicationResource
                {
                    ApplicationId = item.ApplicationId,
                    PNANumber = "PNANumber",
                    AssignedResourceId = item.AssignedResourceId.Value
                };

                resource.SetPnaStatus(pnaStatusId);

                db.ApplicationResources.Add(resource);

                db.SaveChanges();
            }

            var currentUserProfile = ServiceFactory.CreateCurrentUserProfileService();

            if (hasPermissions)
                currentUserProfile.Permissions = new string[] { Permission.ApplicationResourceReassign };

            var validator = GetValidator(db, currentUserProfile);

            // Act
            await validator.ValidateAsync(item);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(ApplicationStatus.Submitted)]
        [InlineData(ApplicationStatus.Postponed)]
        public async Task ValidateAsync_Throws_CountExceeded(string applicationStatusCode)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidCreateDto();
            var pnaStatusId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = pnaStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = PnaStatus.Issued,
                Value = string.Empty
            });

            db.SaveChanges();

            var resource = new ApplicationResource
            {
                ApplicationId = item.ApplicationId,
                PNANumber = "PNANumber",
                AssignedResourceId = item.AssignedResourceId.Value
            };

            resource.SetPnaStatus(pnaStatusId);

            db.ApplicationResources.Add(resource);

            db.SaveChanges();

            CreateApplication(db, item.ApplicationId, applicationStatusCode);
            CreateResource(db, item.AssignedResourceId.Value);

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(item));

            // Assert
            Assert.Equal(ApplicationResourceValidator.Error.CountExceeded, result.Message);
        }

        [Theory]
        [InlineData(ApplicationStatus.Declined)]
        [InlineData(ApplicationStatus.Withdrawn)]
        [InlineData(ApplicationStatus.Deleted)]
        public async Task ValidateAsync_Throws_CreationForbidden(string applicationStatusCode)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidCreateDto();

            CreateApplication(db, item.ApplicationId, applicationStatusCode);
            CreateResource(db, item.AssignedResourceId.Value);

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(item));

            // Assert
            Assert.Equal(ApplicationResourceValidator.Error.CreationForbidden, result.Message);
        }

        [Theory]
        [InlineData(PnaStatus.Preparing, false)]
        [InlineData(PnaStatus.Prepared, false)]
        [InlineData(PnaStatus.Issued, false)]
        [InlineData(PnaStatus.Returned, false)]
        [InlineData(PnaStatus.Stolen, false)]
        [InlineData(PnaStatus.Lost, false)]
        [InlineData(PnaStatus.Cancelled, false)]
        [InlineData(PnaStatus.Preparing, true)]
        [InlineData(PnaStatus.Prepared, true)]
        [InlineData(PnaStatus.Issued, true)]
        public async Task ValidateAsync_Throws_ReassignForbidden(string pnaStatusCode, bool hasPermissions)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = CreateValidCreateDto();
            var pnaStatusId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = pnaStatusId,
                Type = ClassifierTypes.PnaStatus,
                Code = pnaStatusCode,
                Value = string.Empty
            });

            db.SaveChanges();

            var resource = new ApplicationResource
            {
                ApplicationId = item.ApplicationId,
                PNANumber = "PNANumber",
                AssignedResourceId = item.AssignedResourceId.Value
            };

            resource.SetPnaStatus(pnaStatusId);

            db.ApplicationResources.Add(resource);

            db.SaveChanges();

            CreateApplication(db, item.ApplicationId, ApplicationStatus.Confirmed);
            CreateResource(db, item.AssignedResourceId.Value);

            var currentUserProfile = ServiceFactory.CreateCurrentUserProfileService();

            if (hasPermissions)
                currentUserProfile.Permissions = new string[] { Permission.ApplicationResourceReassign };

            var validator = GetValidator(db, currentUserProfile);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(item));

            // Assert
            Assert.Equal(ApplicationResourceValidator.Error.ReassignForbidden, result.Message);
        }

        [Theory]
        [InlineData(ResourceStatus.New)]
        [InlineData(ResourceStatus.InUse)]
        [InlineData(ResourceStatus.Reserved)]
        [InlineData(ResourceStatus.UnderRepair)]
        [InlineData(ResourceStatus.Lost)]
        [InlineData(ResourceStatus.Stolen)]
        public async Task ValidateResourceStatusAsync_Throws_AssignedResourceInvalidResourceStatus(string resourceStatus)
        {
            // Assign
            var resourceStatusId = Guid.NewGuid();
            var item = CreateValidEditDto(resourceStatusId);

            using var db = ServiceFactory.ConnectDb();

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceStatusId,
                Type = ClassifierTypes.ResourceStatus,
                Code = resourceStatus,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateResourceStatusAsync(item));

            // Assert
            Assert.Equal(ApplicationResourceValidator.Error.AssignedResourceInvalidResourceStatus, result.Message);
        }

        [Theory]
        [InlineData(ResourceStatus.Available)]
        [InlineData(ResourceStatus.Damaged)]
        [InlineData(ResourceStatus.Maintenance)]
        public async Task ValidateResourceStatusAsync_Succeeds(string resourceStatus)
        {
            // Assign
            var resourceStatusId = Guid.NewGuid();
            var item = CreateValidEditDto(resourceStatusId);

            using var db = ServiceFactory.ConnectDb();

            await db.Classifiers.AddAsync(new Classifier
            {
                Id = resourceStatusId,
                Type = ClassifierTypes.ResourceStatus,
                Code = resourceStatus,
                Value = string.Empty
            });

            await db.SaveChangesAsync();

            var validator = GetValidator(db);

            // Act
            await validator.ValidateResourceStatusAsync(item);

            // Assert
            Assert.True(true);
        }

        private ApplicationResourceCreateDto CreateValidCreateDto()
        {
            return new ApplicationResourceCreateDto
            {
                ApplicationId = Guid.NewGuid(),
                AssignedResourceId = Guid.NewGuid(),
                AssignedResourceReturnDate = DateTime.Now,
                Notes = string.Empty
            };
        }

        private ApplicationResourceReturnEditDto CreateValidEditDto(Guid resourceStatusId)
        {
            return new ApplicationResourceReturnEditDto
            {
                ResourceStatusId = resourceStatusId,
                ReturnResourceStateId = Guid.NewGuid(),
                ReturnResourceDate = DateTime.Now,
                Notes = string.Empty
            };
        }

        private ApplicationResourceValidator GetValidator(IAppDbContext db, ICurrentUserProfileService currentUserProfile = null)
        {
            return new ApplicationResourceValidator(
                db,
                currentUserProfile: currentUserProfile ?? ServiceFactory.CreateCurrentUserProfileService());
        }

        private void CreateApplication(
            IAppDbContext db,
            Guid applicatonId,
            string applicationStatusCode = ApplicationStatus.Confirmed)
        {
            var applicationStatusId = Guid.NewGuid();
            var submitterTypeId = Guid.NewGuid();
            var resourceTargetPersonTypeId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();

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

            db.Applications.AddAsync(new Domain.Entities.Application(applicationStatusId)
            {
                Id = applicatonId,
                ApplicationNumber = Guid.NewGuid().ToString().Substring(0, 10),
                ApplicationDate = DateTime.UtcNow,
                EducationalInstitutionId = 0,
                ResourceSubTypeId = resourceSubTypeId,
                SubmitterTypeId = submitterTypeId,
                ResourceTargetPersonId = Guid.NewGuid(),
                ResourceTargetPersonTypeId = resourceTargetPersonTypeId
            });

            db.SaveChanges();
        }

        private void CreateResource(IAppDbContext db, Guid resourceId)
        {
            var resourceLocationId = Guid.NewGuid();
            var targetGroupId = Guid.NewGuid();
            var usagePurposeTypeId = Guid.NewGuid();
            var acquisitionTypeId = Guid.NewGuid();
            var manufacturerId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
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
                    Id = resourceSubTypeId,
                    Type = ClassifierTypes.ResourceSubType,
                    Code = ResourceSubType.WindowsLaptop,
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
                Id = resourceId,
                ModelNameId = resourceModelId,
                ResourceStatusId = resourceStatusId,
                ResourceSubTypeId = resourceSubTypeId,
                ResourceNumber = "ResourceNumber",
                ResourceName = "ResourceName",
                InventoryNumber = "InventoryNumber",
                SerialNumber = "SerialNumber",
                AcquisitionTypeId = acquisitionTypeId,
                UsagePurposeTypeId = usagePurposeTypeId,
                TargetGroupId = targetGroupId,
                ResourceLocationId = resourceLocationId,
                ManufacturerId = manufacturerId,
                EducationalInstitutionId = 0
            });

            db.SaveChanges();
        }
    }
}
