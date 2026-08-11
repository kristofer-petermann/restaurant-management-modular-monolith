using Base.Domain;
using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain;
using System.Text.Json;

namespace Modules.Orders.Infrastructure;

internal class OrdersDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; } = default!;
    public DbSet<OrderRow> OrderRows { get; set; } = default!;
    public DbSet<Payment> Payments { get; set; } = default!;

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("orders");
        base.OnModelCreating(builder);
        builder.Ignore<LangStr>();

        builder.Entity<Order>().Property(e => e.Summary)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<OrderRow>().Property(e => e.Summary)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<Payment>().Property(e => e.Summary)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<Order>().Property(o => o.TotalPrice).HasColumnType("decimal(10,2)");
        builder.Entity<OrderRow>().Property(or => or.UnitPriceSnapshot).HasColumnType("decimal(10,2)");
        builder.Entity<OrderRow>().Property(or => or.LineTotal).HasColumnType("decimal(10,2)");
        builder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(10,2)");
    }
}
