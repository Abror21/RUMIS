using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public sealed class ResourcesControllerTests
    {
        private ResourcesController controller;
        private ClassifierServiceFake classifierServiceFake;
        private ResourceServiceFake serviceFake;
        private CurrentUserServiceFake currentUser;
        private ResourcesImportServiceFake resourcesImportServiceFake;

        public ResourcesControllerTests() 
        {
            serviceFake = new ResourceServiceFake();
            currentUser = new CurrentUserServiceFake();
            classifierServiceFake = new ClassifierServiceFake();
            resourcesImportServiceFake = new ResourcesImportServiceFake();
            controller = new ResourcesController(serviceFake, resourcesImportServiceFake, classifierServiceFake);
        }

        [Fact]
        public async Task Get_Succeeds()
        {
            // Assign
            currentUser.Permissions = new string[] { IdentityPermissions.ResourceView };

            const int educationalInstitutionId = 1;
            const int supervisorId = 1;

            var statusClassifierId = Guid.NewGuid();
            var resourceLocationId = Guid.NewGuid();
            var resourceSybTypeId = Guid.NewGuid();
            var resourceTypeId = Guid.NewGuid();
            var targetGroupId = Guid.NewGuid();
            var usagePurposeTypeId = Guid.NewGuid();
            var acquisitionTypeId = Guid.NewGuid();
            var manufacturerId = Guid.NewGuid();
            var resourceParameterClassifierId = Guid.NewGuid();
            var resourceParameterId = Guid.NewGuid();
            var resourceStatusId = Guid.NewGuid();
            var resourceModelId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            var classifiers = new Classifier[]
            {
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
                    Id = resourceTypeId,
                    Type = ClassifierTypes.ResourceType,
                    Code = "123",
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = resourceSybTypeId,
                    Type = ClassifierTypes.ResourceSubType,
                    Code = string.Empty,
                    Value = string.Empty,
                    Payload = "{\"resource_type\":\"123\"}"
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
                    Id = resourceParameterClassifierId,
                    Code = "c",
                    Value = "v",
                    Type = ClassifierTypes.ResourceParameter
                },
            };

            db.Classifiers.AddRange(classifiers);

            await db.SaveChangesAsync();

            await db.Supervisors.AddAsync(new Supervisor
            {
                Id = supervisorId,
                Code = "Code",
                Name = "Name",
            }); 

            await db.EducationalInstitutions.AddAsync(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                SupervisorId = supervisorId,
                StatusId = statusClassifierId,
                Code = "Code",
                Name = "Name"
            });

            await db.Resources.AddAsync(new Resource
            {
                ModelNameId = resourceModelId,
                ResourceStatusId = resourceStatusId,
                ResourceNumber = "ResourceNumber",
                ResourceName = "ResourceName",
                InventoryNumber = "InventoryNumber",
                SerialNumber = "SerialNumber",
                ResourceSubTypeId = resourceSybTypeId,
                AcquisitionTypeId = acquisitionTypeId,
                UsagePurposeTypeId = usagePurposeTypeId,
                TargetGroupId = targetGroupId,
                ResourceLocationId = resourceLocationId,
                ManufacturerId = manufacturerId,
                EducationalInstitutionId = educationalInstitutionId,
                ResourceParameters = {
                    new Domain.Entities.ResourceParameter
                    {
                        Id = resourceParameterId,
                        Value = "Value",
                        ParameterId = resourceParameterClassifierId
                    }
                }
            });

            await db.SaveChangesAsync();

            serviceFake.Resources = db.Resources.AsQueryable();
            classifierServiceFake.Data = new TestAsyncEnumerable<Classifier>(classifiers);

            // Act
            var result = await controller.Get();

            // Assert
            Assert.Equal(serviceFake.Resources.Count(), result.Value.Items.Count());
            Assert.Equal(resourceTypeId, result.Value.Items.First().ResourceType.Id);
        }
    }
}
