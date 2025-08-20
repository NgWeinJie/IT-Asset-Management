using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ITAssetManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AssetTag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WarrantyExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Assets",
                columns: new[] { "Id", "AssetTag", "AssignedTo", "Brand", "Category", "CreatedAt", "CreatedBy", "Location", "Model", "Name", "Notes", "PurchaseDate", "PurchasePrice", "SerialNumber", "Status", "UpdatedAt", "UpdatedBy", "WarrantyExpiry" },
                values: new object[,]
                {
                    { 1, "LT001", "John Doe", "Dell", "Laptop", new DateTime(2025, 8, 15, 16, 12, 50, 926, DateTimeKind.Local).AddTicks(1009), "admin", "Office Floor 1", "OptiPlex 7490", "Dell Laptop OptiPlex 1", "", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 1200.00m, "DL2024001", "Assigned", null, "", new DateTime(2027, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "DT001", "", "HP", "Desktop", new DateTime(2025, 8, 15, 16, 12, 50, 926, DateTimeKind.Local).AddTicks(1013), "admin", "IT Storage", "Pro 3500", "HP Desktop Pro 1", "", new DateTime(2024, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 800.00m, "HP2024001", "Available", null, "", new DateTime(2027, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "MN001", "Jane Smith", "Samsung", "Monitor", new DateTime(2025, 8, 15, 16, 12, 50, 926, DateTimeKind.Local).AddTicks(1016), "admin", "Office Floor 2", "S24E450", "Samsung Monitor 24 inch", "", new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 250.00m, "SM2024001", "Assigned", null, "", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, "LT002", "", "Lenovo", "Laptop", new DateTime(2025, 8, 15, 16, 12, 50, 926, DateTimeKind.Local).AddTicks(1020), "admin", "IT Department", "ThinkPad X1 Carbon", "Lenovo ThinkPad X1", "Screen flickering issue - under repair", new DateTime(2023, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1800.00m, "LV2024001", "Maintenance", null, "", new DateTime(2026, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetTag",
                table: "Assets",
                column: "AssetTag",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");
        }
    }
}
