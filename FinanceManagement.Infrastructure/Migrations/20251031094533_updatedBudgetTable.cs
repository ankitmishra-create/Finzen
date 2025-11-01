using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatedBudgetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomBudgetEndDate",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "CustomBudgetStartDate",
                table: "Budgets");

            migrationBuilder.AddColumn<DateTime>(
                name: "BudgetEndDate",
                table: "Budgets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "BudgetStartDate",
                table: "Budgets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BudgetEndDate",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "BudgetStartDate",
                table: "Budgets");

            migrationBuilder.AddColumn<DateTime>(
                name: "CustomBudgetEndDate",
                table: "Budgets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CustomBudgetStartDate",
                table: "Budgets",
                type: "datetime2",
                nullable: true);
        }
    }
}
