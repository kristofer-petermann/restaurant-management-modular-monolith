namespace Modules.Orders.Infrastructure.Seeding;

internal static class OrdersSeeder
{
    public static Task SeedAsync(OrdersDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        return Task.CompletedTask;
    }
}
