using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PharmacyApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CATEGORY",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORY", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "EMPLOYEE",
                columns: table => new
                {
                    EID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMPLOYEE", x => x.EID);
                });

            migrationBuilder.CreateTable(
                name: "ROLE",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLE", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "SUPPLIER",
                columns: table => new
                {
                    SupplierID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Contact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUPPLIER", x => x.SupplierID);
                });

            migrationBuilder.CreateTable(
                name: "MEDICINE",
                columns: table => new
                {
                    DrugID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DrugName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    GenericName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CategoryID = table.Column<int>(type: "integer", nullable: true),
                    ReorderLevel = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MEDICINE", x => x.DrugID);
                    table.ForeignKey(
                        name: "FK_MEDICINE_CATEGORY_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "CATEGORY",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AUDIT_LOG",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecordID = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true),
                    ActionBy = table.Column<int>(type: "integer", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUDIT_LOG", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_AUDIT_LOG_EMPLOYEE_ActionBy",
                        column: x => x.ActionBy,
                        principalTable: "EMPLOYEE",
                        principalColumn: "EID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SALES_ORDER",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EID = table.Column<int>(type: "integer", nullable: false),
                    CustomerInfo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALES_ORDER", x => x.OrderID);
                    table.ForeignKey(
                        name: "FK_SALES_ORDER_EMPLOYEE_EID",
                        column: x => x.EID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "EID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EMPLOYEE_ROLE",
                columns: table => new
                {
                    EID = table.Column<int>(type: "integer", nullable: false),
                    RoleID = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMPLOYEE_ROLE", x => new { x.EID, x.RoleID });
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_ROLE_EMPLOYEE_EID",
                        column: x => x.EID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "EID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_ROLE_ROLE_RoleID",
                        column: x => x.RoleID,
                        principalTable: "ROLE",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "INVENTORY_BATCH",
                columns: table => new
                {
                    BatchID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DrugID = table.Column<int>(type: "integer", nullable: false),
                    SupplierID = table.Column<int>(type: "integer", nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QuantityInStock = table.Column<int>(type: "integer", nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    SellingPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MfgDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INVENTORY_BATCH", x => x.BatchID);
                    table.ForeignKey(
                        name: "FK_INVENTORY_BATCH_MEDICINE_DrugID",
                        column: x => x.DrugID,
                        principalTable: "MEDICINE",
                        principalColumn: "DrugID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_INVENTORY_BATCH_SUPPLIER_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "SUPPLIER",
                        principalColumn: "SupplierID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SALES_ORDER_ITEM",
                columns: table => new
                {
                    OrderItemID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderID = table.Column<int>(type: "integer", nullable: false),
                    BatchID = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALES_ORDER_ITEM", x => x.OrderItemID);
                    table.ForeignKey(
                        name: "FK_SALES_ORDER_ITEM_INVENTORY_BATCH_BatchID",
                        column: x => x.BatchID,
                        principalTable: "INVENTORY_BATCH",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SALES_ORDER_ITEM_SALES_ORDER_OrderID",
                        column: x => x.OrderID,
                        principalTable: "SALES_ORDER",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STOCK_TRANSACTION",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchID = table.Column<int>(type: "integer", nullable: false),
                    EID = table.Column<int>(type: "integer", nullable: false),
                    TransType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReferenceNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STOCK_TRANSACTION", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK_STOCK_TRANSACTION_EMPLOYEE_EID",
                        column: x => x.EID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "EID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STOCK_TRANSACTION_INVENTORY_BATCH_BatchID",
                        column: x => x.BatchID,
                        principalTable: "INVENTORY_BATCH",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ROLE",
                columns: new[] { "RoleID", "CreatedAt", "IsActive", "RoleName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Admin", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Pharmacist", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "StockEmployee", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AUDIT_LOG_ActionBy",
                table: "AUDIT_LOG",
                column: "ActionBy");

            migrationBuilder.CreateIndex(
                name: "IX_AUDIT_LOG_ActionDate",
                table: "AUDIT_LOG",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_AUDIT_LOG_TableName_RecordID",
                table: "AUDIT_LOG",
                columns: new[] { "TableName", "RecordID" });

            migrationBuilder.CreateIndex(
                name: "IX_EMPLOYEE_Email",
                table: "EMPLOYEE",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EMPLOYEE_ROLE_RoleID",
                table: "EMPLOYEE_ROLE",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORY_BATCH_DrugID",
                table: "INVENTORY_BATCH",
                column: "DrugID");

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORY_BATCH_ExpDate",
                table: "INVENTORY_BATCH",
                column: "ExpDate");

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORY_BATCH_SupplierID",
                table: "INVENTORY_BATCH",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_MEDICINE_Barcode",
                table: "MEDICINE",
                column: "Barcode");

            migrationBuilder.CreateIndex(
                name: "IX_MEDICINE_CategoryID",
                table: "MEDICINE",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_MEDICINE_DrugName",
                table: "MEDICINE",
                column: "DrugName");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDER_CreatedAt",
                table: "SALES_ORDER",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDER_EID",
                table: "SALES_ORDER",
                column: "EID");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDER_ITEM_BatchID",
                table: "SALES_ORDER_ITEM",
                column: "BatchID");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDER_ITEM_OrderID",
                table: "SALES_ORDER_ITEM",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_STOCK_TRANSACTION_BatchID",
                table: "STOCK_TRANSACTION",
                column: "BatchID");

            migrationBuilder.CreateIndex(
                name: "IX_STOCK_TRANSACTION_CreatedAt",
                table: "STOCK_TRANSACTION",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_STOCK_TRANSACTION_EID",
                table: "STOCK_TRANSACTION",
                column: "EID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AUDIT_LOG");

            migrationBuilder.DropTable(
                name: "EMPLOYEE_ROLE");

            migrationBuilder.DropTable(
                name: "SALES_ORDER_ITEM");

            migrationBuilder.DropTable(
                name: "STOCK_TRANSACTION");

            migrationBuilder.DropTable(
                name: "ROLE");

            migrationBuilder.DropTable(
                name: "SALES_ORDER");

            migrationBuilder.DropTable(
                name: "INVENTORY_BATCH");

            migrationBuilder.DropTable(
                name: "EMPLOYEE");

            migrationBuilder.DropTable(
                name: "MEDICINE");

            migrationBuilder.DropTable(
                name: "SUPPLIER");

            migrationBuilder.DropTable(
                name: "CATEGORY");
        }
    }
}
