using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public sealed class UserProfilesControllerTests
    {
        private UserProfileController controller;
        private UserProfileServiceFake userProfileServiceFake;
        private GdprAuditServiceFake gdprAuditServiceFake;

        public UserProfilesControllerTests()
        {
            userProfileServiceFake = new UserProfileServiceFake();
            gdprAuditServiceFake = new GdprAuditServiceFake();

            controller = new UserProfileController(userProfileServiceFake, gdprAuditServiceFake);
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            // Act
            var result = await controller.Create(new UserProfileEditRequest());

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Delete_Succeeds()
        {
            // Act
            var result = await controller.Delete(Guid.NewGuid());

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Get_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            db.UserProfiles.AddRange(new List<UserProfile>
            {
                CreateUserProfile(),
                CreateUserProfile()
            });

            await db.SaveChangesAsync();

            userProfileServiceFake.UserProfiles = db.UserProfiles.AsQueryable();

            // Act
            var result = await controller.Get();

            // Assert
            Assert.Equal(userProfileServiceFake.UserProfiles.Count(), result.Value.Total);
            Assert.NotNull(gdprAuditServiceFake.TraceRangeAsyncCalledWith);
        }

        [Fact]
        public async Task GetById_NotFound()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            userProfileServiceFake.UserProfiles = db.UserProfiles.AsQueryable();

            var result = await controller.GetById(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetById_Succeeds()
        {
            // Assign
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.UserProfiles.Add(CreateUserProfile(id));

            await db.SaveChangesAsync();

            userProfileServiceFake.UserProfiles = db.UserProfiles.AsQueryable();

            var result = await controller.GetById(id);

            // Assert
            Assert.NotNull(result.Value);
            Assert.NotNull(gdprAuditServiceFake.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            // Act
            var result = await controller.Update(Guid.NewGuid(), new UserProfileEditRequest());

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        private static UserProfile CreateUserProfile(Guid? id = null)
        {
            var userProfile = UserProfile.Create();

            userProfile.Id = id ?? Guid.NewGuid();
            userProfile.User = User.Create();

            return userProfile;
        }
    }
}
