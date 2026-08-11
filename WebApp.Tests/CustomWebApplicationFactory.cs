using Asp.Versioning;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.Identity.Infrastructure;
using Modules.Orders.Infrastructure;
using Modules.Restaurants.Infrastructure;
using WebApp.Tests.Helpers;

namespace WebApp.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _identityDbName = $"identity-test-{Guid.NewGuid():N}";
    private readonly string _restaurantsDbName = $"restaurants-test-{Guid.NewGuid():N}";
    private readonly string _ordersDbName = $"orders-test-{Guid.NewGuid():N}";

    private SqliteConnection? _identityKeepAlive;
    private SqliteConnection? _restaurantsKeepAlive;
    private SqliteConnection? _ordersKeepAlive;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Skip Program.cs's SetupAppData (MigrateDatabase/SeedIdentity/SeedData):
        // we manage schema with EnsureCreated and seed via DataSeeder below.
        builder.UseSetting("DataInitialization:DropDatabase", "false");
        builder.UseSetting("DataInitialization:MigrateDatabase", "false");
        builder.UseSetting("DataInitialization:SeedIdentity", "false");
        builder.UseSetting("DataInitialization:SeedData", "false");

        builder.ConfigureServices(services =>
        {
            RemoveDbContextRegistration<IdentityDbContext>(services);
            RemoveDbContextRegistration<RestaurantsDbContext>(services);
            RemoveDbContextRegistration<OrdersDbContext>(services);

            _identityKeepAlive ??= OpenKeepAliveConnection(_identityDbName);
            _restaurantsKeepAlive ??= OpenKeepAliveConnection(_restaurantsDbName);
            _ordersKeepAlive ??= OpenKeepAliveConnection(_ordersDbName);

            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseSqlite(ConnectionStringFor(_identityDbName));
                options.ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            services.AddDbContext<RestaurantsDbContext>(options =>
            {
                options.UseSqlite(ConnectionStringFor(_restaurantsDbName));
                options.ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            services.AddDbContext<OrdersDbContext>(options =>
            {
                options.UseSqlite(ConnectionStringFor(_ordersDbName));
                options.ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            // Asp.Versioning's default rejects unversioned requests with 400. The unversioned
            // ListItemsController route ("api/[controller]") needs to opt into the default version
            // so tests can hit it directly. This only affects the test host.
            services.Configure<ApiVersioningOptions>(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
            });
        });

        // Run schema + seed once after the host is fully built so UserManager/RoleManager are available.
        builder.UseDefaultServiceProvider((_, _) => { });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        EnsureCreatedAndSeed(host.Services);
        return host;
    }

    protected void EnsureCreatedAndSeed(IServiceProvider rootServices)
    {
        using var scope = rootServices.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

        sp.GetRequiredService<IdentityDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<RestaurantsDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<OrdersDbContext>().Database.EnsureCreated();

        try
        {
            DataSeeder.SeedData(rootServices);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred seeding the " +
                                "database with test data. Error: {Message}", ex.Message);
            throw;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _identityKeepAlive?.Dispose();
            _restaurantsKeepAlive?.Dispose();
            _ordersKeepAlive?.Dispose();
            _identityKeepAlive = null;
            _restaurantsKeepAlive = null;
            _ordersKeepAlive = null;
        }
        base.Dispose(disposing);
    }

    private static void RemoveDbContextRegistration<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var descriptors = services
            .Where(d =>
                d.ServiceType == typeof(IDbContextOptionsConfiguration<TContext>) ||
                d.ServiceType == typeof(DbContextOptions<TContext>) ||
                d.ServiceType == typeof(TContext))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static SqliteConnection OpenKeepAliveConnection(string dbName)
    {
        var connection = new SqliteConnection(ConnectionStringFor(dbName));
        connection.Open();
        return connection;
    }

    private static string ConnectionStringFor(string dbName) =>
        $"DataSource=file:{dbName}?mode=memory&cache=shared";
}
