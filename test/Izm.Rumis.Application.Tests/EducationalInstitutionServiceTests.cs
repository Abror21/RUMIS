using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class EducationalInstitutionServiceTests
    {
        [Fact]
        public async Task CreateAsync_Succeeds()
        {
            // Assign
            const int supervisorId = 1;
            var statusClassifierId = Guid.NewGuid();

            var model = new EducationalInstitutionCreateDto
            {
                Code = "someCode",
                Name = "someName",
                SupervisorId = supervisorId,
                StatusId = statusClassifierId
            };

            using var db = ServiceFactory.ConnectDb();

            await db.Supervisors.AddAsync(new Supervisor
            {
                Id = supervisorId,
                Code = "someCode",
                Name = "someName"
            });

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var result = await service.CreateAsync(model);

            // Assert
            Assert.True(db.EducationalInstitutions.Any());
        }

        [Fact]
        public async Task CreateAsync_CheckData()
        {
            // Assign
            const int supervisorId = 1;
            const string code = "someCode";
            const string name = "someName";
            var statusClassifierId = Guid.NewGuid();

            var model = new EducationalInstitutionCreateDto
            {
                Code = code,
                Name = name,
                SupervisorId = supervisorId,
                StatusId = statusClassifierId
            };

            using var db = ServiceFactory.ConnectDb();

            await db.Supervisors.AddAsync(new Supervisor
            {
                Id = supervisorId,
                Code = "someCode",
                Name = "someName"
            });

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var result = await service.CreateAsync(model);

            var EducationalInstitution = db.EducationalInstitutions.First();

            // Assert
            Assert.Equal(code, EducationalInstitution.Code);
            Assert.Equal(name, EducationalInstitution.Name);
            Assert.Equal(supervisorId, EducationalInstitution.SupervisorId);
        }

        [Fact]
        public async Task DeleteAsync_Succeeds()
        {
            // Assing
            const int id = 1;
            var statusClassifierId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.EducationalInstitutions.Add(new EducationalInstitution
            {
                Id = id,
                StatusId = statusClassifierId,
                Code = "someCode",
                Name = "someName",
                Supervisor = new Supervisor
                {
                    Code = "someCode",
                    Name = "someName"
                }
            });

            db.SaveChanges();

            var service = GetService(db);

            // Act
            await service.DeleteAsync(1);

            // Assert
            Assert.Empty(db.EducationalInstitutions);
        }

        [Fact]
        public async Task DeleteAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.DeleteAsync(1));
        }

        [Fact]
        public async Task Get_Succeeds_CountryProfileType()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var statusClassifierId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            var EducationalInstitutions = new List<EducationalInstitution>()
            {
                new EducationalInstitution
                {
                    Name = "someName",
                    Code = "someCode",
                    StatusId = statusClassifierId,
                    Supervisor = new Supervisor
                    {
                        Code = "someCode",
                        Name = "someName"
                    }
                },
                new EducationalInstitution
                {
                    Code = "someCode",
                    Name = "someName",
                    StatusId = statusClassifierId,
                    Supervisor = new Supervisor
                    {
                        Code = "someCode",
                        Name = "someName"
                    }
                }
            };

            db.EducationalInstitutions.AddRange(EducationalInstitutions);

            await db.SaveChangesAsync();

            var currentUserProfile = ServiceFactory.CreateCurrentUserProfileService();

            currentUserProfile.Type = UserProfileType.Country;

            var service = GetService(db, currentUserProfile);

            // Act
            var result = service.Get();

            // Assert
            Assert.Equal(EducationalInstitutions.Count, result.Count());
        }

        [Fact]
        public async Task Get_Succeeds_SupervisorProfileType()
        {
            // Assign
            const int supervisorId = 1;
            var statusClassifierId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.EducationalInstitutions.AddRange(new List<EducationalInstitution>()
            {
                new EducationalInstitution
                {
                    Code = "someCode",
                    Name = "someName",
                    StatusId = statusClassifierId,
                    Supervisor = new Supervisor
                    {
                        Id = supervisorId,
                        Code = "someCode",
                        Name = "someName"
                    }
                },
                new EducationalInstitution
                {
                    Code = "someCode",
                    Name = "someName",
                    StatusId = statusClassifierId,
                    Supervisor = new Supervisor
                    {
                        Id = supervisorId + 1,
                        Code = "someCode",
                        Name = "someName"
                    }
                }
            });

            await db.SaveChangesAsync();

            var currentUserProfile = ServiceFactory.CreateCurrentUserProfileService();

            currentUserProfile.Type = UserProfileType.Supervisor;
            currentUserProfile.SupervisorId = supervisorId;

            var service = GetService(db, currentUserProfile);

            // Act
            var result = service.Get();

            // Assert
            Assert.Single(result.List());
        }

        [Fact]
        public async Task Get_Succeeds_EducationalInstitutionProfileType()
        {
            // Assign
            const int EducationalInstitutiondId = 1;
            var statusClassifierId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.EducationalInstitutions.AddRange(new List<EducationalInstitution>()
            {
                new EducationalInstitution
                {
                    Id = EducationalInstitutiondId,
                    Code = "someCode",
                    Name = "someName",
                    StatusId = statusClassifierId,
                    Supervisor = new Supervisor
                    {
                        Id = 1,
                        Code = "someCode",
                        Name = "someName"
                    }
                },
                new EducationalInstitution
                {
                    Id = EducationalInstitutiondId + 1,
                    Code = "someCode",
                    Name = "someName",
                    StatusId = statusClassifierId,
                    Supervisor = new Supervisor
                    {
                        Id = 2,
                        Code = "someCode",
                        Name = "someName"
                    }
                }
            });

            await db.SaveChangesAsync();

            var currentUserProfile = ServiceFactory.CreateCurrentUserProfileService();

            currentUserProfile.Type = UserProfileType.EducationalInstitution;
            currentUserProfile.EducationalInstitutionId = EducationalInstitutiondId;

            var service = GetService(db, currentUserProfile);

            // Act
            var result = service.Get();

            // Assert
            Assert.Single(result.List());
            Assert.Equal(EducationalInstitutiondId, result.First().Id);
        }

        [Fact]
        public async Task UpdateAsync_Succeeds()
        {
            // Assing
            const int id = 1;
            var statusClassifierId = Guid.NewGuid();
            var resourceSubTypeFirstClassifierId = Guid.NewGuid();
            var resourceSubTypeSecondClassifierId = Guid.NewGuid();
            var educationalInstitutionContactPersonId = Guid.NewGuid();
            var contactPersonResourceSubTypeId = Guid.NewGuid();
            var jobPositionId = Guid.NewGuid();
            var newJobPositionId = Guid.NewGuid();

            var dto = new EducationalInstitutionUpdateDto
            {
                Code = "updatedCode",
                Name = "updatedName",
                StatusId = statusClassifierId,
                SupervisorId = 3,
                EducationalInstitutionContactPersons = new List<EducationalInstitutionUpdateDto.EducationalInstitutionContactPersonData>
                {
                    new EducationalInstitutionUpdateDto.EducationalInstitutionContactPersonData
                    {
                        Id = educationalInstitutionContactPersonId,
                        Name = "SomeName",
                        Email = "SomeEmail",
                        PhoneNumber = "SomePhoneNumber",
                        JobPositionId = newJobPositionId,
                        ContactPersonResourceSubTypes = new List<EducationalInstitutionUpdateDto.EducationalInstitutionContactPersonData.ContactPersonResourceSubTypeData>
                        {
                            new EducationalInstitutionUpdateDto.EducationalInstitutionContactPersonData.ContactPersonResourceSubTypeData
                            {
                                Id = contactPersonResourceSubTypeId,
                                ResourceSubTypeId = resourceSubTypeSecondClassifierId
                            },
                            new EducationalInstitutionUpdateDto.EducationalInstitutionContactPersonData.ContactPersonResourceSubTypeData
                            {
                                ResourceSubTypeId = resourceSubTypeFirstClassifierId
                            }
                        }
                    },
                    new EducationalInstitutionUpdateDto.EducationalInstitutionContactPersonData
                    {
                        JobPositionId = jobPositionId,
                        ContactPersonResourceSubTypes = new List<EducationalInstitutionUpdateDto.EducationalInstitutionContactPersonData.ContactPersonResourceSubTypeData>
                        {
                            new EducationalInstitutionUpdateDto.EducationalInstitutionContactPersonData.ContactPersonResourceSubTypeData
                            {
                                ResourceSubTypeId = resourceSubTypeFirstClassifierId
                            }
                        }
                    }
                }
            };

            using var db = ServiceFactory.ConnectDb();

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.Classifiers.Add(new Classifier
            {
                Id = newJobPositionId,
                Code = string.Empty,
                Value = string.Empty,
                Type = ClassifierTypes.EducationalInstitutionJobPosition
            });

            db.Classifiers.Add(new Classifier
            {
                Id = jobPositionId,
                Code = string.Empty,
                Value = string.Empty,
                Type = ClassifierTypes.EducationalInstitutionJobPosition
            });

            db.Classifiers.Add(new Classifier
            {
                Id = resourceSubTypeFirstClassifierId,
                Code = string.Empty,
                Value = string.Empty,
                Type = ClassifierTypes.ResourceSubType
            });

            db.Classifiers.Add(new Classifier
            {
                Id = resourceSubTypeSecondClassifierId,
                Code = string.Empty,
                Value = string.Empty,
                Type = ClassifierTypes.ResourceSubType
            });

            db.Supervisors.Add(new Supervisor
            {
                Id = dto.SupervisorId,
                Code = "someCode",
                Name = "someName"
            });

            db.SaveChanges();

            db.EducationalInstitutions.Add(new EducationalInstitution
            {
                Id = id,
                Code = "someCode",
                Name = "someName",
                StatusId = statusClassifierId,
                Supervisor = new Supervisor
                {
                    Id = dto.SupervisorId + 1,
                    Code = "someCode",
                    Name = "someName"
                }
            });

            db.EducationalInstitutionContactPersons.Add(new EducationalInstitutionContactPerson
            {
                Id = educationalInstitutionContactPersonId,
                JobPositionId = jobPositionId,
                EducationalInstitutionId = id
            });

            await db.SaveChangesAsync();

            db.ContactPersonResourceSubTypes.Add(new ContactPersonResourceSubType
            {
                Id = contactPersonResourceSubTypeId,
                EducationalInstitutionContactPersonId = educationalInstitutionContactPersonId,
                ResourceSubTypeId = resourceSubTypeFirstClassifierId
            });

            db.ContactPersonResourceSubTypes.Add(new ContactPersonResourceSubType
            {
                EducationalInstitutionContactPersonId = educationalInstitutionContactPersonId,
                ResourceSubTypeId = resourceSubTypeSecondClassifierId
            });

            await db.SaveChangesAsync();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            // Act
            await service.UpdateAsync(1, dto);

            var educationalInstitution = db.EducationalInstitutions.First();

            var educationalInstitutionContactPerson = db.EducationalInstitutionContactPersons;

            var contactPersonResourceSubType = db.ContactPersonResourceSubTypes;

            // Assert
            Assert.Equal(dto.Code, educationalInstitution.Code);
            Assert.Equal(dto.Name, educationalInstitution.Name);
            Assert.Equal(dto.SupervisorId, educationalInstitution.SupervisorId);
            Assert.Equal(2, educationalInstitutionContactPerson.Count());
            Assert.Equal(3, contactPersonResourceSubType.Count());
            Assert.Equal(dto.EducationalInstitutionContactPersons.FirstOrDefault(t => t.Id == educationalInstitutionContactPersonId).Name,
                         educationalInstitutionContactPerson.FirstOrDefault(t => t.Id == educationalInstitutionContactPersonId).Name);
            Assert.Equal(dto.EducationalInstitutionContactPersons.FirstOrDefault(t => t.Id == educationalInstitutionContactPersonId).Email,
                         educationalInstitutionContactPerson.FirstOrDefault(t => t.Id == educationalInstitutionContactPersonId).Email);
            Assert.Equal(dto.EducationalInstitutionContactPersons.FirstOrDefault(t => t.Id == educationalInstitutionContactPersonId).PhoneNumber,
                         educationalInstitutionContactPerson.FirstOrDefault(t => t.Id == educationalInstitutionContactPersonId).PhoneNumber);
            Assert.Equal(dto.EducationalInstitutionContactPersons.FirstOrDefault(t => t.Id == educationalInstitutionContactPersonId).JobPositionId,
                         educationalInstitutionContactPerson.FirstOrDefault(t => t.Id == educationalInstitutionContactPersonId).JobPositionId);
            Assert.Equal(dto.EducationalInstitutionContactPersons.FirstOrDefault(t => t.Id == educationalInstitutionContactPersonId)
                        .ContactPersonResourceSubTypes.FirstOrDefault(t => t.Id == contactPersonResourceSubTypeId).ResourceSubTypeId,
                         educationalInstitutionContactPerson.FirstOrDefault(t => t.Id == educationalInstitutionContactPersonId)
                         .ContactPersonResourceSubTypes.FirstOrDefault(t => t.Id == contactPersonResourceSubTypeId).ResourceSubTypeId);
            Assert.NotNull(gdprAuditService.TraceRangeAsyncCalledWith);

        }

        [Fact]
        public async Task UpdateAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.UpdateAsync(1, new EducationalInstitutionUpdateDto()));
        }

        private EducationalInstitutionService GetService(
            IAppDbContext db,
            ICurrentUserProfileService currentUserProfileService = null,
            IGdprAuditService gdprAuditService = null)
        {
            return new EducationalInstitutionService(
                db: db,
                currentUserProfile: currentUserProfileService ?? ServiceFactory.CreateCurrentUserProfileService(),
                gdprAuditService: gdprAuditService ?? ServiceFactory.CreateGdprAuditService()
                );
        }
    }
}
