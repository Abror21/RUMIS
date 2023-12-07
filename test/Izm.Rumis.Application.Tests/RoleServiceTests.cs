using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public sealed class RoleServiceTests
    {
        [Fact]
        public async Task CreateAsync_Succeeds()
        {
            // Assign
            var model = new RoleEditDto
            {
                Code = "someCode",
                Name = "someName",
                Permissions = Permission.All
            };

            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act
            var result = await service.CreateAsync(model);

            // Assert
            Assert.True(db.Roles.Any());
            Assert.True(db.RolePermissions.Any());
        }

        [Fact]
        public async Task CreateAsync_CheckData()
        {
            // Assign

            var dto = new RoleEditDto
            {
                Code = "someCode",
                Name = "someName",
                Permissions = Permission.All
            };

            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act
            var result = await service.CreateAsync(dto);

            var role = db.Roles
                .Include(t => t.Permissions)
                .First();

            // Assert
            Assert.Equal(dto.Code, role.Code);
            Assert.Equal(dto.Name, role.Name);
            Assert.True(!dto.Permissions.Except(role.Permissions.Select(t => t.Value)).Any()
                && !role.Permissions.Select(t => t.Value).Except(dto.Permissions).Any());
        }

        [Fact]
        public async Task DeleteAsync_Succeeds()
        {
            using var db = ServiceFactory.ConnectDb();

            await db.Roles.AddAsync(new Role
            {
                Code = "someCode",
                Name = "someName"
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.DeleteAsync(1);

            // Assert
            Assert.Empty(db.Roles);
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
        public async Task Get_Succeeds()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var roles = new List<Role>()
            {
                new Role
                {
                    Code = "someCode",
                    Name = "someName"
                },
                new Role
                {
                    Code = "someCode",
                    Name = "someName"
                }
            };

            await db.Roles.AddRangeAsync(roles);

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            var result = service.Get();

            // Assert
            Assert.Equal(roles.Count, result.Count());
        }

        [Fact]
        public async Task UpdateAsync_Succeeds()
        {
            // Assing
            const int id = 1;

            var dto = new RoleEditDto
            {
                Code = "updatedCode",
                Name = "updatedName",
                Permissions = Permission.All
            };

            using var db = ServiceFactory.ConnectDb();

            await db.Roles.AddAsync(new Role
            {
                Id = id,
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
                        Value = "oneMoreValue"
                    },
                }
            });

            await db.SaveChangesAsync();

            var service = GetService(db);

            // Act
            await service.UpdateAsync(id, dto);

            var role = db.Roles.First();

            // Assert
            Assert.Equal(dto.Code, role.Code);
            Assert.Equal(dto.Name, role.Name);
            Assert.Equal(dto.Permissions.Count(), role.Permissions.Count);
            Assert.True(!dto.Permissions.Except(role.Permissions.Select(t => t.Value)).Any()
                && !role.Permissions.Select(t => t.Value).Except(dto.Permissions).Any());
        }

        [Fact]
        public async Task UpdateAsync_Throws_EntityNotFound()
        {
            // Assing
            using var db = ServiceFactory.ConnectDb();

            var service = GetService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.UpdateAsync(1, new RoleEditDto()));
        }

        private RoleService GetService(IAppDbContext db)
        {
            return new RoleService(
                db: db
                );
        }
    }
}
