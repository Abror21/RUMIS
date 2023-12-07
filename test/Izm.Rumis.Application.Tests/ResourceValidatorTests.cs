using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class ResourceValidatorTests
    {
        [Fact]
        public async void ValidateAsync_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var dto = CreateValidResourceCreateDto(db);

            var validator = GetValidator(db);

            // Act
            await validator.ValidateAsync(dto);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task ValidateAsync_Throws_ParameterRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var dto = CreateValidResourceCreateDto(db);

            dto.ResourceParameters.First().Value = string.Empty;

            await db.SaveChangesAsync();

            var validator = GetValidator(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAsync(dto));

            // Assert
            Assert.Equal(ResourceValidator.Error.ParameterRequired, result.Message);
        }

        private ResourceCreateDto CreateValidResourceCreateDto(IAppDbContext db)
        {
            var resourceSubTypeId = Guid.NewGuid();
            var parameterId1 = Guid.NewGuid();
            var parameterId2 = Guid.NewGuid();
            var parameterId3 = Guid.NewGuid();

            SeedClassifiers(db, resourceSubTypeId, parameterId1, parameterId2, parameterId3);

            return new ResourceCreateDto
            {
                ResourceSubTypeId = resourceSubTypeId,
                ResourceParameters = new List<ResourceParameterDto>
                {
                    new ResourceParameterDto
                    {
                        ParameterId = parameterId1,
                        Value = "Value"
                    },
                    new ResourceParameterDto
                    {
                        ParameterId = parameterId2,
                        Value = string.Empty
                    },
                    new ResourceParameterDto
                    {
                        ParameterId = parameterId3,
                        Value = string.Empty
                    }
                }
            };
        }

        private ResourceValidator GetValidator(IAppDbContext db)
        {
            return new ResourceValidator(db);
        }

        private void SeedClassifiers(IAppDbContext db, Guid resourceSubTypeId, Guid parameterId1, Guid parameterId2, Guid parameterId3)
        {
            db.Classifiers.AddRange(
                new Classifier
                {
                    Id = resourceSubTypeId,
                    Type = ClassifierTypes.ResourceSubType,
                    Code = ResourceSubType.WindowsLaptop,
                    Value = string.Empty,
                    Payload = JsonSerializer.Serialize(new ResourceSubTypePayload
                    {
                        ResourceParameterGroups = new[]
                        {
                                        new ResourceSubTypePayload.ResourceParameterGroup
                                        {
                                            Parameters = new[]
                                            {
                                                 new ResourceSubTypePayload.ResourceParameterGroup.Parameter
                                                 {
                                                     Code = Domain.Constants.Classifiers.ResourceParameter.InternetConnection,
                                                     IsRequired = true
                                                 }
                                            }
                                        },
                                        new ResourceSubTypePayload.ResourceParameterGroup
                                        {
                                            Parameters = new[]
                                            {
                                                 new ResourceSubTypePayload.ResourceParameterGroup.Parameter
                                                 {
                                                     Code = Domain.Constants.Classifiers.ResourceParameter.Ram,
                                                     IsRequired = false
                                                 }
                                            }
                                        }
                        }
                    })
                },
                new Classifier
                {
                    Id = parameterId1,
                    Type = ClassifierTypes.ResourceParameter,
                    Code = Domain.Constants.Classifiers.ResourceParameter.InternetConnection,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = parameterId2,
                    Type = ClassifierTypes.ResourceParameter,
                    Code = Domain.Constants.Classifiers.ResourceParameter.Ram,
                    Value = string.Empty
                },
                new Classifier
                {
                    Id = parameterId3,
                    Type = ClassifierTypes.ResourceParameter,
                    Code = Domain.Constants.Classifiers.ResourceParameter.DiscType,
                    Value = string.Empty
                });

            db.SaveChanges();
        }
    }
}
