using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Izm.Rumis.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class UserServiceTests
    {
        [Fact]
        public async Task DeleteAsync_Succeeds_NoRelations()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Users.Add(User.Create(id));

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.DeleteAsync(id);

            // Assert
            Assert.Empty(db.Users);
        }

        [Fact]
        public async Task DeleteAsync_Succeeds_WithProfiles()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Users.Add(User.Create(id));

            var userProfile = UserProfile.Create();
            userProfile.UserId = id;
            userProfile.SetRoles(new List<Role>()
            {
                new Role
                {
                    Code = "someRole",
                    Name = "someName"
                }
            });

            db.UserProfiles.Add(userProfile);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.DeleteAsync(id);

            // Assert
            Assert.Empty(db.Users);
            Assert.Empty(db.UserProfiles);
        }

        [Fact]
        public async Task DeleteAsync_Succeeds_WithIdentity()
        {
            // Assign
            var id = Guid.NewGuid();
            var loginId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            var user = User.Create(id);

            db.Users.Add(user);

            db.IdentityUsers.Add(new IdentityUser
            {
                Logins = new List<IdentityUserLogin>
                {
                    new IdentityUserLogin
                    {
                        Id = loginId
                    }
                },
                User = user
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.DeleteAsync(id);

            // Assert
            Assert.Empty(db.Users);
            Assert.Empty(db.IdentityUsers);
            Assert.Empty(db.IdentityUserLogins);
        }

        [Fact]
        public async Task DeleteAsync_Succeeds_WithPerson()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            var user = User.Create(id);

            db.Users.Add(user);

            db.PersonTechnicals.Add(new PersonTechnical
            {
                UserId = id
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.DeleteAsync(id);

            // Assert
            Assert.Empty(db.Users);
            Assert.Single(db.PersonTechnicals);
        }

        [Fact]
        public async Task DeleteAsync_Throws_CannotDeleteActivated()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Users.Add(User.Create(id));

            var userProfile = UserProfile.Create();
            userProfile.UserId = id;
            userProfile.IsLoggedIn = true;

            db.UserProfiles.Add(userProfile);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(id));

            // Assert
            Assert.Equal(UserService.Error.CannotDeleteActivated, result.Message);
        }

        [Fact]
        public async Task DeleteAsync_Throws_NotFound()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetPersons_Succeeds()
        {
            using var db = ServiceFactory.ConnectDb();

            db.PersonTechnicals.Add(new PersonTechnical
            {
                User = User.Create()
            });

            db.Users.Add(User.Create());

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var result = service.GetPersons();

            // Assert
            Assert.Single(result.List());
        }

        [Theory]
        [InlineData(UserProfileType.Country, 3)]
        [InlineData(UserProfileType.Supervisor, 2)]
        [InlineData(UserProfileType.EducationalInstitution, 1)]
        public async Task GetPersons_Authorized_Succeeds(UserProfileType currentUserProfileType, int resultCount)
        {
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;
            const int supervisorId = 1;

            var statusClassifierId = Guid.NewGuid();

            Guid userId = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid user3Id = Guid.NewGuid();

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

            db.PersonTechnicals.AddRange(
                new PersonTechnical { UserId = userId },
                new PersonTechnical { UserId = user2Id },
                new PersonTechnical { UserId = user3Id });

            db.Users.AddRange(
                User.Create(userId),
                User.Create(user2Id),
                User.Create(user3Id));

            var countryProfile = UserProfile.Create();
            countryProfile.UserId = userId;

            countryProfile.SetAccessLevel(new AccessLevel
            {
                Type = UserProfileType.Country
            });

            var supervisorProfile = UserProfile.Create();
            supervisorProfile.UserId = user2Id;

            supervisorProfile.SetAccessLevel(new AccessLevel
            {
                SupervisorId = supervisorId,
                Type = UserProfileType.Supervisor
            });

            var educationalInstitutionProfile = UserProfile.Create();
            educationalInstitutionProfile.UserId = user3Id;

            educationalInstitutionProfile.SetAccessLevel(new AccessLevel
            {
                EducationalInstitutionId = educationalInstitutionId,
                Type = UserProfileType.EducationalInstitution
            });


            db.UserProfiles.AddRange(countryProfile, supervisorProfile, educationalInstitutionProfile);

            await db.SaveChangesAsync();

            var service = GetService(db, currentUserProfile);

            // Act
            var result = service.GetPersons();

            // Assert
            Assert.Equal(resultCount, result.Count());
        }

        public UserService GetService(
            IAppDbContext db,
            ICurrentUserProfileService currentUserProfileService = null)
        {
            return new UserService(
                db: db,
                currentUserProfile: currentUserProfileService ?? ServiceFactory.CreateCurrentUserProfileService());
        }
    }
}
