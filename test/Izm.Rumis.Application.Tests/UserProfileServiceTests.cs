using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class UserProfileServiceTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ActivateAsync_Succeeds(bool hasActivated)
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            var userProfile = UserProfile.Create();
            userProfile.Id = id;
            userProfile.IsLoggedIn = hasActivated;
            userProfile.User = User.Create();

            db.UserProfiles.Add(userProfile);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.ActivateAsync(id);

            var profile = db.UserProfiles.First();

            // Assert
            Assert.True(profile.IsLoggedIn);
        }

        [Fact]
        public async Task ActivateAsync_Throws_NotFound()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.ActivateAsync(Guid.NewGuid()));
        }

        [Theory]
        [InlineData(null, "someValue")]
        [InlineData("someValue", null)]
        [InlineData("someValue", "someValue")]
        public async Task CreateAsync_Succeeds_GdprTraceCreated(string email, string phoneNumber)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var user = await AddUserAsync(db);

            db.PersonTechnicals.Add(new PersonTechnical
            {
                UserId = user.Id
            });

            await db.SaveChangesAsync();

            var model = new UserProfileEditDto
            {
                Email = email,
                PhoneNumber = phoneNumber,
                UserId = user.Id,
                RoleIds = Array.Empty<int>()
            };

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await service.CreateAsync(model);

            // Assert
            Assert.True(db.UserProfiles.Any());
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task CreateAsync_Succeeds_GdprTraceNotCreated()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var user = await AddUserAsync(db);

            db.PersonTechnicals.Add(new PersonTechnical
            {
                UserId = user.Id
            });

            await db.SaveChangesAsync();

            var model = new UserProfileEditDto
            {
                UserId = user.Id,
                RoleIds = Array.Empty<int>()
            };

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            // Act
            var result = await service.CreateAsync(model);

            // Assert
            Assert.True(db.UserProfiles.Any());
            Assert.Null(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task CreateAsync_CheckData_CountryProfileType()
        {
            // Assign
            const UserProfileType type = UserProfileType.Country;

            using var db = ServiceFactory.ConnectDb();

            var user = await AddUserAsync(db);

            var supervisor = await AddSupervisorAsync(db);

            var educationalInstitution = await AddEducationalInstitutionAsync(db);

            await db.SaveChangesAsync();

            var model = new UserProfileEditDto
            {
                SupervisorId = supervisor.Id,
                RoleIds = Array.Empty<int>(),
                EducationalInstitutionId = educationalInstitution.Id,
                PermissionType = type,
                UserId = user.Id
            };

            var service = GetService(db);

            // Act
            var result = await service.CreateAsync(model);

            var userProfile = db.UserProfiles.First();

            // Assert
            Assert.Equal(user.Id, userProfile.UserId);
            Assert.Empty(userProfile.Roles);
            Assert.Equal(type, userProfile.PermissionType);
            Assert.Null(userProfile.SupervisorId);
            Assert.Null(userProfile.EducationalInstitutionId);
            Assert.Equal(model.Job, userProfile.Job);
            Assert.Equal(model.Email, userProfile.Email);
            Assert.Equal(model.PhoneNumber, userProfile.PhoneNumber);
            Assert.Equal(model.Expires, userProfile.Expires);
            Assert.Equal(model.IsDisabled, userProfile.Disabled);
        }

        [Fact]
        public async Task CreateAsync_CheckData_SupervisorProfileType()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var user = await AddUserAsync(db);

            var supervisor = await AddSupervisorAsync(db);

            var EducationalInstitution = await AddEducationalInstitutionAsync(db);

            await db.SaveChangesAsync();

            var model = new UserProfileEditDto
            {
                SupervisorId = supervisor.Id,
                RoleIds = Array.Empty<int>(),
                Job = "someJob",
                Email = "someEmail",
                PhoneNumber = "somePhoneNumber",
                Expires = DateTime.Now,
                IsDisabled = true,
                EducationalInstitutionId = EducationalInstitution.Id,
                PermissionType = UserProfileType.Supervisor,
                UserId = user.Id
            };

            var service = GetService(db);

            // Act
            var result = await service.CreateAsync(model);

            var userProfile = db.UserProfiles.First();

            // Assert
            Assert.Equal(user.Id, userProfile.UserId);
            Assert.Empty(userProfile.Roles);
            Assert.Equal(model.PermissionType, userProfile.PermissionType);
            Assert.Equal(supervisor.Id, userProfile.SupervisorId);
            Assert.Null(userProfile.EducationalInstitutionId);
            Assert.Equal(model.Job, userProfile.Job);
            Assert.Equal(model.Email, userProfile.Email);
            Assert.Equal(model.PhoneNumber, userProfile.PhoneNumber);
            Assert.Equal(model.Expires, userProfile.Expires);
            Assert.Equal(model.IsDisabled, userProfile.Disabled);
        }

        [Fact]
        public async Task CreateAsync_CheckData_EducationalInstitutionProfileType()
        {
            // Assign
            const UserProfileType type = UserProfileType.EducationalInstitution;

            using var db = ServiceFactory.ConnectDb();

            var user = await AddUserAsync(db);

            var supervisor = await AddSupervisorAsync(db);

            var EducationalInstitution = await AddEducationalInstitutionAsync(db);

            await db.SaveChangesAsync();

            var model = new UserProfileEditDto
            {
                SupervisorId = supervisor.Id,
                RoleIds = Array.Empty<int>(),
                EducationalInstitutionId = EducationalInstitution.Id,
                PermissionType = type,
                UserId = user.Id
            };

            var service = GetService(db);

            // Act
            var result = await service.CreateAsync(model);

            var userProfile = db.UserProfiles.First();

            // Assert
            Assert.Equal(user.Id, userProfile.UserId);
            Assert.Empty(userProfile.Roles);
            Assert.Equal(type, userProfile.PermissionType);
            Assert.Null(userProfile.SupervisorId);
            Assert.Equal(EducationalInstitution.Id, userProfile.EducationalInstitutionId);
            Assert.Equal(model.Job, userProfile.Job);
            Assert.Equal(model.Email, userProfile.Email);
            Assert.Equal(model.PhoneNumber, userProfile.PhoneNumber);
            Assert.Equal(model.Expires, userProfile.Expires);
            Assert.Equal(model.IsDisabled, userProfile.Disabled);
        }

        [Theory]
        [InlineData(null, "someValue")]
        [InlineData("someValue", null)]
        [InlineData("someValue", "someValue")]
        public async Task DeleteAsync_Succeeds_GdprTraceCreated(string email, string phoneNumber)
        {
            // Assing
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            var userProfile = UserProfile.Create();
            userProfile.Id = id;
            userProfile.Email = email;
            userProfile.PhoneNumber = phoneNumber;
            userProfile.UserId = userId;

            userProfile.SetRoles(new List<Role>
            {
                new Role
                {
                    Code = "someCode",
                    Name = "someName"
                }
            });

            db.UserProfiles.Add(userProfile);

            db.PersonTechnicals.Add(new PersonTechnical
            {
                UserId = userId
            });

            await db.SaveChangesAsync();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            // Act
            await service.DeleteAsync(id);

            // Assert
            Assert.Empty(db.UserProfiles);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task DeleteAsync_Succeeds_GdprTraceNotCreated()
        {
            // Assing
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            var userProfile = UserProfile.Create();
            userProfile.Id = id;
            userProfile.User = User.Create();

            userProfile.SetRoles(new List<Role>
            {
                new Role
                {
                    Code = "someCode",
                    Name = "someName"
                }
            });

            db.UserProfiles.Add(userProfile);

            db.PersonTechnicals.Add(new PersonTechnical
            {
                UserId = userId
            });

            await db.SaveChangesAsync();

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            // Act
            await service.DeleteAsync(id);

            // Assert
            Assert.Empty(db.UserProfiles);
            Assert.Null(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task DeleteAsync_Throws_CannotDeleteActivated()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            var userProfile = UserProfile.Create();
            userProfile.Id = id;
            userProfile.IsLoggedIn = true;
            userProfile.User = User.Create();

            db.UserProfiles.Add(userProfile);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(id));

            // Assert
            Assert.Equal(UserProfileService.Error.CannotDeleteActivated, result.Message);
        }

        [Fact]
        public async Task DeleteAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task Get_Succeeds()
        {
            using var db = ServiceFactory.ConnectDb();

            var userProfiles = new List<UserProfile>()
            {
                UserProfile.Create(),
                UserProfile.Create()
            };

            foreach (var userProfile in userProfiles)
                userProfile.User = User.Create();

            db.UserProfiles.AddRange(userProfiles);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var result = service.Get();

            // Assert
            Assert.Equal(userProfiles.Count, result.Count());
        }

        [Theory]
        [InlineData(UserProfileType.Country, 3)]
        [InlineData(UserProfileType.Supervisor, 2)]
        [InlineData(UserProfileType.EducationalInstitution, 1)]
        public async Task Get_Authorized_Succeeds(UserProfileType currentUserProfileType, int resultCount)
        {
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;
            const int supervisorId = 1;
            var statusClassifierId = Guid.NewGuid();

            var currentUserProfile = new CurrentUserProfileServiceFake();
            currentUserProfile.Type = currentUserProfileType;

            if (currentUserProfileType == UserProfileType.EducationalInstitution)
                currentUserProfile.EducationalInstitutionId = educationalInstitutionId;

            if (currentUserProfileType == UserProfileType.Supervisor)
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

            db.EducationalInstitutions.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId
            });

            var countryProfile = UserProfile.Create();
            countryProfile.User = User.Create();
            countryProfile.SetAccessLevel(new AccessLevel
            {
                Type = UserProfileType.Country
            });

            var supervisorProfile = UserProfile.Create();
            supervisorProfile.User = User.Create();
            supervisorProfile.SetAccessLevel(new AccessLevel
            {
                SupervisorId = supervisorId,
                Type = UserProfileType.Supervisor
            });

            var educationalInstitutionProfile = UserProfile.Create();
            educationalInstitutionProfile.User = User.Create();
            educationalInstitutionProfile.SetAccessLevel(new AccessLevel
            {
                EducationalInstitutionId = educationalInstitutionId,
                Type = UserProfileType.EducationalInstitution
            });

            db.UserProfiles.AddRange(countryProfile, supervisorProfile, educationalInstitutionProfile);

            await db.SaveChangesAsync();

            var service = GetService(db: db, currentUserProfileService: currentUserProfile);

            // Act
            var result = service.Get();

            // Assert
            Assert.Equal(resultCount, result.Count());
        }

        [Fact]
        public async Task GetCurrentUserProfiles_Succeeds()
        {
            // Assign
            var userId = Guid.NewGuid();
            using var db = ServiceFactory.ConnectDb();

            var userProfile = UserProfile.Create();
            userProfile.UserId = userId;

            var userProfiles = new List<UserProfile>()
            {
                userProfile,
                UserProfile.Create()
            };

            db.UserProfiles.AddRange(userProfiles);

            await db.SaveChangesAsync();

            var currentUser = ServiceFactory.CreateCurrentUserService();
            currentUser.Id = userId;

            var service = GetService(
                db: db,
                currentUser: currentUser
                );

            // Act
            var result = service.GetCurrentUserProfiles();

            // Assert
            Assert.Equal(1, result.Count());
        }

        [Fact]
        public async Task UpdateAsync_Succeeds_CountryUserProfile()
        {
            // Assign
            var id = Guid.NewGuid();
            const UserProfileType updatedType = UserProfileType.Country;

            using var db = ServiceFactory.ConnectDb();

            var initialUser = await AddUserAsync(db);
            var updatedUser = await AddUserAsync(db);

            var initialSupervisor = await AddSupervisorAsync(db);
            var updatedSupervisor = await AddSupervisorAsync(db);

            var initialEducationalInstitution = await AddEducationalInstitutionAsync(db);
            var updatedEducationalInstitution = await AddEducationalInstitutionAsync(db);

            await db.SaveChangesAsync();

            var userProfile = UserProfile.Create();
            userProfile.Id = id;
            userProfile.UserId = initialUser.Id;
            userProfile.SetAccessLevel(new AccessLevel
            {
                EducationalInstitutionId = initialEducationalInstitution.Id,
                SupervisorId = initialSupervisor.Id,
                Type = UserProfileType.Supervisor
            });

            db.UserProfiles.Add(userProfile);

            await db.SaveChangesAsync();

            var dto = new UserProfileEditDto()
            {
                SupervisorId = updatedSupervisor.Id,
                EducationalInstitutionId = updatedEducationalInstitution.Id,
                PermissionType = updatedType,
                UserId = updatedUser.Id
            };

            var service = GetService(db);

            // Act
            await service.UpdateAsync(id, dto);

            var createdUserProfile = db.UserProfiles.First();

            // Assert
            Assert.Equal(dto.UserId, createdUserProfile.UserId);
            Assert.Null(createdUserProfile.SupervisorId);
            Assert.Null(createdUserProfile.EducationalInstitutionId);
        }

        [Fact]
        public async Task UpdateAsync_Succeeds_SupervisorUserProfile()
        {
            // Assign
            var id = Guid.NewGuid();
            const UserProfileType updatedType = UserProfileType.Supervisor;

            using var db = ServiceFactory.ConnectDb();

            var initialRole = await AddRoleAsync(db);
            var updatedRole = await AddRoleAsync(db);

            var initialUser = await AddUserAsync(db);
            var updatedUser = await AddUserAsync(db);

            var initialSupervisor = await AddSupervisorAsync(db);
            var updatedSupervisor = await AddSupervisorAsync(db);

            var initialEducationalInstitution = await AddEducationalInstitutionAsync(db);
            var updatedEducationalInstitution = await AddEducationalInstitutionAsync(db);

            await db.SaveChangesAsync();

            var userProfile = UserProfile.Create();
            userProfile.Id = id;
            userProfile.UserId = initialUser.Id;
            userProfile.SetAccessLevel(new AccessLevel
            {
                EducationalInstitutionId = initialEducationalInstitution.Id,
                SupervisorId = initialSupervisor.Id,
                Type = UserProfileType.Supervisor
            });

            db.UserProfiles.Add(userProfile);

            db.PersonTechnicals.Add(new PersonTechnical
            {
                UserId = updatedUser.Id
            });

            await db.SaveChangesAsync();

            var dto = new UserProfileEditDto()
            {
                SupervisorId = updatedSupervisor.Id,
                EducationalInstitutionId = updatedEducationalInstitution.Id,
                PermissionType = updatedType,
                UserId = updatedUser.Id
            };

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            // Act
            await service.UpdateAsync(id, dto);

            var createdUserProfile = db.UserProfiles.First();

            // Assert
            Assert.Equal(dto.UserId, createdUserProfile.UserId);
            Assert.Equal(dto.SupervisorId, createdUserProfile.SupervisorId);
            Assert.Null(createdUserProfile.EducationalInstitutionId);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task UpdateAsync_Succeeds_EducationalInstitutionyUserProfile()
        {
            // Assign
            var id = Guid.NewGuid();
            const UserProfileType updatedType = UserProfileType.EducationalInstitution;

            using var db = ServiceFactory.ConnectDb();

            var initialRole = await AddRoleAsync(db);
            var updatedRole = await AddRoleAsync(db);

            var initialUser = await AddUserAsync(db);
            var updatedUser = await AddUserAsync(db);

            var initialSupervisor = await AddSupervisorAsync(db);
            var updatedSupervisor = await AddSupervisorAsync(db);

            var initialEducationalInstitution = await AddEducationalInstitutionAsync(db);
            var updatedEducationalInstitution = await AddEducationalInstitutionAsync(db);

            await db.SaveChangesAsync();

            var userProfile = UserProfile.Create();
            userProfile.Id = id;
            userProfile.UserId = initialUser.Id;
            userProfile.SetAccessLevel(new AccessLevel
            {
                EducationalInstitutionId = initialEducationalInstitution.Id,
                SupervisorId = initialSupervisor.Id,
                Type = UserProfileType.Supervisor
            });

            await db.UserProfiles.AddAsync(userProfile);

            db.PersonTechnicals.Add(new PersonTechnical
            {
                UserId = updatedUser.Id
            });

            await db.SaveChangesAsync();

            var dto = new UserProfileEditDto()
            {
                SupervisorId = updatedSupervisor.Id,
                EducationalInstitutionId = updatedEducationalInstitution.Id,
                PermissionType = updatedType,
                UserId = updatedUser.Id
            };

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var service = GetService(
                db: db,
                gdprAuditService: gdprAuditService
                );

            // Act
            await service.UpdateAsync(id, dto);

            var createdUserProfile = db.UserProfiles.First();

            // Assert
            Assert.Equal(dto.UserId, createdUserProfile.UserId);
            Assert.Null(createdUserProfile.SupervisorId);
            Assert.Equal(dto.EducationalInstitutionId, createdUserProfile.EducationalInstitutionId);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task UpdateAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.UpdateAsync(Guid.NewGuid(), new UserProfileEditDto()));
        }

        private async Task<Supervisor> AddSupervisorAsync(IAppDbContext db)
        {
            var supervisor = new Supervisor
            {
                Code = "someCode",
                Name = "someName"
            };

            await db.Supervisors.AddAsync(supervisor);

            return supervisor;
        }

        private async Task<Role> AddRoleAsync(IAppDbContext db, params string[] permissions)
        {
            var role = new Role
            {
                Code = "someCode",
                Name = "someName",
                Permissions = permissions.Select(t => new RolePermission
                {
                    Value = t
                }).ToArray()

            };

            await db.Roles.AddAsync(role);

            return role;
        }

        private async Task<EducationalInstitution> AddEducationalInstitutionAsync(IAppDbContext db)
        {
            var classifierId = Guid.NewGuid();

            db.Classifiers.Add(new Classifier
            {
                Id = classifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            await db.SaveChangesAsync();

            var EducationalInstitution = new EducationalInstitution
            {
                Code = "someCode",
                Name = "someName",
                StatusId = classifierId,
                Supervisor = new Supervisor
                {
                    Code = "someCode",
                    Name = "someName"
                }
            };

            await db.EducationalInstitutions.AddAsync(EducationalInstitution);

            return EducationalInstitution;
        }

        private async Task<User> AddUserAsync(IAppDbContext db)
        {
            var user = User.Create();

            await db.Users.AddAsync(user);

            return user;
        }

        private UserProfileService GetService(
            IAppDbContext db,
            IAuthorizationService authorizationService = null,
            ICurrentUserService currentUser = null,
            ICurrentUserProfileService currentUserProfileService = null,
            IGdprAuditService gdprAuditService = null)
        {
            return new UserProfileService(
                db: db,
                authorizationService: authorizationService ?? ServiceFactory.CreateAuthorizationService(),
                currentUserService: currentUser ?? ServiceFactory.CreateCurrentUserService(),
                currentUserProfile: currentUserProfileService ?? ServiceFactory.CreateCurrentUserProfileService(),
                gdprAuditService: gdprAuditService ?? ServiceFactory.CreateGdprAuditService()
                );
        }
    }
}
