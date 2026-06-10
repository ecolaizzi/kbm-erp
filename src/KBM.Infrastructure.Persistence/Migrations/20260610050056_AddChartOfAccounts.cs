using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KBM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChartOfAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountMasters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Nature = table.Column<int>(type: "int", nullable: false),
                    Sign = table.Column<int>(type: "int", nullable: false),
                    SubKind = table.Column<int>(type: "int", nullable: false),
                    AllowsPosting = table.Column<bool>(type: "bit", nullable: false),
                    BilCeeDare = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BilCeeAvere = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountMasters_AccountMasters_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AccountMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountMasters_CompanyId_FullCode",
                table: "AccountMasters",
                columns: new[] { "CompanyId", "FullCode" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AccountMasters_CompanyId_Level",
                table: "AccountMasters",
                columns: new[] { "CompanyId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountMasters_ParentId",
                table: "AccountMasters",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountMasters");
        }
    }
}
