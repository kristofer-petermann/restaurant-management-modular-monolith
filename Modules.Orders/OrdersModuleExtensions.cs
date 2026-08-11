using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.Orders.Application;
using Modules.Orders.Application.Services;
using Modules.Orders.Infrastructure;
using Modules.Orders.Infrastructure.Seeding;
using Shared.Contracts.Orders;

namespace Modules.Orders;

public static class OrdersModuleExtensions
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IOrdersUow, OrdersUow>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderRowRepository, OrderRowRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<Modules.Orders.Application.Services.IOrderService, Modules.Orders.Application.Services.OrderService>();
        services.AddScoped<Modules.Orders.Application.Services.IOrderRowService, Modules.Orders.Application.Services.OrderRowService>();
        services.AddScoped<Modules.Orders.Application.Services.IPaymentService, Modules.Orders.Application.Services.PaymentService>();
        services.AddScoped<IOrdersModuleApi, OrdersModuleApi>();
        return services;
    }

    public static async Task InitializeOrdersModuleAsync(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(configuration);

        if (!IsEnabled(configuration, "DataInitialization:MigrateDatabase"))
        {
            return;
        }

        await serviceProvider.MigrateOrdersModuleAsync();
    }

    public static async Task MigrateOrdersModuleAsync(this IServiceProvider rootServices)
    {
        ArgumentNullException.ThrowIfNull(rootServices);

        using var scope = rootServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        await context.Database.MigrateAsync();
    }

    public static async Task SeedOrdersModuleAsync(this IServiceProvider rootServices)
    {
        ArgumentNullException.ThrowIfNull(rootServices);

        using var scope = rootServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        await OrdersSeeder.SeedAsync(context);
    }

    private static bool IsEnabled(IConfiguration configuration, string key)
    {
        return bool.TryParse(configuration[key], out var enabled) && enabled;
    }
}
