using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Src.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPaidToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Functions");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Reservations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Reservations");

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Functions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
