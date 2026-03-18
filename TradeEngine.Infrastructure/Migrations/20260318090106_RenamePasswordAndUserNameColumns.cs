using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamePasswordAndUserNameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "userName",
                table: "Accounts",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "passWord",
                table: "Accounts",
                newName: "PasswordHash");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_userName",
                table: "Accounts",
                newName: "IX_Accounts_UserName");

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledAt",
                table: "Trades",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Side",
                table: "Trades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "AssetSymbol",
                table: "Positions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "AverageCost",
                table: "Positions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Positions",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Positions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "AssetSymbol",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExecutedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FilledQuantity",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Side",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "StopPrice",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trades_AccountId",
                table: "Trades",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_OrderId",
                table: "Trades",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_AccountId_AssetSymbol",
                table: "Positions",
                columns: new[] { "AccountId", "AssetSymbol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AccountId_Status",
                table: "Orders",
                columns: new[] { "AccountId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AssetSymbol_Side_Status_Price",
                table: "Orders",
                columns: new[] { "AssetSymbol", "Side", "Status", "Price" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IdempotencyKey",
                table: "Orders",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_AccountId",
                table: "LedgerEntries",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_AccountId",
                table: "RefreshTokens",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Trades_AccountId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Trades_OrderId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Positions_AccountId_AssetSymbol",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AccountId_Status",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AssetSymbol_Side_Status_Price",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_IdempotencyKey",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_AccountId",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "SettledAt",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "Side",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "AverageCost",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "ExecutedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FilledQuantity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SettledAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Side",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StopPrice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Accounts",
                newName: "userName");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Accounts",
                newName: "passWord");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_UserName",
                table: "Accounts",
                newName: "IX_Accounts_userName");

            migrationBuilder.AlterColumn<string>(
                name: "AssetSymbol",
                table: "Positions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "AssetSymbol",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
