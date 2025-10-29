using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedStepUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStepUpTransaction",
                table: "RecurringTransactions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStepUpDate",
                table: "RecurringTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextStepUpDate",
                table: "RecurringTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StepUpAmount",
                table: "RecurringTransactions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StepUpFrequeny",
                table: "RecurringTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StepUpPercentage",
                table: "RecurringTransactions",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStepUpTransaction",
                table: "RecurringTransactions");

            migrationBuilder.DropColumn(
                name: "LastStepUpDate",
                table: "RecurringTransactions");

            migrationBuilder.DropColumn(
                name: "NextStepUpDate",
                table: "RecurringTransactions");

            migrationBuilder.DropColumn(
                name: "StepUpAmount",
                table: "RecurringTransactions");

            migrationBuilder.DropColumn(
                name: "StepUpFrequeny",
                table: "RecurringTransactions");

            migrationBuilder.DropColumn(
                name: "StepUpPercentage",
                table: "RecurringTransactions");
        }
    }
}
