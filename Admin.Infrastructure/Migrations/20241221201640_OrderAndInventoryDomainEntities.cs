using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrderAndInventoryDomainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShippingCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingCostCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ShippingAddress_Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShippingAddress_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingAddress_State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingAddress_PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShippingAddress_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingAddress_Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BillingAddress_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingAddress_State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingAddress_PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BillingAddress_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Payment_TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Payment_Method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Payment_Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Payment_ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShippingInfo_Carrier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingInfo_TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingInfo_EstimatedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShippingInfo_ActualDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AvailableStock = table.Column<int>(type: "int", nullable: false, computedColumnSql: "[CurrentStock] - [ReservedStock]"),
                    IsLowStock = table.Column<bool>(type: "bit", nullable: false, computedColumnSql: "CASE WHEN [CurrentStock] - [ReservedStock] <= [LowStockThreshold] THEN 1 ELSE 0 END"),
                    IsOutOfStock = table.Column<bool>(type: "bit", nullable: false, computedColumnSql: "CASE WHEN [CurrentStock] - [ReservedStock] <= 0 THEN 1 ELSE 0 END"),
                    CurrentStock = table.Column<int>(type: "int", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "int", nullable: false),
                    ReservedStock = table.Column<int>(type: "int", nullable: false),
                    TrackInventory = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StockReservation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StockItemId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockReservation_StockItem_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockReservation_StockItem_StockItemId1",
                        column: x => x.StockItemId1,
                        principalTable: "StockItem",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StockItem_IsLowStock",
                table: "StockItem",
                column: "IsLowStock");

            migrationBuilder.CreateIndex(
                name: "IX_StockItem_IsOutOfStock",
                table: "StockItem",
                column: "IsOutOfStock");

            migrationBuilder.CreateIndex(
                name: "IX_StockItem_ProductId",
                table: "StockItem",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockReservation_ExpiresAt",
                table: "StockReservation",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservation_OrderId",
                table: "StockReservation",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservation_Status",
                table: "StockReservation",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservation_StockItemId",
                table: "StockReservation",
                column: "StockItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservation_StockItemId1",
                table: "StockReservation",
                column: "StockItemId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "StockReservation");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "StockItem");
        }
    }
}
