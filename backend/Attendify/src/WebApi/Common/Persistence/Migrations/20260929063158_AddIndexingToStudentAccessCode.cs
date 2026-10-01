using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Attendify.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexingToStudentAccessCode : Migration
    {
        private static readonly string[] IndexColumns = { "UserId", "GenerationDate" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentAccessCodes_UserId",
                table: "StudentAccessCodes");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAccessCodes_UserId_GenerationDate",
                table: "StudentAccessCodes",
                columns: IndexColumns,
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
