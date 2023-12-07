using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class SupervisorServiceTests
    {
        [Fact]
        public async Task CreateAsync_Succeeds()
        {
            // Assign
            var model = new SupervisorCreateDto
            {
                Code = "someCode",
                Name = "someName"
            };

            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act
            var result = await service.CreateAsync(model);

            // Assert
            Assert.True(db.Supervisors.Any());
        }

        [Fact]
        public async Task CreateAsync_CheckData()
        {
            // Assign
            const string code = "someCode";
            const string name = "someName";

            var model = new SupervisorCreateDto
            {
                Code = code,
                Name = name
            };

            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act
            var result = await service.CreateAsync(model);

            var supervisor = db.Supervisors.First();

            // Assert
            Assert.Equal(code, supervisor.Code);
            Assert.Equal(name, supervisor.Name);
        }

        [Fact]
        public async Task DeleteAsync_Succeeds()
        {
            // Assing
            const int id = 1;

            using var db = ServiceFactory.ConnectDb();

            await db.Supervisors.AddAsync(new Supervisor
            {
                Id = id,
                Code = "someCode",
                Name = "someName"
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.DeleteAsync(1);

            // Assert
            Assert.Empty(db.Supervisors);
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

            var supervisors = new List<Supervisor>()
            {
                new Supervisor
                {
                    Code = "someCode",
                    Name = "someName"
                },
                new Supervisor
                {
                    Code = "someCode",
                    Name = "someName"
                }
            };

            db.Supervisors.AddRange(supervisors);

            await db.SaveChangesAsync();

            var currentUserProfile = ServiceFactory.CreateCurrentUserProfileService();

            var service = GetService(db);

            // Act
            var result = service.Get();

            // Assert
            Assert.Equal(supervisors.Count, result.Count());
        }

        //[Fact]
        //public async Task Get_Succeeds_SupervisorProfileType()
        //{
        //    // Assign
        //    const int id = 1;

        //    using var db = ServiceFactory.ConnectDb();

        //    db.Supervisors.AddRange(new List<Supervisor>()
        //    {
        //        new Supervisor
        //        {
        //            Id = id,
        //            Code = "someCode",
        //            Name = "someName"
        //        },
        //        new Supervisor
        //        {
        //            Id = id + 1,
        //            Code = "someCode",
        //            Name = "someName"
        //        }
        //    });

        //    await db.SaveChangesAsync();

        //    var currentUserProfile = ServiceFactory.CreateCurrentUserProfileService();

        //    currentUserProfile.Type = UserProfileType.Supervisor;
        //    currentUserProfile.SupervisorId = id;

        //    var service = GetService(db, currentUserProfile);

        //    // Act
        //    var result = service.Get();

        //    // Assert
        //    Assert.Single(result.List());
        //}

        //[Fact]
        //public async Task Get_Throws_UnauthorizedAccess()
        //{
        //    // Assign
        //    using var db = ServiceFactory.ConnectDb();

        //    var currentUserProfile = ServiceFactory.CreateCurrentUserProfileService();

        //    currentUserProfile.Type = UserProfileType.EducationalInstitution;
        //    currentUserProfile.EducationalInstitutionId = 1;

        //    var service = GetService(db, currentUserProfile);

        //    // Act & Assert
        //    Assert.Throws<UnauthorizedAccessException>(() => service.Get());
        //}

        [Fact]
        public async Task UpdateAsync_Succeeds()
        {
            // Assing
            const int id = 1;

            var dto = new SupervisorUpdateDto
            {
                Code = "someCode",
                Name = "updatedName"
            };

            using var db = ServiceFactory.ConnectDb();

            await db.Supervisors.AddAsync(new Supervisor
            {
                Id = id,
                Code = "someCode",
                Name = "someName"
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.UpdateAsync(1, dto);

            var supervisor = db.Supervisors.First();

            // Assert
            Assert.Equal(dto.Code, supervisor.Code);
            Assert.Equal(dto.Name, supervisor.Name);
        }

        [Fact]
        public async Task UpdateAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.UpdateAsync(1, new SupervisorUpdateDto()));
        }

        private SupervisorService GetService(IAppDbContext db)
        {
            return new SupervisorService(
                db: db
                );
        }
    }
}
