using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Attendify.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangedDataTypeOfRefreshTokenHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "TokenHash", table: "RefreshTokens");

            migrationBuilder.AddColumn<byte[]>(
                name: "TokenHash",
                table: "RefreshTokens",
                type: "bytea",
                maxLength: 32,
                nullable: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "TokenHash", table: "RefreshTokens");

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "RefreshTokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false
            );
        }
    }
}
