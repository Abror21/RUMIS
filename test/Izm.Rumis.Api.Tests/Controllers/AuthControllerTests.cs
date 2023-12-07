using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Api.Tests.Setup.Options;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Identity;
using Izm.Rumis.Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private AuthController controller;
        private AuthSettingsFake options;
        private DistributedCacheFake cache;
        private UserManagerFake userManager;
        private LoggerFake<AuthController> logger;
        private GdprAuditServiceFake gdprAuditService;
        private WebHostEnvironmentFake environment;

        public AuthControllerTests()
        {
            options = new AuthSettingsFake();
            cache = new DistributedCacheFake();
            userManager = new UserManagerFake();
            logger = new LoggerFake<AuthController>();
            gdprAuditService = new GdprAuditServiceFake();
            environment = new WebHostEnvironmentFake();

            controller = new AuthController(cache, userManager, logger, options, gdprAuditService, environment);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
        }

        [Fact]
        public void Configuration_ReturnsData()
        {
            var result = controller.Configuration(new PasswordSettings());
            var data = result.Value;

            Assert.NotNull(data);
        }

        [Fact]
        public async Task Login_ByInvalidCode_Throws()
        {
            var user = new IdentityUserLogin
            {
                UserName = "u1",
                AuthType = UserAuthType.Forms
            };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
            {
                return controller.Login(new AuthLoginModel
                {
                    Code = "anycode"
                }, ServiceFactory.CreateSessionManager(), ServiceFactory.CreateSessionService());
            });

            Assert.Equal("auth.loginFailed", ex.Message);
        }

        [Fact]
        public async Task Login_ByCodeInvalidAuthType_Throws()
        {
            const string code = "c1";

            var user = new IdentityUserLogin
            {
                Id = Guid.Parse("0766b6ea-9814-46bc-81ca-fbb634e58f1b"),
                UserName = "u1",
                AuthType = UserAuthType.Forms
            };

            cache.SetString(code, user.Id.ToString());

            var ex = await Assert.ThrowsAsync<Exception>(() =>
            {
                return controller.Login(new AuthLoginModel
                {
                    Code = code
                }, ServiceFactory.CreateSessionManager(), ServiceFactory.CreateSessionService());
            });

            Assert.Equal("auth.loginFailed", ex.Message);
        }

        [Fact]
        public async Task Login_ByCodeNoUser_Throws()
        {
            const string code = "c1";

            var user = new IdentityUserLogin
            {
                Id = Guid.Parse("0766b6ea-9814-46bc-81ca-fbb634e58f1b"),
                UserName = "u1",
                AuthType = UserAuthType.Adfs
            };

            cache.SetString(code, Guid.Parse("399d8ab0-d167-48ac-bfe1-9deee2152ead").ToString());

            var ex = await Assert.ThrowsAsync<Exception>(() =>
            {
                return controller.Login(new AuthLoginModel
                {
                    Code = code
                }, ServiceFactory.CreateSessionManager(), ServiceFactory.CreateSessionService());
            });

            Assert.Equal("auth.loginFailed", ex.Message);
        }

        [Fact]
        public async Task Login_ByCode_Succeeds()
        {
            // Assign
            const string code = "x";
            var loginId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Users.Add(User.Create(userId));

            db.IdentityUserLogins.Add(new IdentityUserLogin
            {
                Id = loginId,
                UserName = "adfs",
                AuthType = UserAuthType.Adfs,
                UserId = userId,
                User = new IdentityUser
                {
                    Id = userId
                }
            });

            db.PersonTechnicals.Add(new PersonTechnical
            {
                UserId = userId
            });

            await db.SaveChangesAsync();

            options.Settings.ExternalEnabled = true;

            userManager.UserLogins = db.IdentityUserLogins.AsQueryable();
            userManager.Users = db.IdentityUsers.AsQueryable();

            cache.SetString(code, userId.ToString());

            var session = ServiceFactory.CreateSession();
            controller.HttpContext.Session = session;

            var sessionManager = ServiceFactory.CreateSessionManager();
            var sessionService = ServiceFactory.CreateSessionService();

            // Act
            var result = await controller.Login(new AuthLoginModel
            {
                Code = code
            }, sessionManager, sessionService);

            var model = result.Value;

            // Assert
            Assert.Equal(userId, model.Id);
            Assert.Equal(session.Id, sessionManager.AddActivityTraceCalledWith);
            Assert.Equal(session.Id, sessionService.CreateCalledWith.Id.ToString());
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task Login_ByUserNameInvalidAuthType_Throws()
        {
            const string username = "u1";

            var user = new IdentityUserLogin
            {
                UserName = username,
                AuthType = UserAuthType.Adfs
            };

            userManager.VerifyPasswordResult = true;

            var ex = await Assert.ThrowsAsync<Exception>(() =>
            {
                return controller.Login(new AuthLoginModel
                {
                    UserName = username,
                    Password = "pwd"
                }, ServiceFactory.CreateSessionManager(), ServiceFactory.CreateSessionService());
            });

            Assert.Equal("auth.loginFailed", ex.Message);
        }

        [Fact]
        public async Task Login_ByUserNameNoUser_Throws()
        {
            const string username = "u1";

            var user = new IdentityUserLogin
            {
                UserName = username,
                AuthType = UserAuthType.Forms
            };

            userManager.VerifyPasswordResult = true;

            var ex = await Assert.ThrowsAsync<Exception>(() =>
            {
                return controller.Login(new AuthLoginModel
                {
                    UserName = "u2",
                    Password = "pwd"
                }, ServiceFactory.CreateSessionManager(), ServiceFactory.CreateSessionService());
            });

            Assert.Equal("auth.loginFailed", ex.Message);
        }

        [Fact]
        public async Task Login_ByUserNameWrongPassword_Throws()
        {
            const string username = "u1";

            var user = new IdentityUserLogin
            {
                UserName = username,
                AuthType = UserAuthType.Forms
            };

            userManager.VerifyPasswordResult = false;

            var ex = await Assert.ThrowsAsync<Exception>(() =>
            {
                return controller.Login(new AuthLoginModel
                {
                    UserName = username,
                    Password = "pwd"
                }, ServiceFactory.CreateSessionManager(), ServiceFactory.CreateSessionService());
            });

            Assert.Equal("auth.loginFailed", ex.Message);
        }

        [Fact]
        public async Task Login_ByUserName_Succeeds()
        {
            const string userName = "user";

            var userId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Users.Add(User.Create(userId));

            db.IdentityUserLogins.Add(new IdentityUserLogin
            {
                UserName = userName,
                AuthType = UserAuthType.Forms,
                User = new IdentityUser
                {
                    Id = userId
                }
            });

            db.PersonTechnicals.Add(new PersonTechnical
            {
                UserId = userId
            });

            await db.SaveChangesAsync();

            options.Settings.FormsEnabled = true;

            userManager.UserLogins = db.IdentityUserLogins.AsQueryable();
            userManager.Users = db.IdentityUsers.AsQueryable();
            userManager.VerifyPasswordResult = true;

            var session = ServiceFactory.CreateSession();
            controller.HttpContext.Session = session;

            var sessionManager = ServiceFactory.CreateSessionManager();
            var sessionService = ServiceFactory.CreateSessionService();

            var result = await controller.Login(new AuthLoginModel
            {
                Password = "x",
                UserName = userName
            }, sessionManager, sessionService);

            var model = result.Value;

            // Assert
            Assert.Equal(userId, model.Id);
            Assert.Equal(session.Id, sessionManager.AddActivityTraceCalledWith);
            Assert.Equal(session.Id, sessionService.CreateCalledWith.Id.ToString());
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        [Fact]
        public async Task Login_Inactive_Throws()
        {
            const string userName = "u";
            const string password = "p";

            userManager.UserLogins = new TestAsyncEnumerable<IdentityUserLogin>(new IdentityUserLogin[] {
                new IdentityUserLogin
                {
                    UserName = userName,
                    AuthType = UserAuthType.Forms,
                    User = new IdentityUser()
                }
            });

            userManager.VerifyPasswordResult = true;

            var ex = await Assert.ThrowsAsync<Exception>(() =>
            {
                return controller.Login(new AuthLoginModel
                {
                    Password = password,
                    UserName = userName
                }, ServiceFactory.CreateSessionManager(), ServiceFactory.CreateSessionService());
            });

            Assert.Equal("auth.loginFailed", ex.Message);
        }

        [Fact]
        public async Task Login_Throws_SessionAlreadyActive()
        {
            const string userName = "user";

            var userId = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Users.Add(User.Create(userId));

            db.IdentityUserLogins.Add(new IdentityUserLogin
            {
                UserName = userName,
                AuthType = UserAuthType.Forms,
                User = new IdentityUser
                {
                    Id = userId
                }
            });

            await db.SaveChangesAsync();

            options.Settings.FormsEnabled = true;

            userManager.UserLogins = db.IdentityUserLogins.AsQueryable();
            userManager.Users = db.IdentityUsers.AsQueryable();
            userManager.VerifyPasswordResult = true;

            var session = ServiceFactory.CreateSession();
            session.Keys = new string[] { SessionKey.UserId };

            controller.HttpContext.Session = session;

            var sessionManager = ServiceFactory.CreateSessionManager();
            var sessionService = ServiceFactory.CreateSessionService();

            // Act
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => controller.Login(new AuthLoginModel
            {
                Password = "x",
                UserName = userName
            }, sessionManager, sessionService));

            // Assert
            Assert.Equal(AuthController.Error.SessionAlreadyActive, result.Message);
        }

        //[Fact]
        //public async Task Login_Admin_Succeeds()
        //{
        //    var id = Guid.Parse("0766b6ea-9814-46bc-81ca-fbb634e58f1b");

        //    var user = new IdentityUserLogin
        //    {
        //        UserName = UserNames.Admin,
        //        AuthType = UserAuthType.Forms,
        //        User = new IdentityUser(id)
        //        {
        //            IsActive = true
        //        }
        //    };

        //    userManager.UserLogins = new IdentityUserLogin[] { new IdentityUserLogin(), user };
        //    userManager.Users = new IdentityUser[] { new IdentityUser(), user.User };
        //    userManager.VerifyPasswordResult = true;

        //    var result = await controller.Login(new AuthLoginModel
        //    {
        //        Password = "adminwpd",
        //        UserName = UserNames.Admin
        //    });

        //    var data = result.Value;

        //    Assert.Equal(id, data.Id);
        //    Assert.Equal(UserNames.Admin, data.UserName);
        //}

        [Theory]
        [InlineData(UserAuthType.Adfs, null, null, "code")]
        [InlineData(UserAuthType.Forms, "uname", "pwd", null)]
        public async Task Login_ReturnsData(UserAuthType authType, string username, string password, string code)
        {
            // Assign
            var userId = Guid.NewGuid();
            var loginId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            string adjusteduserName = username ?? "pk";

            using var db = ServiceFactory.ConnectDb();

            db.Users.Add(User.Create(userId));

            await db.IdentityUserLogins.AddAsync(new IdentityUserLogin
            {
                Id = loginId,
                UserName = adjusteduserName,
                AuthType = authType,
                UserId = userId,
                User = new IdentityUser
                {
                    Id = userId
                }
            });

            db.PersonTechnicals.Add(new PersonTechnical
            {
                UserId = userId
            });

            await db.SaveChangesAsync();

            options.Settings.FormsEnabled = true;
            options.Settings.ExternalEnabled = true;

            userManager.UserLogins = db.IdentityUserLogins.AsQueryable();
            userManager.Users = db.IdentityUsers.AsQueryable();
            userManager.VerifyPasswordResult = true;

            if (code != null)
                cache.SetString(code, userId.ToString());

            var session = ServiceFactory.CreateSession();
            controller.HttpContext.Session = session;

            // Act
            var result = await controller.Login(new AuthLoginModel
            {
                Password = password,
                UserName = username,
                Code = code
            }, ServiceFactory.CreateSessionManager(), ServiceFactory.CreateSessionService());

            var data = result.Value;

            // Assert
            Assert.Equal(userId, data.Id);
            Assert.Equal(adjusteduserName, data.UserName);
            Assert.NotNull(data.AccessToken);
            Assert.Equal(authType, data.AuthType);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
            // TODO test if cookies set
            //Assert.Equal(authType.ToString(), controller.Response.Cookies[AuthController.AuthTypeTypeCookieName]);
            //Assert.NotNull(controller.Response.Cookies[AuthController.RefreshTokenCookieName]);
        }

        [Fact]
        public async Task Ticket_UnknownType_ThrowsNotSupported()
        {
            var result = await controller.Ticket(new AuthTicketSignInModel
            {
                Ticket = "{}",
                Type = "x"
            });

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Ticket_ExternalDisabled_ThrowsNotSupported()
        {
            await Assert.ThrowsAsync<NotSupportedException>(() =>
            {
                return controller.Ticket(new AuthTicketSignInModel
                {
                    Ticket = "{}",
                    Type = "adfs"
                });
            });
        }

        [Fact]
        public async Task Ticket_Succeeds()
        {
            const string personCode = "00000000001";

            options.Value.ExternalEnabled = true;

            var claims = new Dictionary<string, string>
            {
                { ClaimTypesExtensions.PrivatePersonalIdentifier, personCode }
            };

            var claimsJson = JsonConvert.SerializeObject(claims.Select(t => new
            {
                Type = t.Key,
                t.Value
            }));

            userManager.UserId = Guid.NewGuid();
            userManager.Users = new List<IdentityUser>
            {
                new IdentityUser
                {
                    Id = userManager.UserId.Value,
                    IsDisabled = false,
                    Logins = new List<IdentityUserLogin>
                    {
                        new IdentityUserLogin
                        {
                            AuthType = UserAuthType.Adfs
                        }
                    }
                }
            }.AsQueryable();

            var ticket = StringCipher.Encrypt(claimsJson, options.Value.TicketPassword);

            var result = await controller.Ticket(new AuthTicketSignInModel
            {
                Ticket = ticket,
                Type = "adfs"
            });

            var data = result.Value;

            Assert.NotEmpty(data.RedirectUrl);
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        //[Fact]
        //public async Task Refresh_Succeeds()
        //{
        //    // Assign
        //    const string username = "u1";
        //    const string token = "refresh";
        //    const UserAuthType authType = UserAuthType.Adfs;

        //    var userId = Guid.NewGuid();
        //    var profileId = Guid.NewGuid();

        //    using var db = ServiceFactory.ConnectDb();

        //    db.Users.Add(User.Create(userId));

        //    await db.IdentityUserLogins.AddAsync(new IdentityUserLogin
        //    {
        //        Id = profileId,
        //        UserName = username,
        //        AuthType = authType,
        //        UserId = userId,
        //        User = new IdentityUser
        //        {
        //            Id = userId
        //        }
        //    });

        //    await db.IdentityRefreshTokens.AddAsync(new IdentityRefreshToken
        //    {
        //        UserLoginId = profileId,
        //        Value = token
        //    });

        //    await db.SaveChangesAsync();

        //    userManager.UserLogins = db.IdentityUserLogins.AsQueryable();
        //    userManager.Users = db.IdentityUsers.AsQueryable();

        //    var partialTokenJson = JsonConvert.SerializeObject(new PartialUserToken
        //    {
        //        Id = userId,
        //        UserName = username,
        //        Permissions = new string[] { }
        //    });

        //    encryption.EncryptResult = "enc";
        //    encryption.DecryptResult = partialTokenJson;

        //    controller.Request.Cookies = new RequestCookieCollectionFake(
        //        new KeyValuePair<string, string>(AuthController.RefreshTokenCookieName, token),
        //        new KeyValuePair<string, string>(AuthController.AuthTypeCookieName, authType.ToString()),
        //        new KeyValuePair<string, string>(AuthController.PartialTokenCookieName, "pt")
        //    );

        //    // Act
        //    var result = await controller.Refresh();

        //    // Assert
        //    Assert.IsAssignableFrom<AuthUserResponse>(result.Value);
        //}

        //[Theory]
        //[InlineData(UserAuthType.Adfs, UserAuthType.Adfs, "a", "b")]
        //[InlineData(UserAuthType.Forms, UserAuthType.Forms, "a", "b")]
        //[InlineData(UserAuthType.Adfs, UserAuthType.Forms, "c", "c")]
        //[InlineData(UserAuthType.Forms, UserAuthType.Adfs, "c", "c")]
        //public async Task Refresh_NoUser_Throws(UserAuthType userAuth, UserAuthType cookieAuth, string userToken, string cookieToken)
        //{
        //    // Assign
        //    var profileId = Guid.NewGuid();
        //    var userId = Guid.NewGuid();
        //    using var db = ServiceFactory.ConnectDb();

        //    db.Users.Add(User.Create(userId));

        //    await db.IdentityUserLogins.AddAsync(new IdentityUserLogin
        //    {
        //        Id = profileId,
        //        UserName = "someUserName",
        //        AuthType = userAuth,
        //        UserId = userId,
        //        User = new IdentityUser
        //        {
        //            Id = userId
        //        }
        //    });

        //    await db.IdentityRefreshTokens.AddAsync(new IdentityRefreshToken
        //    {
        //        UserLoginId = profileId,
        //        Value = userToken
        //    });

        //    await db.SaveChangesAsync();

        //    userManager.UserLogins = db.IdentityUserLogins.AsQueryable();

        //    var partialTokenJson = JsonConvert.SerializeObject(new PartialUserToken
        //    {
        //        Id = userId,
        //        Permissions = new string[] { }
        //    });

        //    encryption.EncryptResult = "enc";
        //    encryption.DecryptResult = partialTokenJson;

        //    controller.Request.Cookies = new RequestCookieCollectionFake(
        //        new KeyValuePair<string, string>(AuthController.RefreshTokenCookieName, cookieToken),
        //        new KeyValuePair<string, string>(AuthController.AuthTypeCookieName, cookieAuth.ToString()),
        //        new KeyValuePair<string, string>(AuthController.PartialTokenCookieName, "pt")
        //    );

        //    var ex = await Assert.ThrowsAsync<Exception>(() =>
        //    {
        //        return controller.Refresh();
        //    });

        //    Assert.Equal("user.notFound", ex.Message);
        //}

        //[Fact]
        //public async Task Refresh_Inactive_Throws()
        //{
        //    // Assign
        //    var loginId = Guid.NewGuid();
        //    const string userName = "u";
        //    const string tokenCookie = "x";

        //    var roleId = Guid.NewGuid();
        //    var userId = Guid.NewGuid();

        //    var partialTokenJson = JsonConvert.SerializeObject(new PartialUserToken
        //    {
        //        Id = userId,
        //        Permissions = new string[] { }
        //    });

        //    encryption.EncryptResult = "enc";
        //    encryption.DecryptResult = partialTokenJson;

        //    controller.Request.Cookies = new RequestCookieCollectionFake(
        //        new KeyValuePair<string, string>(AuthController.RefreshTokenCookieName, tokenCookie),
        //        new KeyValuePair<string, string>(AuthController.AuthTypeCookieName, UserAuthType.Forms.ToString()),
        //        new KeyValuePair<string, string>(AuthController.PartialTokenCookieName, "pt")
        //    );

        //    using var db = ServiceFactory.ConnectDb();

        //    db.Users.Add(User.Create(userId));

        //    await db.IdentityUserLogins.AddAsync(new IdentityUserLogin
        //    {
        //        Id = loginId,
        //        UserName = userName,
        //        AuthType = UserAuthType.Forms,
        //        User = new IdentityUser
        //        {
        //            Id = userId,
        //            IsDisabled = true
        //        }
        //    });

        //    await db.IdentityRefreshTokens.AddAsync(new IdentityRefreshToken
        //    {
        //        UserLoginId = loginId,
        //        Value = tokenCookie
        //    });

        //    await db.SaveChangesAsync();

        //    userManager.UserLogins = db.IdentityUserLogins.AsQueryable();

        //    // Act & Assert
        //    var ex = await Assert.ThrowsAsync<Exception>(() =>
        //    {
        //        return controller.Refresh();
        //    });

        //    // Assert
        //    Assert.Equal("user.notFound", ex.Message);
        //}

        [Fact]
        public async Task Logout_External_Succeeds()
        {
            const UserAuthType authType = UserAuthType.Adfs;

            controller.Request.Cookies = new RequestCookieCollectionFake(
                //new KeyValuePair<string, string>(AuthController.RefreshTokenCookieName, "x"),
                new KeyValuePair<string, string>(AuthController.AuthTypeCookieName, authType.ToString())
            );

            var session = ServiceFactory.CreateSession();
            controller.HttpContext.Session = session;

            var sessionService = ServiceFactory.CreateSessionService();

            var result = await controller.Logout(sessionService);
            var data = result.Value;

            // Assert
            Assert.True(session.ClearCalled);
            Assert.Equal(session.Id, sessionService.DeleteCalledWith.ToString());
            Assert.Equal($"{options.Value.ExternalUrl}/adfs/signout", data.RedirectUrl);
        }

        [Fact]
        public async Task Logout_Forms_Succeeds()
        {
            const UserAuthType authType = UserAuthType.Forms;

            controller.Request.Cookies = new RequestCookieCollectionFake(
                //new KeyValuePair<string, string>(AuthController.RefreshTokenCookieName, "x"),
                new KeyValuePair<string, string>(AuthController.AuthTypeCookieName, authType.ToString())
            );

            var session = ServiceFactory.CreateSession();
            controller.HttpContext.Session = session;

            var sessionService = ServiceFactory.CreateSessionService();

            var result = await controller.Logout(sessionService);
            var data = result.Value;

            // Assert
            Assert.True(session.ClearCalled);
            Assert.Equal(session.Id, sessionService.DeleteCalledWith.ToString());
            Assert.Equal("/", data.RedirectUrl);
        }
    }
}
