using Base.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;

namespace WebApp.Tests.Helpers;

public static class DataSeeder
{
    public static void SeedData(IServiceProvider rootServices)
    {
        using var scope = rootServices.CreateScope();
        var sp = scope.ServiceProvider;

        SeedIdentityAsync(sp).GetAwaiter().GetResult();
        SeedRestaurantsAsync(sp).GetAwaiter().GetResult();
        SeedOrdersAsync(sp).GetAwaiter().GetResult();
    }

    private static async Task SeedIdentityAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<AppRole>>();
        var userManager = sp.GetRequiredService<UserManager<AppUser>>();
        var identityDb = sp.GetRequiredService<IdentityDbContext>();

        foreach (var roleName in new[] { "admin", "user", "root" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole { Name = roleName, NormalizedName = roleName.ToUpperInvariant() });
            }
        }

        var userA = await EnsureUserAsync(userManager, TestUsers.UserAId, TestUsers.UserAEmail);
        var userB = await EnsureUserAsync(userManager, TestUsers.UserBId, TestUsers.UserBEmail);

        if (!await userManager.IsInRoleAsync(userA, "admin")) await userManager.AddToRoleAsync(userA, "admin");
        if (!await userManager.IsInRoleAsync(userA, "user")) await userManager.AddToRoleAsync(userA, "user");
        if (!await userManager.IsInRoleAsync(userB, "user")) await userManager.AddToRoleAsync(userB, "user");

        if (!await identityDb.RestaurantUsers.AnyAsync(ru => ru.Id == TestUsers.RestaurantUserAId))
        {
            identityDb.RestaurantUsers.Add(new Modules.Identity.Domain.RestaurantUser
            {
                Id = TestUsers.RestaurantUserAId,
                UserId = TestUsers.UserAId,
                RestaurantId = TestUsers.RestaurantAId,
                UserRole = EUserRoleInRestaurant.Admin,
                Role = "admin",
                Summary = new LangStr("A admin")
            });
        }

        if (!await identityDb.RestaurantUsers.AnyAsync(ru => ru.Id == TestUsers.RestaurantUserBId))
        {
            identityDb.RestaurantUsers.Add(new Modules.Identity.Domain.RestaurantUser
            {
                Id = TestUsers.RestaurantUserBId,
                UserId = TestUsers.UserBId,
                RestaurantId = TestUsers.RestaurantBId,
                UserRole = EUserRoleInRestaurant.Admin,
                Role = "admin",
                Summary = new LangStr("B admin")
            });
        }

        if (!await identityDb.RestaurantUsers.AnyAsync(ru => ru.Id == TestUsers.RestaurantUserForeignInAId))
        {
            identityDb.RestaurantUsers.Add(new Modules.Identity.Domain.RestaurantUser
            {
                Id = TestUsers.RestaurantUserForeignInAId,
                UserId = TestUsers.UserBId,
                RestaurantId = TestUsers.RestaurantAId,
                UserRole = EUserRoleInRestaurant.Normal,
                Role = "user",
                Summary = new LangStr("Foreign in A")
            });
        }

        await identityDb.SaveChangesAsync();
    }

    private static async Task SeedRestaurantsAsync(IServiceProvider sp)
    {
        var ctx = sp.GetRequiredService<RestaurantsDbContext>();

        if (!await ctx.Set<RestaurantUserProfile>().AnyAsync(u => u.Id == TestUsers.UserAId))
        {
            ctx.Set<RestaurantUserProfile>().Add(new RestaurantUserProfile
            {
                Id = TestUsers.UserAId,
                UserName = TestUsers.UserAEmail,
                Email = TestUsers.UserAEmail
            });
        }

        if (!await ctx.Set<RestaurantUserProfile>().AnyAsync(u => u.Id == TestUsers.UserBId))
        {
            ctx.Set<RestaurantUserProfile>().Add(new RestaurantUserProfile
            {
                Id = TestUsers.UserBId,
                UserName = TestUsers.UserBEmail,
                Email = TestUsers.UserBEmail
            });
        }

        if (!await ctx.Restaurants.AnyAsync(r => r.Id == TestUsers.RestaurantAId))
        {
            ctx.Restaurants.Add(new Restaurant
            {
                Id = TestUsers.RestaurantAId,
                RestaurantName = "Restaurant A",
                Description = new LangStr("Restaurant A"),
                Phone = "+3725000001",
                Email = "a@restaurant.test",
                Summary = new LangStr("A")
            });
        }

        if (!await ctx.Restaurants.AnyAsync(r => r.Id == TestUsers.RestaurantBId))
        {
            ctx.Restaurants.Add(new Restaurant
            {
                Id = TestUsers.RestaurantBId,
                RestaurantName = "Restaurant B",
                Description = new LangStr("Restaurant B"),
                Phone = "+3725000002",
                Email = "b@restaurant.test",
                Summary = new LangStr("B")
            });
        }

        if (!await ctx.Set<Modules.Restaurants.Domain.RestaurantUser>().AnyAsync(ru => ru.Id == TestUsers.RestaurantUserAId))
        {
            ctx.Set<Modules.Restaurants.Domain.RestaurantUser>().Add(new Modules.Restaurants.Domain.RestaurantUser
            {
                Id = TestUsers.RestaurantUserAId,
                UserId = TestUsers.UserAId,
                RestaurantId = TestUsers.RestaurantAId
            });
        }

        if (!await ctx.Set<Modules.Restaurants.Domain.RestaurantUser>().AnyAsync(ru => ru.Id == TestUsers.RestaurantUserBId))
        {
            ctx.Set<Modules.Restaurants.Domain.RestaurantUser>().Add(new Modules.Restaurants.Domain.RestaurantUser
            {
                Id = TestUsers.RestaurantUserBId,
                UserId = TestUsers.UserBId,
                RestaurantId = TestUsers.RestaurantBId
            });
        }

        if (!await ctx.Set<Modules.Restaurants.Domain.RestaurantUser>().AnyAsync(ru => ru.Id == TestUsers.RestaurantUserForeignInAId))
        {
            ctx.Set<Modules.Restaurants.Domain.RestaurantUser>().Add(new Modules.Restaurants.Domain.RestaurantUser
            {
                Id = TestUsers.RestaurantUserForeignInAId,
                UserId = TestUsers.UserBId,
                RestaurantId = TestUsers.RestaurantAId
            });
        }

        if (!await ctx.Addresses.AnyAsync(a => a.Id == TestUsers.UserAItemId))
        {
            ctx.Addresses.Add(new Address
            {
                Id = TestUsers.UserAItemId,
                RestaurantId = TestUsers.RestaurantAId,
                Street = "Test 1",
                City = "Tallinn",
                PostalCode = "10111",
                Country = new LangStr("Estonia"),
                Summary = new LangStr("Address A")
            });
        }

        if (!await ctx.Menus.AnyAsync(m => m.Id == TestUsers.MenuAId))
        {
            ctx.Menus.Add(new Menu
            {
                Id = TestUsers.MenuAId,
                RestaurantId = TestUsers.RestaurantAId,
                Name = new LangStr("Menu A"),
                IsActive = true,
                Summary = new LangStr("Menu A")
            });
        }

        if (!await ctx.MenuCategories.AnyAsync(mc => mc.Id == TestUsers.MenuCategoryAId))
        {
            ctx.MenuCategories.Add(new MenuCategory
            {
                Id = TestUsers.MenuCategoryAId,
                MenuId = TestUsers.MenuAId,
                Name = new LangStr("Category A"),
                Summary = new LangStr("Category A")
            });
        }

        if (!await ctx.MenuItems.AnyAsync(mi => mi.Id == TestUsers.MenuItemAId))
        {
            ctx.MenuItems.Add(new MenuItem
            {
                Id = TestUsers.MenuItemAId,
                MenuCategoryId = TestUsers.MenuCategoryAId,
                Name = new LangStr("Item A"),
                Description = new LangStr("Item A"),
                Price = 10m,
                IsAvailable = true,
                Summary = new LangStr("Item A")
            });
        }

        if (!await ctx.OpeningHours.AnyAsync(oh => oh.Id == TestUsers.OpeningHourAId))
        {
            ctx.OpeningHours.Add(new OpeningHour
            {
                Id = TestUsers.OpeningHourAId,
                RestaurantId = TestUsers.RestaurantAId,
                DayOfWeek = new LangStr("Monday"),
                OpenTime = new TimeOnly(9, 0),
                CloseTime = new TimeOnly(18, 0),
                Summary = new LangStr("Opening A")
            });
        }

        if (!await ctx.Reviews.AnyAsync(r => r.Id == TestUsers.ReviewAId))
        {
            ctx.Reviews.Add(new Review
            {
                Id = TestUsers.ReviewAId,
                UserId = TestUsers.UserAId,
                RestaurantId = TestUsers.RestaurantAId,
                Rating = 5,
                Comment = "A review",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Summary = new LangStr("Review A")
            });
        }

        await ctx.SaveChangesAsync();
    }

    private static async Task SeedOrdersAsync(IServiceProvider sp)
    {
        var ctx = sp.GetRequiredService<OrdersDbContext>();

        if (!await ctx.Orders.AnyAsync(o => o.Id == TestUsers.OrderAId))
        {
            ctx.Orders.Add(new Order
            {
                Id = TestUsers.OrderAId,
                UserId = TestUsers.UserAId,
                RestaurantId = TestUsers.RestaurantAId,
                RestaurantNameSnapshot = "Restaurant A",
                OrderStatus = "Pending",
                TotalPrice = 10m,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                PickupTime = DateTime.UtcNow.AddHours(1),
                Summary = new LangStr("Order A")
            });
        }

        if (!await ctx.OrderRows.AnyAsync(or => or.Id == TestUsers.OrderRowAId))
        {
            ctx.OrderRows.Add(new OrderRow
            {
                Id = TestUsers.OrderRowAId,
                OrderId = TestUsers.OrderAId,
                MenuItemId = TestUsers.MenuItemAId,
                MenuItemNameSnapshot = "Item A",
                Quantity = 1,
                UnitPriceSnapshot = 10m,
                LineTotal = 10m,
                Summary = new LangStr("OrderRow A")
            });
        }

        if (!await ctx.Payments.AnyAsync(p => p.Id == TestUsers.PaymentAId))
        {
            ctx.Payments.Add(new Payment
            {
                Id = TestUsers.PaymentAId,
                OrderId = TestUsers.OrderAId,
                Amount = 10m,
                PaymentStatus = "Paid",
                PaymentMethod = "Card",
                PaidAt = DateTime.UtcNow,
                Summary = new LangStr("Payment A")
            });
        }

        await ctx.SaveChangesAsync();
    }

    private static async Task<AppUser> EnsureUserAsync(UserManager<AppUser> userManager, Guid id, string email)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing != null) return existing;

        var user = new AppUser
        {
            Id = id,
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var res = await userManager.CreateAsync(user, TestUsers.Password);
        if (!res.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create test user {email}: {string.Join(", ", res.Errors.Select(e => e.Description))}");
        }

        return user;
    }
}
