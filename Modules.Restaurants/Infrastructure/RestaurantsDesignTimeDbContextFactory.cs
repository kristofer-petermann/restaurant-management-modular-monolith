using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace Modules.Restaurants.Infrastructure;

internal class RestaurantsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<RestaurantsDbContext>
{
    public RestaurantsDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<RestaurantsDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new RestaurantsDbContext(optionsBuilder.Options);
    }

    private static string ResolveConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv;

        var appSettingsPath = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "WebApp", "appsettings.json")
        );

        using var stream = File.OpenRead(appSettingsPath);
        using var doc = JsonDocument.Parse(stream);
        if (doc.RootElement.TryGetProperty("ConnectionStrings", out var csNode) &&
            csNode.TryGetProperty("DefaultConnection", out var defaultNode))
        {
            var value = defaultNode.GetString();
            if (!string.IsNullOrWhiteSpace(value)) return value;
        }

        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }
}
