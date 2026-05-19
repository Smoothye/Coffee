using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingPlannerApp.Migrations
{
    /// <inheritdoc />
    public partial class SyncVenueModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventSuppliers_Suppliers_SupplierId",
                table: "EventSuppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Suppliers_SupplierId",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Expenses",
                newName: "EstimatedAmount");

            migrationBuilder.AddColumn<bool>(
                name: "IsHeadTable",
                table: "WeddingTables",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Venues",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Venues",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Venues",
                type: "TEXT",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Venues",
                type: "TEXT",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Venues",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tags",
                table: "Venues",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DietaryType",
                table: "Menus",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasPlusOne",
                table: "Guests",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SeatNumber",
                table: "Guests",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualAmount",
                table: "Expenses",
                type: "TEXT",
                precision: 8,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ExpenseCategory",
                table: "Expenses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrideName",
                table: "Events",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroomName",
                table: "Events",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "CheckListTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventSuppliers_Suppliers_SupplierId",
                table: "EventSuppliers",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Suppliers_SupplierId",
                table: "Expenses",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventSuppliers_Suppliers_SupplierId",
                table: "EventSuppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Suppliers_SupplierId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "IsHeadTable",
                table: "WeddingTables");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "DietaryType",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "HasPlusOne",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "SeatNumber",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "ActualAmount",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ExpenseCategory",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "BrideName",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "GroomName",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "CheckListTasks");

            migrationBuilder.RenameColumn(
                name: "EstimatedAmount",
                table: "Expenses",
                newName: "Amount");

            migrationBuilder.AddForeignKey(
                name: "FK_EventSuppliers_Suppliers_SupplierId",
                table: "EventSuppliers",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Suppliers_SupplierId",
                table: "Expenses",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
