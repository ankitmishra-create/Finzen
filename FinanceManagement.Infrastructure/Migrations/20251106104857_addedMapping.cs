using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions");

            migrationBuilder.CreateTable(
                name: "SavingsOrBudgetsMappings",
                columns: table => new
                {
                    TransactionSavingsOrBudgetsMappingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SavingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BudgetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SavedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsOrBudgetsMappings", x => x.TransactionSavingsOrBudgetsMappingId);
                    table.ForeignKey(
                        name: "FK_SavingsOrBudgetsMappings_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "BudgetId");
                    table.ForeignKey(
                        name: "FK_SavingsOrBudgetsMappings_Savings_SavingId",
                        column: x => x.SavingId,
                        principalTable: "Savings",
                        principalColumn: "SavingId");
                    table.ForeignKey(
                        name: "FK_SavingsOrBudgetsMappings_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavingsOrBudgetsMappings_BudgetId",
                table: "SavingsOrBudgetsMappings",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsOrBudgetsMappings_SavingId",
                table: "SavingsOrBudgetsMappings",
                column: "SavingId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsOrBudgetsMappings_TransactionId",
                table: "SavingsOrBudgetsMappings",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "SavingsOrBudgetsMappings");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
