using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Modules.Identity.Domain;

namespace Modules.Identity.Infrastructure.Seeding;

internal static class IdentityDataInit
{
    public static void DeleteDatabase(IdentityDbContext context)
    {
        context.Database.EnsureDeleted();
    }

    public static void MigrateDatabase(IdentityDbContext context)
    {
        // During transition we may not have module migrations yet.
        // In that case, create schema/tables directly from the model.
        if (!context.Database.GetMigrations().Any())
        {
            var creator = context.GetService<IRelationalDatabaseCreator>();

            if (!creator.Exists())
            {
                creator.Create();
            }

            // EnsureCreated() is a no-op when *any* table exists in the DB.
            // For module transition we need to create Identity tables even in
            // a partially initialized shared database.
            var hasIdentityRolesTable = context.Database
                .SqlQueryRaw<int>(
                    """SELECT 1 WHERE to_regclass('identity."AspNetRoles"') IS NOT NULL"""
                )
                .Any();

            if (!hasIdentityRolesTable)
            {
                creator.CreateTables();
            }

            return;
        }

        context.Database.Migrate();
    }

    public static void SeedIdentity(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        foreach (var roleName in InitialData.Roles)
        {
            var role = roleManager.FindByNameAsync(roleName).Result;
            if (role != null) continue;

            role = new AppRole { Name = roleName };
            var result = roleManager.CreateAsync(role).Result;
            if (!result.Succeeded)
            {
                throw new ApplicationException("Role creation failed!");
            }
        }

        foreach (var userInfo in InitialData.Users)
        {
            var user = userManager.FindByEmailAsync(userInfo.email).Result;
            if (user == null)
            {
                user = new AppUser
                {
                    Email = userInfo.email,
                    UserName = userInfo.email,
                    EmailConfirmed = true
                };
                var result = userManager.CreateAsync(user, userInfo.password).Result;
                if (!result.Succeeded)
                {
                    throw new ApplicationException("User creation failed!");
                }
            }

            foreach (var role in userInfo.roles)
            {
                if (userManager.IsInRoleAsync(user, role).Result) continue;
                _ = userManager.AddToRoleAsync(user, role).Result;
            }
        }
    }
}
