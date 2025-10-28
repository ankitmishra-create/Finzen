using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatedSelectedCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedCurrency",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedCurrency",
                table: "Transactions");
        }
    }
}
