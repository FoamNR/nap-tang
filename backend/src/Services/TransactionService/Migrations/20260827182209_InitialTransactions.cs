using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EasyTrack.TransactionService.Migrations
{
    /// <inheritdoc />
    public partial class InitialTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "easytrack_transactions");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "easytrack_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IconName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ColorHex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                schema: "easytrack_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SlipUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transactions_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "easytrack_transactions",
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "easytrack_transactions",
                table: "categories",
                columns: new[] { "Id", "ColorHex", "CreatedAt", "IconName", "Name", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "#EF4444", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Utensils", "Food & Drinks", "expense", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "#3B82F6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Car", "Transportation", "expense", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "#EC4899", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ShoppingBag", "Shopping", "expense", null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "#F59E0B", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Receipt", "Bills & Utilities", "expense", null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "#8B5CF6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tv", "Entertainment", "expense", null },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "#10B981", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Briefcase", "Salary", "income", null },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "#06B6D4", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "TrendingUp", "Investment", "income", null },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "#64748B", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Coins", "Other Income", "income", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_CategoryId",
                schema: "easytrack_transactions",
                table: "transactions",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactions",
                schema: "easytrack_transactions");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "easytrack_transactions");
        }
    }
}
