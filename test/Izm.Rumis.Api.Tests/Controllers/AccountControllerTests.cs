using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Services;
using Izm.Rumis.Api.Tests.Setup.Options;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public class AccountControllerTests
    {
        private AccountController controller;
        private AuthSettingsFake authOptions;
        private AuthUserProfileOptionsFake userProfileOptions;
        private CurrentUserServiceFake currentUser;
        private IPasswordManager passwordManager;
        private UserProfileServiceFake userProfileService;
        private GdprAuditServiceFake gdprAuditService;

        public AccountControllerTests()
        {
            currentUser = new CurrentUserServiceFake();
            passwordManager = new PasswordManagerFake();
            userProfileService = new UserProfileServiceFake();
            authOptions = new AuthSettingsFake();
            userProfileOptions = new AuthUserProfileOptionsFake();
            gdprAuditService = new GdprAuditServiceFake();

            controller = new AccountController(
                authOptions: authOptions,
                userProfileOptions: userProfileOptions,
                currentUser: currentUser,
                passwordManager: passwordManager,
                userProfileService: userProfileService,
                gdprAuditService: gdprAuditService
                );

            controller.ControllerContext.HttpContext = new DefaultHttpContext();
        }

        [Fact]
        public async Task GetProfiles_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            db.UserProfiles.Add(CreateActiveUserProfile());

            await db.SaveChangesAsync();

            userProfileService.CurrentUserProfiles = db.UserProfiles.AsQueryable();

            // Act
            var result = await controller.GetProfiles();

            // Assert
            Assert.Single(result.Value);
            Assert.True(userProfileService.GetCurrentUserProfilesCalled);
            Assert.Equal(gdprAuditService.TraceRangeAsyncCalledWith.Count(), result.Value.Count());
        }

        [Fact]
        public async Task GetProfiles_Succeeds_FilterDisabled()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var userProfile = CreateActiveUserProfile();

            userProfile.Disable();

            db.UserProfiles.Add(userProfile);

            await db.SaveChangesAsync();

            userProfileService.CurrentUserProfiles = db.UserProfiles.AsQueryable();

            // Act
            var result = await controller.GetProfiles();

            // Assert
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetProfiles_Succeeds_FilterExpired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var userProfile = CreateActiveUserProfile();

            userProfile.SetExpiration(DateTime.UtcNow.AddDays(-1));

            db.UserProfiles.Add(userProfile);

            await db.SaveChangesAsync();

            userProfileService.CurrentUserProfiles = db.UserProfiles.AsQueryable();

            // Act
            var result = await controller.GetProfiles();

            // Assert
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task SetProfile_CheckData()
        {
            // Assign
            var profileId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            var roles = new List<Role>()
            {
                new Role
                {
                    Code = "someCode",
                    Name = "someName",
                    Permissions = new List<RolePermission>()
                    {
                        new RolePermission
                        {
                            Value = "someValue"
                        },
                        new RolePermission
                        {
                            Value = "someValue"
                        }
                    }
                },
                 new Role
                {
                    Code = "someCode1",
                    Name = "someName1",
                    Permissions = new List<RolePermission>()
                    {
                        new RolePermission
                        {
                            Value = "someValue"
                        },
                        new RolePermission
                        {
                            Value = "someValue1"
                        }
                    }
                },
            };

            var userProfile = UserProfile.Create();
            userProfile.Id = profileId;
            userProfile.UserId = currentUser.Id;

            userProfile.SetExpiration(DateTime.MaxValue);
            userProfile.SetRoles(roles);

            userProfile.Enable();

            db.UserProfiles.Add(userProfile);

            await db.SaveChangesAsync();

            userProfileService.CurrentUserProfiles = db.UserProfiles.AsQueryable();

            var session = ServiceFactory.CreateSession();

            session.Set(SessionKey.Created, new byte[] { 1, 2, 3 });

            controller.HttpContext.Session = session;

            // Act
            var response = await controller.SetProfile(profileId);

            var result = response.Value;

            // Assert
            Assert.NotNull(result.Token);
            Assert.Equal(roles.Count, result.Roles.Count());
            Assert.Equal(roles.SelectMany(t => t.Permissions).Distinct().Count(), result.Permissions.Count());
        }

        [Fact]
        public async Task SetProfile_InvalidProfileId_ProfileDoesNotExist()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            userProfileService.CurrentUserProfiles = db.UserProfiles.AsQueryable();

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => controller.SetProfile(Guid.NewGuid()));

            // Assert
            Assert.Equal(AccountController.Error.InvalidUserProfileId, result.Message);
        }

        [Fact]
        public async Task SetProfile_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var userProfile = CreateSetProfileValidUserProfile(currentUser.Id);

            db.UserProfiles.Add(userProfile);

            await db.SaveChangesAsync();

            userProfileService.CurrentUserProfiles = db.UserProfiles.AsQueryable();

            var session = ServiceFactory.CreateSession();

            session.Set(SessionKey.Created, new byte[] { 1, 2, 3 });

            controller.HttpContext.Session = session;

            // Act
            var result = await controller.SetProfile(userProfile.Id);

            // Assert
            Assert.NotNull(result.Value);
            Assert.True(userProfileService.GetCurrentUserProfilesCalled);
        }

        [Fact]
        public async Task SetProfile_Throws_CannotSetDisabledProfile()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var userProfile = CreateSetProfileValidUserProfile(currentUser.Id);

            userProfile.Disable();

            db.UserProfiles.Add(userProfile);

            await db.SaveChangesAsync();

            userProfileService.CurrentUserProfiles = db.UserProfiles.AsQueryable();

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => controller.SetProfile(userProfile.Id));

            // Assert
            Assert.Equal(AccountController.Error.CannotSetDisabledProfile, result.Message);
        }

        [Fact]
        public async Task SetProfile_Throws_CannotSetExpiredProfile()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var userProfile = CreateSetProfileValidUserProfile(currentUser.Id);

            userProfile.SetExpiration(DateTime.MinValue);

            db.UserProfiles.Add(userProfile);

            await db.SaveChangesAsync();

            userProfileService.CurrentUserProfiles = db.UserProfiles.AsQueryable();

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => controller.SetProfile(userProfile.Id));

            // Assert
            Assert.Equal(AccountController.Error.CannotSetExpiredProfile, result.Message);
        }

        [Fact]
        public async Task SetProfile_Throws_InvalidUserProfileId()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            userProfileService.CurrentUserProfiles = db.UserProfiles.AsQueryable();

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => controller.SetProfile(Guid.NewGuid()));

            // Assert
            Assert.Equal(AccountController.Error.InvalidUserProfileId, result.Message);
        }

        [Fact]
        public async Task ResetPassword_ReturnsNotFound()
        {
            var result = await controller.ResetPassword(new AccountPasswordResetModel());
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ResetPassword_ReturnsNoContent()
        {
            authOptions.Settings.FormsEnabled = true;

            var result = await controller.ResetPassword(new AccountPasswordResetModel());

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ChangePassword_ReturnsNotFound()
        {
            var result = await controller.ChangePassword(new AccountPasswordChangeModel());
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ChangePassword_ReturnsNoContent()
        {
            authOptions.Settings.FormsEnabled = true;

            var result = await controller.ChangePassword(new AccountPasswordChangeModel());

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ChangePassword_ThrowsValidation()
        {
            authOptions.Settings.FormsEnabled = true;

            var result = await controller.ChangePassword(new AccountPasswordChangeModel());

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RecoverPassword_ReturnsNotFound()
        {
            var result = await controller.RecoverPassword(new AccountPasswordRecoverModel());
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RecoverPassword_ReturnsNoContent()
        {
            const string email = "x";

            authOptions.Settings.FormsEnabled = true;

            var result = await controller.RecoverPassword(new AccountPasswordRecoverModel { Email = email });

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RecoverPassword_ThrowsValidation()
        {
            authOptions.Settings.FormsEnabled = true;

            var result = await controller.RecoverPassword(new AccountPasswordRecoverModel());

            Assert.IsType<NoContentResult>(result);
        }

        private static UserProfile CreateSetProfileValidUserProfile(Guid? userId = null)
        {
            var userProfile = UserProfile.Create();
            userProfile.Id = Guid.NewGuid();
            userProfile.UserId = userId ?? Guid.NewGuid();

            userProfile.SetExpiration(DateTime.MaxValue);

            userProfile.Enable();

            return userProfile;
        }

        private static UserProfile CreateActiveUserProfile()
        {
            var userProfile = UserProfile.Create();
            userProfile.UserId = Guid.NewGuid();
            userProfile.User = User.Create();

            userProfile.SetExpiration(DateTime.UtcNow.AddDays(1));

            return userProfile;
        }
    }
}
