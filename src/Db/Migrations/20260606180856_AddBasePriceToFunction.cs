using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Src.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddBasePriceToFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Functions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Functions");
        }
    }
}
