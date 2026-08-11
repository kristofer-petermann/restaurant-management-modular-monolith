using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Restaurants.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FillMissingRestaurantsUserEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE restaurants."AppUser" au
                SET "Email" = COALESCE(
                    NULLIF(u."Email", ''),
                    NULLIF(u."UserName", '')
                )
                FROM identity."AspNetUsers" u
                WHERE au."Id" = u."Id"
                  AND (au."Email" IS NULL OR au."Email" = '')
                  AND (
                      NULLIF(u."Email", '') IS NOT NULL
                      OR NULLIF(u."UserName", '') IS NOT NULL
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data fix migration: no safe down operation.
        }
    }
}
