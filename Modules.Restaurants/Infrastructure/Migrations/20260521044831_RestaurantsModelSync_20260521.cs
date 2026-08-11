using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Restaurants.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestaurantsModelSync_20260521 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderRow",
                schema: "restaurants");

            migrationBuilder.DropTable(
                name: "Payment",
                schema: "restaurants");

            migrationBuilder.DropTable(
                name: "Order",
                schema: "restaurants");

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "restaurants",
                table: "RestaurantUser");

            migrationBuilder.DropColumn(
                name: "UserRole",
                schema: "restaurants",
                table: "RestaurantUser");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                schema: "restaurants",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                schema: "restaurants",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                schema: "restaurants",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                schema: "restaurants",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                schema: "restaurants",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                schema: "restaurants",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                schema: "restaurants",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                schema: "restaurants",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "restaurants",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                schema: "restaurants",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                schema: "restaurants",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                schema: "restaurants",
                table: "AppUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                schema: "restaurants",
                table: "RestaurantUser",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserRole",
                schema: "restaurants",
                table: "RestaurantUser",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                schema: "restaurants",
                table: "AppUser",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                schema: "restaurants",
                table: "AppUser",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                schema: "restaurants",
                table: "AppUser",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                schema: "restaurants",
                table: "AppUser",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                schema: "restaurants",
                table: "AppUser",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                schema: "restaurants",
                table: "AppUser",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                schema: "restaurants",
                table: "AppUser",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                schema: "restaurants",
                table: "AppUser",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "restaurants",
                table: "AppUser",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                schema: "restaurants",
                table: "AppUser",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                schema: "restaurants",
                table: "AppUser",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                schema: "restaurants",
                table: "AppUser",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Order",
                schema: "restaurants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderStatus = table.Column<string>(type: "text", nullable: false),
                    PickupTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_AppUser_UserId",
                        column: x => x.UserId,
                        principalSchema: "restaurants",
                        principalTable: "AppUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalSchema: "restaurants",
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderRow",
                schema: "restaurants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderRow_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalSchema: "restaurants",
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderRow_Order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "restaurants",
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                schema: "restaurants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    PaymentStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_Order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "restaurants",
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Order_RestaurantId",
                schema: "restaurants",
                table: "Order",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_UserId",
                schema: "restaurants",
                table: "Order",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRow_MenuItemId",
                schema: "restaurants",
                table: "OrderRow",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRow_OrderId",
                schema: "restaurants",
                table: "OrderRow",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_OrderId",
                schema: "restaurants",
                table: "Payment",
                column: "OrderId");
        }
    }
}
