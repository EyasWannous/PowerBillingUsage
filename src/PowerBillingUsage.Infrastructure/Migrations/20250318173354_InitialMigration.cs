using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PowerBillingUsage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingTypeValue = table.Column<int>(type: "integer", nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpperLimitInKWh = table.Column<int>(type: "integer", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false),
                    BillingTypeValue = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TierName = table.Column<string>(type: "text", nullable: false),
                    Consumption = table.Column<int>(type: "integer", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    BillId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillDetails_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Tiers",
                columns: new[] { "Id", "BillingTypeValue", "Name", "Rate", "UpperLimitInKWh" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 1, "Up to 160 KWh", 0.05m, 160 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 1, "Up to 300 KWh", 0.10m, 300 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 1, "Up to 500 KWh", 0.12m, 500 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 1, "Up to 600 KWh", 0.16m, 600 },
                    { new Guid("55555555-5555-5555-5555-555555555555"), 1, "Up to 750 KWh", 0.22m, 750 },
                    { new Guid("66666666-6666-6666-6666-666666666666"), 1, "Up to 1000 KWh", 0.27m, 1000 },
                    { new Guid("77777777-7777-7777-7777-777777777777"), 1, "Above 1000 KWh", 0.37m, 2147483647 },
                    { new Guid("88888888-8888-8888-8888-888888888888"), 2, "Up to 200 KWh", 0.08m, 200 },
                    { new Guid("99999999-9999-9999-9999-999999999999"), 2, "Up to 500 KWh", 0.15m, 500 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 2, "Above 500 KWh", 0.25m, 2147483647 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_BillId",
                table: "BillDetails",
                column: "BillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillDetails");

            migrationBuilder.DropTable(
                name: "Tiers");

            migrationBuilder.DropTable(
                name: "Bills");
        }
    }
}
