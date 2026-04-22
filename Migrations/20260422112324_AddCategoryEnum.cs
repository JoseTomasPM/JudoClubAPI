using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JudoClubAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "Students",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "Attended",
                table: "SesionStudents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Sesions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attended",
                table: "SesionStudents");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Sesions");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Students",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
