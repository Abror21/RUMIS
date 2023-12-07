using Izm.Rumis.Application.Common;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Izm.Rumis.Infrastructure.Common;
using Izm.Rumis.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Seeders
{
    public sealed class IdentityOptions
    {
        public string AdminPassword { get; set; }
        public string ExternalAdminRole { get; set; }
    }

    internal static class IdentitySeeder
    {
        private static SeedHelper helper = new SeedHelper(UserIds.Application);

        public static async Task SeedAsync(IIdentityDbContext identityDb, IAppDbContext db, IdentityOptions identityOptions)
        {
            var adminId = await EnsureLoginAsync(identityDb, IdentityRoleNames.Admin, UserNames.Admin, identityOptions.AdminPassword);

            await EnsureCountryProfileAsync(db, adminId, new string[] { RoleCode.Administrator });
        }

        private static async Task<Guid> EnsureLoginAsync(IIdentityDbContext db, string role, string userName, string password = null)
        {
            var login = await db.IdentityUserLogins
                .FirstOrDefaultAsync(t => t.UserName == userName);

            var passwordHasher = new PasswordHasher<IdentityUserLogin>();

            if (login == null)
            {
                login = new IdentityUserLogin
                {
                    UserName = userName,
                    AuthType = UserAuthType.Forms,
                    MustResetPassword = true
                };

                if (!string.IsNullOrEmpty(password))
                    login.PasswordHash = passwordHasher.HashPassword(login, password);

                var user = await db.IdentityUsers.FirstOrDefaultAsync(t => t.User.Name == userName);

                if (user == null)
                {
                    user = new IdentityUser()
                    {
                        User = User.Create(name: UserNames.Admin)
                    };

                    user.Id = user.User.Id;

                    db.IdentityUsers.Add(helper.Audit(user));

                    login.UserId = user.Id;
                }

                db.IdentityUserLogins.Add(helper.Audit(login));
            }
            else
            {
                if (!string.IsNullOrEmpty(password) && string.IsNullOrEmpty(login.PasswordHash))
                {
                    login.PasswordHash = passwordHasher.HashPassword(login, password);
                    login.MustResetPassword = true;
                }
            }

            await db.SaveChangesAsync();

            return login.UserId;
        }

        private static async Task EnsureCountryProfileAsync(IAppDbContext db, Guid userId, IEnumerable<string> roleCodes)
        {
            var profile = await db.UserProfiles
                .FirstOrDefaultAsync(t => t.UserId == userId && t.PermissionType == UserProfileType.Country && !t.Disabled);

            if (profile == null)
            {
                profile = UserProfile.Create();
                profile.UserId = userId;

                profile.SetDisabled(false);
                profile.SetExpiration(DateTime.ParseExact("2100.01.01", "yyyy.MM.dd", null));
                profile.SetAccessLevel(new AccessLevel
                {
                    Type = UserProfileType.Country
                });

                db.UserProfiles.Add(helper.Audit(profile));
            }

            profile.ClearRoles();

            profile.SetRoles(db.Roles
                .Where(t => roleCodes.Contains(t.Code))
                .ToList()
                );

            await db.SaveChangesAsync();
        }
    }
}
