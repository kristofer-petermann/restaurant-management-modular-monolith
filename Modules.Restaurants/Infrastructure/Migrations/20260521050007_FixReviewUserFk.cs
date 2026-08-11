using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Restaurants.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixReviewUserFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AppUser_AppUserId",
                schema: "restaurants",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_AppUserId",
                schema: "restaurants",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                schema: "restaurants",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                schema: "restaurants",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AppUser_UserId",
                schema: "restaurants",
                table: "Reviews",
                column: "UserId",
                principalSchema: "restaurants",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AppUser_UserId",
                schema: "restaurants",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId",
                schema: "restaurants",
                table: "Reviews");

            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId",
                schema: "restaurants",
                table: "Reviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AppUserId",
                schema: "restaurants",
                table: "Reviews",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AppUser_AppUserId",
                schema: "restaurants",
                table: "Reviews",
                column: "AppUserId",
                principalSchema: "restaurants",
                principalTable: "AppUser",
                principalColumn: "Id");
        }
    }
}
