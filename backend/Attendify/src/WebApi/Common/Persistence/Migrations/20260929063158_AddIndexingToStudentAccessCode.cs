using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Attendify.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexingToStudentAccessCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentAccessCodes_UserId",
                table: "StudentAccessCodes");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAccessCodes_UserId_GenerationDate",
                table: "StudentAccessCodes",
                columns: new[] { "UserId", "GenerationDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentAccessCodes_UserId_GenerationDate",
                table: "StudentAccessCodes");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAccessCodes_UserId",
                table: "StudentAccessCodes",
                column: "UserId");
        }
    }
}
