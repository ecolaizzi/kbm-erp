using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KBM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseRequestsAndRfqs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestingUnit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_PurchaseRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequestLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseRequestId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RequiredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProposedPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    LineStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("PK_PurchaseRequestLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestLines_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseRequestLines_PurchaseRequests_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rfqs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    PurchaseRequestId = table.Column<long>(type: "bigint", nullable: true),
                    ResponseDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Rfqs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rfqs_PurchaseRequests_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rfqs_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequestLineSuppliers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseRequestLineId = table.Column<long>(type: "bigint", nullable: false),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_PurchaseRequestLineSuppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestLineSuppliers_PurchaseRequestLines_PurchaseRequestLineId",
                        column: x => x.PurchaseRequestLineId,
                        principalTable: "PurchaseRequestLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestLineSuppliers_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RfqLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RfqId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Available = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_RfqLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RfqLines_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RfqLines_Rfqs_RfqId",
                        column: x => x.RfqId,
                        principalTable: "Rfqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestLines_CompanyId_PurchaseRequestId",
                table: "PurchaseRequestLines",
                columns: new[] { "CompanyId", "PurchaseRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestLines_ItemId",
                table: "PurchaseRequestLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestLines_PurchaseRequestId",
                table: "PurchaseRequestLines",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestLineSuppliers_CompanyId_PurchaseRequestLineId",
                table: "PurchaseRequestLineSuppliers",
                columns: new[] { "CompanyId", "PurchaseRequestLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestLineSuppliers_PurchaseRequestLineId",
                table: "PurchaseRequestLineSuppliers",
                column: "PurchaseRequestLineId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestLineSuppliers_SupplierId",
                table: "PurchaseRequestLineSuppliers",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_CompanyId_Date",
                table: "PurchaseRequests",
                columns: new[] { "CompanyId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_CompanyId_Number",
                table: "PurchaseRequests",
                columns: new[] { "CompanyId", "Number" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RfqLines_CompanyId_RfqId",
                table: "RfqLines",
                columns: new[] { "CompanyId", "RfqId" });

            migrationBuilder.CreateIndex(
                name: "IX_RfqLines_ItemId",
                table: "RfqLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RfqLines_RfqId",
                table: "RfqLines",
                column: "RfqId");

            migrationBuilder.CreateIndex(
                name: "IX_Rfqs_CompanyId_Date",
                table: "Rfqs",
                columns: new[] { "CompanyId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Rfqs_CompanyId_Number",
                table: "Rfqs",
                columns: new[] { "CompanyId", "Number" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Rfqs_PurchaseRequestId",
                table: "Rfqs",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Rfqs_SupplierId",
                table: "Rfqs",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseRequestLineSuppliers");

            migrationBuilder.DropTable(
                name: "RfqLines");

            migrationBuilder.DropTable(
                name: "PurchaseRequestLines");

            migrationBuilder.DropTable(
                name: "Rfqs");

            migrationBuilder.DropTable(
                name: "PurchaseRequests");
        }
    }
}
