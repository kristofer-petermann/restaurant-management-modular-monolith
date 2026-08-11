using Base.Domain;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Domain;
using System.Text.Json;

namespace Modules.Restaurants.Infrastructure;

internal class RestaurantsDbContext : DbContext
{
    public DbSet<Restaurant> Restaurants { get; set; } = default!;
    public DbSet<Address> Addresses { get; set; } = default!;
    public DbSet<OpeningHour> OpeningHours { get; set; } = default!;
    public DbSet<Menu> Menus { get; set; } = default!;
    public DbSet<MenuCategory> MenuCategories { get; set; } = default!;
    public DbSet<MenuItem> MenuItems { get; set; } = default!;
    public DbSet<Review> Reviews { get; set; } = default!;

    public RestaurantsDbContext(DbContextOptions<RestaurantsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("restaurants");
        base.OnModelCreating(builder);
        builder.Ignore<LangStr>();

        builder.Entity<Address>().Property(e => e.Summary)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<Address>().Property(e => e.Country)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<Menu>().Property(e => e.Name)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<Menu>().Property(e => e.Summary)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<MenuCategory>().Property(e => e.Name)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<MenuCategory>().Property(e => e.Summary)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<MenuItem>().Property(e => e.Name)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<MenuItem>().Property(e => e.Description)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<MenuItem>().Property(e => e.Summary)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<OpeningHour>().Property(e => e.DayOfWeek)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<OpeningHour>().Property(e => e.Summary)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<Restaurant>().Property(e => e.Description)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<Restaurant>().Property(e => e.Summary)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<Review>().Property(e => e.Summary)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        builder.Entity<RestaurantUserProfile>().ToTable("AppUser");
        builder.Entity<RestaurantUser>().ToTable("RestaurantUser");

        builder.Entity<Review>()
            .HasOne(r => r.AppUser)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId);

        builder.Entity<MenuItem>().Property(mi => mi.Price).HasColumnType("decimal(10,2)");
    }
}
