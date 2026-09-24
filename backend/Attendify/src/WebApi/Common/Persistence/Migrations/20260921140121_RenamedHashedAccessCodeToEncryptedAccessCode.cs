using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Attendify.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenamedHashedAccessCodeToEncryptedAccessCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HashedAccessCode",
                table: "AdminAccessCodes",
                newName: "EncryptedAccessCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EncryptedAccessCode",
                table: "AdminAccessCodes",
                newName: "HashedAccessCode");
        }
    }
}
