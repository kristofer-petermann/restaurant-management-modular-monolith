namespace Modules.Restaurants.Infrastructure.Seeding;

internal static class RestaurantsSeeder
{
    public static Task SeedAsync(RestaurantsDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        return Task.CompletedTask;
    }
}
