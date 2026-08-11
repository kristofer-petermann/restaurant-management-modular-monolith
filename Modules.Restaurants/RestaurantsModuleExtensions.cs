using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.Restaurants.Application;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Infrastructure;
using Modules.Restaurants.Infrastructure.Seeding;
using Shared.Contracts.Restaurants;

namespace Modules.Restaurants;

public static class RestaurantsModuleExtensions
{
    public static IServiceCollection AddRestaurantsModule(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<RestaurantsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IRestaurantsUow, RestaurantsUow>();
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IMenuCategoryRepository, MenuCategoryRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        services.AddScoped<IOpeningHourRepository, OpeningHourRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<Modules.Restaurants.Application.Services.IAddressService, Modules.Restaurants.Application.Services.AddressService>();
        services.AddScoped<Modules.Restaurants.Application.Services.IMenuCategoryService, Modules.Restaurants.Application.Services.MenuCategoryService>();
        services.AddScoped<Modules.Restaurants.Application.Services.IMenuItemService, Modules.Restaurants.Application.Services.MenuItemService>();
        services.AddScoped<Modules.Restaurants.Application.Services.IMenuService, Modules.Restaurants.Application.Services.MenuService>();
        services.AddScoped<Modules.Restaurants.Application.Services.IOpeningHourService, Modules.Restaurants.Application.Services.OpeningHourService>();
        services.AddScoped<Modules.Restaurants.Application.Services.IRestaurantService, Modules.Restaurants.Application.Services.RestaurantService>();
        services.AddScoped<Modules.Restaurants.Application.Services.IReviewService, Modules.Restaurants.Application.Services.ReviewService>();
        services.AddScoped<IRestaurantsModuleApi, RestaurantsModuleApi>();
        return services;
    }

    public static async Task InitializeRestaurantsModuleAsync(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(configuration);

        if (!IsEnabled(configuration, "DataInitialization:MigrateDatabase"))
        {
            return;
        }

        await serviceProvider.MigrateRestaurantsModuleAsync();
    }

    public static async Task MigrateRestaurantsModuleAsync(this IServiceProvider rootServices)
    {
        ArgumentNullException.ThrowIfNull(rootServices);

        using var scope = rootServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantsDbContext>();
        await context.Database.MigrateAsync();
    }

    public static async Task SeedRestaurantsModuleAsync(this IServiceProvider rootServices)
    {
        ArgumentNullException.ThrowIfNull(rootServices);

        using var scope = rootServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantsDbContext>();
        await RestaurantsSeeder.SeedAsync(context);
    }

    private static bool IsEnabled(IConfiguration configuration, string key)
    {
        return bool.TryParse(configuration[key], out var enabled) && enabled;
    }
}
