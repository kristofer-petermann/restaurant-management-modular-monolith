using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Modules.Identity.Application;
using Modules.Identity.Application.Services;
using Modules.Identity.Infrastructure;
using Modules.Identity.Infrastructure.Seeding;
using Modules.Identity.Domain;
using Npgsql;
using Shared.Contracts.Identity;

namespace Modules.Identity;

public static class IdentityModuleExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services
            .AddDbContext<IdentityDbContext>(options => options
                .UseNpgsql(
                    connectionString,
                    o => { o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery); }
                )
                .ConfigureWarnings(w =>
                    w.Throw(RelationalEventId.MultipleCollectionIncludeWarning)
                )
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
            );
        services.AddScoped<Modules.Identity.Application.Services.IRestaurantUserService, Modules.Identity.Application.Services.RestaurantUserService>();
        services.AddScoped<IIdentityAccountService, IdentityAccountService>();
        services.AddScoped<IIdentityModuleApi, IdentityModuleApi>();

        services
            .AddDataProtection()
            .PersistKeysToDbContext<IdentityDbContext>();

        services
            .AddIdentity<AppUser, AppRole>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddDefaultUI()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    public static async Task DropDatabaseAsync(this IServiceProvider rootServices)
    {
        ArgumentNullException.ThrowIfNull(rootServices);

        using var scope = rootServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        IdentityDataInit.DeleteDatabase(context);
        await Task.CompletedTask;
    }

    public static async Task MigrateIdentityModuleAsync(this IServiceProvider rootServices)
    {
        ArgumentNullException.ThrowIfNull(rootServices);

        using var scope = rootServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await context.Database.MigrateAsync();
    }

    public static async Task SeedIdentityModuleAsync(this IServiceProvider rootServices)
    {
        ArgumentNullException.ThrowIfNull(rootServices);

        using var scope = rootServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        IdentityDataInit.SeedIdentity(userManager, roleManager);
        await Task.CompletedTask;
    }

    public static async Task InitializeIdentityModuleAsync(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(configuration);

        if (IsEnabled(configuration, "DataInitialization:DropDatabase"))
        {
            await serviceProvider.DropDatabaseAsync();
        }

        if (IsEnabled(configuration, "DataInitialization:MigrateDatabase"))
        {
            await serviceProvider.MigrateIdentityModuleAsync();
        }

        if (IsEnabled(configuration, "DataInitialization:SeedIdentity"))
        {
            await serviceProvider.SeedIdentityModuleAsync();
        }
    }

    public static void WaitDbConnection(string connectionString, ILogger logger)
    {
        while (true)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                conn.Close();
                return;
            }
            catch (NpgsqlException e)
            {
                logger.LogWarning("Checked postgres db connection. Got: {Message}", e.Message);

                if (e.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning("Database does not exist yet, continuing to migration phase.");
                    return;
                }

                logger.LogWarning("Waiting for db connection. Sleep 1 sec");
                Thread.Sleep(1000);
            }
        }
    }

    private static bool IsEnabled(IConfiguration configuration, string key)
    {
        return bool.TryParse(configuration[key], out var enabled) && enabled;
    }
}
