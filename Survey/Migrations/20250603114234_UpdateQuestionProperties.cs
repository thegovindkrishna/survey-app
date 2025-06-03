using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Survey.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestionProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Question",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Required",
                table: "Question",
                newName: "required");

            migrationBuilder.RenameColumn(
                name: "Options",
                table: "Question",
                newName: "options");

            migrationBuilder.RenameColumn(
                name: "MaxRating",
                table: "Question",
                newName: "maxRating");

            migrationBuilder.AlterColumn<string>(
                name: "options",
                table: "Question",
                type: "json",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "type",
                table: "Question",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "required",
                table: "Question",
                newName: "Required");

            migrationBuilder.RenameColumn(
                name: "options",
                table: "Question",
                newName: "Options");

            migrationBuilder.RenameColumn(
                name: "maxRating",
                table: "Question",
                newName: "MaxRating");

            migrationBuilder.AlterColumn<string>(
                name: "Options",
                table: "Question",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
