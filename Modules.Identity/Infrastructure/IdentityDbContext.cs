using Base.Domain;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Domain;
using System.Text.Json;

namespace Modules.Identity.Infrastructure;

internal class IdentityDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IDataProtectionKeyContext
{
    public DbSet<RestaurantUser> RestaurantUsers { get; set; } = default!;
    public DbSet<AppRefreshToken> RefreshTokens { get; set; } = default!;
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = default!;

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("identity");
        base.OnModelCreating(builder);
        builder.Ignore<LangStr>();

        var summaryProperty = builder.Entity<RestaurantUser>().Property(e => e.Summary);
        summaryProperty
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");
        summaryProperty.Metadata.SetValueComparer(new ValueComparer<LangStr?>(
            (a, b) =>
                ReferenceEquals(a, b) ||
                (a != null && b != null &&
                 JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) ==
                 JsonSerializer.Serialize(b, (JsonSerializerOptions?)null)),
            v => v == null ? 0 : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
            v => v == null
                ? null
                : JsonSerializer.Deserialize<LangStr>(
                    JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    (JsonSerializerOptions?)null)
        ));

        builder.Entity<AppUserRole>(b =>
        {
            b.HasOne(x => x.User)
                .WithMany(x => x.AppUserRoles)
                .HasForeignKey(x => x.UserId);
            b.HasOne(x => x.Role)
                .WithMany(x => x.AppUserRoles)
                .HasForeignKey(x => x.RoleId);
        });
    }
}
