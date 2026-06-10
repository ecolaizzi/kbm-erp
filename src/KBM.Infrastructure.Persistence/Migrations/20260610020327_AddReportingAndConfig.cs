using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KBM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingAndConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "SystemSettings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "SystemSettings",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "CompanyId",
                table: "SystemSettings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReportDefinitions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Engine = table.Column<int>(type: "int", nullable: false),
                    OutputFormat = table.Column<int>(type: "int", nullable: false),
                    TemplatePathOrName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDefinitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_CompanyId_Key",
                table: "SystemSettings",
                columns: new[] { "CompanyId", "Key" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitions_CompanyId_Key",
                table: "ReportDefinitions",
                columns: new[] { "CompanyId", "Key" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_SystemSettings_CompanyId_Key",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SystemSettings");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "SystemSettings",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings",
                column: "Key",
                unique: true);
        }
    }
}
