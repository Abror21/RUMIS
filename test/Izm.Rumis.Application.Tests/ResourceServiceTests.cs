using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
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
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class ResourceServiceTests
    {
        [Fact]
        public void Get_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            CreateResource(db);

            var service = GetService(db);

            // Act
            var result = service.Get();

            // Assert
            Assert.Single(result.List());
        }

        [Fact]
        public async void Create_Async()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var statusClassifierId = Guid.NewGuid();
            var resourceLocationId = Guid.NewGuid();
            var targetGroupId = Guid.NewGuid();
            var usagePurposeTypeId = Guid.NewGuid();
            var acquisitionTypeId = Guid.NewGuid();
            var manufacturerId = Guid.NewGuid();
            var parameterId = Guid.NewGuid();
            var resourceStatusId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
            var resourceModelId = Guid.NewGuid();

            const int educationalInstitutionId = 1;

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
                    Id = statusClassifierId,
                    Code = "c",
                    Value = "v",
                    Type = ClassifierTypes.EducationalInstitutionStatus
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
                },
                 new Classifier
                 {
                     Id = parameterId,
                     Type = ClassifierTypes.ResourceParameter,
                     Code = string.Empty,
                     Value = string.Empty
                 });
            db.SaveChanges();

            db.EducationalInstitutions.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                StatusId = statusClassifierId,
                Code = "Code",
                Name = "Name"
            });

            db.SaveChanges();

            var createModel = new ResourceCreateDto
            {
                ResourceName = "ResourceName",
                InventoryNumber = "InventoryNumber",
                SerialNumber = "SerialNumber",
                ResourceStatusId = resourceStatusId,
                ResourceSubTypeId = resourceSubTypeId,
                AcquisitionTypeId = acquisitionTypeId,
                UsagePurposeTypeId = usagePurposeTypeId,
                TargetGroupId = targetGroupId,
                ResourceLocationId = resourceLocationId,
                ManufacturerId = manufacturerId,
                EducationalInstitutionId = educationalInstitutionId,
                ModelNameId = resourceModelId,
                ResourceParameters = new List<ResourceParameterDto>
                {
                    new ResourceParameterDto
                    {
                        ParameterId = parameterId,
                        Value = string.Empty
                    },
                    new ResourceParameterDto
                    {
                        ParameterId = parameterId,
                        Value = string.Empty
                    }
                }
            };

            var validator = ServiceFactory.CreateResourceValidatorFake();

            var authorizationService = ServiceFactory.CreateAuthorizationService();

            var service = GetService(db,
                validator: validator,
                authorizationService: authorizationService);

            // Act
            var resultGuid = service.CreateAsync(createModel);

            var result = service.Get();

            var resultData = await db.Resources.FirstOrDefaultAsync();

            // Assert
            Assert.True(resultGuid != null);
            Assert.Single(result.List());
            Assert.NotNull(authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.NotNull(validator.ValidateAsyncCreateCalledWith);
            Assert.Equal(acquisitionTypeId, resultData.AcquisitionTypeId);
            Assert.Equal(usagePurposeTypeId, resultData.UsagePurposeTypeId);
            Assert.Equal(targetGroupId, resultData.TargetGroupId);
            Assert.Equal(resourceLocationId, resultData.ResourceLocationId);
            Assert.Equal(manufacturerId, resultData.ManufacturerId);
            Assert.Equal(educationalInstitutionId, resultData.EducationalInstitutionId);
            Assert.Equal(createModel.ResourceParameters.Count(), resultData.ResourceParameters.Count);
        }

        [Fact]
        public async void Edit_Async()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();
            CreateResource(db);
            var parameterId = await db.Classifiers.FirstAsync(t => t.Type == ClassifierTypes.ResourceParameter);


            var resultData = await db.Resources.FirstOrDefaultAsync();

            var editModel = new ResourceUpdateDto
            {
                ResourceName = "ResourceName",
                InventoryNumber = "InventoryNumber",
                SerialNumber = "SerialNumber",
                AcquisitionTypeId = resultData.AcquisitionTypeId,
                UsagePurposeTypeId = resultData.UsagePurposeTypeId,
                TargetGroupId = resultData.TargetGroupId,
                ResourceLocationId = resultData.ResourceLocationId,
                ManufacturerId = resultData.ManufacturerId,
                ResourceParameters = new List<ResourceParameterDto>
                {
                    new ResourceParameterDto
                    {
                        Id = resultData.ResourceParameters.First().Id,
                        ParameterId = parameterId.Id,
                        Value = string.Empty
                    },
                    new ResourceParameterDto
                    {
                        ParameterId = parameterId.Id,
                        Value = string.Empty
                    }
                }
            };

            var validator = ServiceFactory.CreateResourceValidatorFake();

            var authorizationService = ServiceFactory.CreateAuthorizationService();

            var service = GetService(db,
                validator: validator,
                authorizationService: authorizationService);

            // Act
            var resultEdit = service.UpdateAsync(resultData.Id, editModel);

            resultData = await db.Resources.FirstOrDefaultAsync();

            // Assert
            Assert.NotNull(authorizationService.AuthorizeEducationalInstitutionCalledWith);
            Assert.NotNull(validator.ValidateAsyncUpdateCalledWith);
            Assert.Equal(resultData.AcquisitionTypeId, editModel.AcquisitionTypeId);
            Assert.Equal(resultData.UsagePurposeTypeId, editModel.UsagePurposeTypeId);
            Assert.Equal(resultData.TargetGroupId, editModel.TargetGroupId);
            Assert.Equal(resultData.ResourceLocationId, editModel.ResourceLocationId);
            Assert.Equal(resultData.ManufacturerId, editModel.ManufacturerId);
            Assert.Equal(resultData.ResourceParameters.Count, editModel.ResourceParameters.Count());

            const string value1 = "123";
            const string value2 = "321";

            editModel.ResourceParameters = new List<ResourceParameterDto>
                 {
                    new ResourceParameterDto
                    {
                        Id = resultData.ResourceParameters.ElementAt(0).Id,
                        ParameterId = parameterId.Id,
                        Value = value1
                    },
                    new ResourceParameterDto
                    {
                        Id = resultData.ResourceParameters.ElementAt(1).Id,
                        ParameterId = parameterId.Id,
                        Value = value2
                    }
                };
            resultEdit = service.UpdateAsync(resultData.Id, editModel);

            resultData = await db.Resources.FirstOrDefaultAsync();

            Assert.Equal(resultData.ResourceParameters.Count, editModel.ResourceParameters.Count());
            Assert.Equal(value1, resultData.ResourceParameters.ElementAt(0).Value);
            Assert.Equal(value2, resultData.ResourceParameters.ElementAt(1).Value);

            editModel.ResourceParameters = new List<ResourceParameterDto> { };

            resultEdit = service.UpdateAsync(resultData.Id, editModel);

            resultData = await db.Resources.FirstOrDefaultAsync();

            Assert.Equal(resultData.ResourceParameters.Count, editModel.ResourceParameters.Count());
        }

        [Theory]
        [InlineData(UserProfileType.Country, 3)]
        [InlineData(UserProfileType.Supervisor, 2)]
        [InlineData(UserProfileType.EducationalInstitution, 1)]
        public void Get_Authorized_Succeeds(UserProfileType currentUserProfileType, int resultCount)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;
            const int supervisorId = 1;
            var statusClassifierId = Guid.NewGuid();

            var currentUserProfile = new CurrentUserProfileServiceFake();
            currentUserProfile.Type = currentUserProfileType;
            currentUserProfile.EducationalInstitutionId = educationalInstitutionId;
            currentUserProfile.SupervisorId = supervisorId;

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.Supervisors.Add(new Supervisor
            {
                Id = supervisorId,
                Code = "c",
                Name = "n"
            });

            db.EducationalInstitutions.AddRange(
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
                    Id = educationalInstitutionId + 1,
                    Code = "c",
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = supervisorId
                });

            CreateResource(db, educationalInstitutionId);
            CreateResource(db, educationalInstitutionId + 1);
            CreateResource(db, educationalInstitutionId + 2);

            var service = GetService(db, currentUserProfile);

            // Act
            var result = service.Get();

            // Assert
            Assert.Equal(resultCount, result.Count());
        }

        private ResourceService GetService(
            IAppDbContext db,
            ICurrentUserProfileService currentUserProfile = null,
            IAuthorizationService authorizationService = null,
            ISequenceService sequenceService = null,
            IResourceValidator validator = null)
        {
            return new ResourceService(
                db: db,
                currentUserProfile: currentUserProfile ?? ServiceFactory.CreateCurrentUserProfileService(),
                authorizationService: authorizationService ?? ServiceFactory.CreateAuthorizationService(),
                sequenceService: sequenceService ?? ServiceFactory.CreateSequenceService(),
                validator: validator ?? ServiceFactory.CreateResourceValidatorFake());
        }

        private void CreateResource(IAppDbContext db, int eduInstId = 0)
        {
            var resourceLocationId = Guid.NewGuid();
            var targetGroupId = Guid.NewGuid();
            var usagePurposeTypeId = Guid.NewGuid();
            var acquisitionTypeId = Guid.NewGuid();
            var manufacturerId = Guid.NewGuid();
            var parameterId = Guid.NewGuid();
            var resourceParameterId = Guid.NewGuid();
            var resourceStatusId = Guid.NewGuid();
            var resourceSubTypeId = Guid.NewGuid();
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
                },
                new Classifier
                {
                    Id = parameterId,
                    Type = ClassifierTypes.ResourceParameter,
                    Code = string.Empty,
                    Value = string.Empty
                });

            db.SaveChanges();

            db.Resources.Add(new Resource
            {
                ModelNameId = resourceModelId,
                ResourceNumber = NumberingPatternHelper.ResourceNumberFormat(),
                ResourceSubTypeId = resourceSubTypeId,
                ResourceStatusId = resourceStatusId,
                ResourceName = "ResourceName",
                InventoryNumber = "InventoryNumber",
                SerialNumber = "SerialNumber",
                AcquisitionTypeId = acquisitionTypeId,
                UsagePurposeTypeId = usagePurposeTypeId,
                TargetGroupId = targetGroupId,
                ResourceLocationId = resourceLocationId,
                ManufacturerId = manufacturerId,
                EducationalInstitutionId = eduInstId,
                ResourceParameters = new List<Domain.Entities.ResourceParameter>()
                {
                    new Domain.Entities.ResourceParameter
                    {
                        Id = resourceParameterId,
                        ParameterId = parameterId,
                        Value = string.Empty
                    }
                }
            });

            db.SaveChanges();
        }
    }
}
