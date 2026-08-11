using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Restaurants.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillRestaurantsAppUserFromIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO restaurants."AppUser" ("Id", "UserName", "Email")
                SELECT DISTINCT u."Id", u."UserName", u."Email"
                FROM identity."AspNetUsers" u
                WHERE u."Id" IN (
                    SELECT r."UserId" FROM restaurants."Reviews" r
                    UNION
                    SELECT ru."UserId" FROM restaurants."RestaurantUser" ru
                    UNION
                    SELECT a."UserId" FROM restaurants."Addresses" a WHERE a."UserId" IS NOT NULL
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM restaurants."AppUser" au
                    WHERE au."Id" = u."Id"
                );
                """);

            migrationBuilder.Sql("""
                UPDATE restaurants."AppUser" au
                SET "UserName" = COALESCE(au."UserName", u."UserName"),
                    "Email" = COALESCE(au."Email", u."Email")
                FROM identity."AspNetUsers" u
                WHERE au."Id" = u."Id"
                  AND (au."UserName" IS NULL OR au."Email" IS NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data backfill migration: no safe down operation.
        }
    }
}
