using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Attendify.Common.Persistence.Migrations;

public partial class AddRefreshTokenFamilies : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "TokenFamilyId",
            table: "RefreshTokens",
            type: "uuid",
            nullable: true
        );

        // Existing refresh tokens for a user are treated as one family so a
        // logout using an older rotated token can revoke its current replacement.
        migrationBuilder.Sql(
            """
            WITH token_families AS (
                SELECT "UserId", gen_random_uuid() AS "TokenFamilyId"
                FROM "RefreshTokens"
                GROUP BY "UserId"
            )
            UPDATE "RefreshTokens" AS tokens
            SET "TokenFamilyId" = token_families."TokenFamilyId"
            FROM token_families
            WHERE tokens."UserId" = token_families."UserId";
            """
        );

        migrationBuilder.AlterColumn<Guid>(
            name: "TokenFamilyId",
            table: "RefreshTokens",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_TokenFamilyId",
            table: "RefreshTokens",
            column: "TokenFamilyId"
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_RefreshTokens_TokenFamilyId",
            table: "RefreshTokens"
        );

        migrationBuilder.DropColumn(name: "TokenFamilyId", table: "RefreshTokens");
    }
}
