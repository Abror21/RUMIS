using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Common;
using Izm.Rumis.Infrastructure.Identity;
using Izm.Rumis.Infrastructure.Tests.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Infrastructure.Tests
{
    public class UserManagerTests
    {
        [Fact]
        public void GetLogins_ReturnsData()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                var userId1 = Guid.NewGuid();
                var userId2 = Guid.NewGuid();

                db.Users.Add(User.Create(userId1));
                db.Users.Add(User.Create(userId2));

                db.IdentityUserLogins.AddRange(
                    new IdentityUserLogin { User = new IdentityUser { Id = userId1 } },
                    new IdentityUserLogin { User = new IdentityUser { Id = userId2 } });

                db.SaveChanges();

                var data = CreateService(db).GetLogins().List();

                Assert.Equal(2, data.Count());
            }
        }

        [Fact]
        public void GetUsers_ReturnsData()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                var userId1 = Guid.NewGuid();
                var userId2 = Guid.NewGuid();

                db.Users.Add(User.Create(userId1));
                db.Users.Add(User.Create(userId2));

                db.IdentityUsers.Add(new IdentityUser { Id = userId1 });
                db.IdentityUsers.Add(new IdentityUser { Id = userId2 });
                db.SaveChanges();

                var data = CreateService(db).GetUsers().List();

                Assert.Equal(2, data.Count());
            }
        }

        [Fact]
        public async Task MustResetPassword_Succeeds()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                var loginId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                const string username = "test";

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    UserName = username,
                    AuthType = UserAuthType.Forms,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });
                db.SaveChanges();

                var result = await CreateService(db).MustResetPassword(username);

                Assert.NotNull(result);
            }
        }

        [Fact]
        public async Task MustResetPassword_Succeeds_Force()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                var loginId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                const string username = "test";

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    UserName = username,
                    PasswordHash = "x",
                    AuthType = UserAuthType.Forms,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });
                db.SaveChanges();

                var result = await CreateService(db).MustResetPassword(username, true);

                var user = await db.IdentityUserLogins.FindAsync(loginId);

                Assert.NotNull(result);
                Assert.Null(user.PasswordHash);
            }
        }

        [Fact]
        public async Task MustResetPassword_ThrowsNotFound()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                {
                    return CreateService(db).MustResetPassword("test");
                });
            }
        }

        [Fact]
        public async Task MustResetPassword_ThrowsNotSupported_Adfs()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                var loginId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                const string username = "test";

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    UserName = username,
                    AuthType = UserAuthType.Adfs,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });
                db.SaveChanges();

                await Assert.ThrowsAsync<NotSupportedException>(() =>
                {
                    return CreateService(db).MustResetPassword(username);
                });
            }
        }

        [Fact]
        public async Task MustResetPassword_ThrowsNotSupported_AdminUpdate()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                var loginId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                const string username = UserNames.Admin;

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    UserName = username,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });
                db.SaveChanges();

                await Assert.ThrowsAsync<NotSupportedException>(() =>
                {
                    return CreateService(db).MustResetPassword(username);
                });
            }
        }

        [Fact]
        public async Task ChangePassword_Succeeds()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                var loginId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                const string username = "test";

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    AuthType = UserAuthType.Forms,
                    UserName = username,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });

                db.SaveChanges();

                await CreateService(db).ChangePassword(username, "p");

                var item = db.IdentityUserLogins.FirstOrDefault(t => t.Id == loginId);

                Assert.NotNull(item.PasswordHash);
                Assert.Null(item.PasswordResetKey);
                Assert.False(item.MustResetPassword);
            }
        }

        [Fact]
        public async Task ResetPassword_Succeeds()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const string resetKey = "k";

                var loginId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    MustResetPassword = true,
                    PasswordResetKey = resetKey,
                    AuthType = UserAuthType.Forms,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });

                db.SaveChanges();

                var result = await CreateService(db).ResetPassword(resetKey, "123");

                Assert.Equal(loginId, result);
            }
        }

        [Fact]
        public async Task ChangePassword_ThrowsNotFound()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                {
                    return CreateService(db).ChangePassword("t", "p");
                });
            }
        }

        //[Fact]
        //public async Task ChangePassword_ThrowsNotSupported_AdminUpdate()
        //{
        //    using (var db = ServiceFactory.ConnectDb())
        //    {
        //        var userId = Guid.NewGuid();
        //        const string username = UserNames.Admin;

        //        db.IdentityUserLogins.Add(new IdentityUserLogin
        //        {
        //            Id = userId,
        //            UserName = username,
        //            User = new IdentityUser()
        //        });
        //        db.SaveChanges();

        //        await Assert.ThrowsAsync<NotSupportedException>(() =>
        //        {
        //            return CreateService(db).ChangePassword(username, "p");
        //        });
        //    }
        //}

        [Fact]
        public async Task ChangePassword_ThrowsNotSupported_Adfs()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                var loginId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                const string username = "test";

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    UserName = username,
                    AuthType = UserAuthType.Adfs,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });
                db.SaveChanges();

                await Assert.ThrowsAsync<NotSupportedException>(() =>
                {
                    return CreateService(db).ChangePassword(username, "p");
                });
            }
        }

        [Fact]
        public async Task ResetPassword_ThrowsArgument_Secret()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() =>
                {
                    return CreateService(db).ResetPassword(null, "p");
                });
            }
        }

        [Fact]
        public async Task ResetPassword_ThrowsNotFound()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                {
                    return CreateService(db).ResetPassword("a", "b");
                });
            }
        }

        [Fact]
        public async Task ResetPassword_ThrowsValidation_Disabled()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const string resetKey = "k";

                var loginId = Guid.NewGuid();

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    MustResetPassword = false,
                    PasswordResetKey = resetKey
                });
                db.SaveChanges();

                await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                {
                    return CreateService(db).ResetPassword("a", resetKey);
                });
            }
        }

        [Fact]
        public async Task ResetPassword_ThrowsNotSupported_AdminUpdate()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const string resetKey = "k";

                var loginId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    UserName = UserNames.Admin,
                    PasswordResetKey = resetKey,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });
                db.SaveChanges();

                await Assert.ThrowsAsync<NotSupportedException>(() =>
                {
                    return CreateService(db).ResetPassword(resetKey, "p");
                });
            }
        }

        [Fact]
        public async Task ResetPassword_ThrowsNotSupported_Adfs()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const string resetKey = "k";

                var loginId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    UserName = "u1",
                    PasswordResetKey = resetKey,
                    AuthType = UserAuthType.Adfs,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });
                db.SaveChanges();

                await Assert.ThrowsAsync<NotSupportedException>(() =>
                {
                    return CreateService(db).ResetPassword(resetKey, "p");
                });
            }
        }

        [Fact]
        public async Task ChangePassword_ThrowsValidation_Requirements()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                var userId = Guid.NewGuid();
                const string username = "test";

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    UserName = username,
                    AuthType = UserAuthType.Forms,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });

                db.SaveChanges();

                await Assert.ThrowsAsync<ValidationException>(() =>
                {
                    return CreateService(db, new PasswordSettings
                    {
                        MinLength = 0,
                        MaxLength = 99,
                        RequireUpper = true
                    }).ChangePassword(username, "p");
                });
            }
        }

        [Fact]
        public async Task ResetPassword_ThrowsValidation_Requirements()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const string resetKey = "k";

                var loginId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    PasswordResetKey = resetKey,
                    AuthType = UserAuthType.Forms,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });

                db.SaveChanges();

                await Assert.ThrowsAsync<ValidationException>(() =>
                {
                    return CreateService(db, new PasswordSettings
                    {
                        MinLength = 0,
                        MaxLength = 99,
                        RequireUpper = true
                    }).ResetPassword(resetKey, "p");
                });
            }
        }

        [Fact]
        public async Task VerifyPassword_ReturnsTrue()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const string resetKey = "k";
                const string userName = "u";
                const string password = "p";

                var loginId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    UserName = userName,
                    PasswordResetKey = resetKey,
                    MustResetPassword = true,
                    AuthType = UserAuthType.Forms,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });

                db.SaveChanges();

                var service = CreateService(db);

                await service.ResetPassword(resetKey, password);

                var result = await CreateService(db).VerifyPassword(userName, password);

                Assert.True(result);
            }
        }

        //[Fact]
        //public async Task VerifyPasswordByUserId_ReturnsTrue()
        //{
        //    using (var db = ServiceFactory.ConnectDb())
        //    {
        //        const string resetKey = "k";
        //        const string userName = "u";
        //        const string password = "p";

        //        var loginId = Guid.NewGuid();

        //        db.UserLogins.Add(new UserLogin 
        //        { 
        //            Id = loginId, 
        //            UserName = userName, 
        //            PasswordResetKey = resetKey, 
        //            MustResetPassword = true, 
        //            AuthType = UserAuthType.Forms 
        //        });
        //        db.SaveChanges();

        //        var service = CreateService(db);

        //        await service.ResetPassword(resetKey, password);

        //        var result = await service.VerifyPassword(loginId, password);

        //        Assert.True(result);
        //    }
        //}

        [Fact]
        public async Task VerifyPassword_ReturnsFalse_UserNotFound()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                var result = await CreateService(db).VerifyPassword("u", "p");
                Assert.False(result);
            }
        }

        //[Fact]
        //public async Task VerifyPasswordByUserId_ReturnsFalse_UserNotFound()
        //{
        //    using (var db = ServiceFactory.ConnectDb())
        //    {
        //        var result = await CreateService(db).VerifyPassword(Guid.NewGuid(), "p");
        //        Assert.False(result);
        //    }
        //}

        [Fact]
        public async Task VerifyPassword_ThrowsNotSupported_Adfs()
        {
            using (var db = ServiceFactory.ConnectDb())
            {
                const string resetKey = "k";
                const string userName = "u";
                const string password = "p";

                var loginId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                db.Users.Add(User.Create(userId));

                db.IdentityUserLogins.Add(new IdentityUserLogin
                {
                    Id = loginId,
                    UserName = userName,
                    PasswordResetKey = resetKey,
                    MustResetPassword = true,
                    AuthType = UserAuthType.Adfs,
                    User = new IdentityUser
                    {
                        Id = userId
                    }
                });
                db.SaveChanges();

                await Assert.ThrowsAsync<NotSupportedException>(() =>
                {
                    return CreateService(db).VerifyPassword(userName, password);
                });
            }
        }

        private UserManager CreateService(AppDbContext db, PasswordSettings passwordSettings = null)
        {
            return new UserManager(db, new PasswordValidator(passwordSettings ?? new PasswordSettings
            {
                MinLength = 0,
                MaxLength = 99
            }));
        }
    }
}
