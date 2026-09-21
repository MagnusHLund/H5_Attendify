using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Attendify.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedEducationalInstitutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Code",
                table: "AdminAccessCodes",
                newName: "HashedAccessCode");

            migrationBuilder.InsertData(
                table: "EducationalInstitutes",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("8d6f2f7b-6c1b-4f6d-9f43-4e0e6c4c1a11"), new DateTimeOffset(new DateTime(2026, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "ZBC - Ringsted", new DateTimeOffset(new DateTime(2026, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("f2b9a3c8-1e74-4c2a-8d91-7b5e6f3a2d22"), new DateTimeOffset(new DateTime(2026, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "ZBC - Slagelse", new DateTimeOffset(new DateTime(2026, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EducationalInstitutes",
                keyColumn: "Id",
                keyValue: new Guid("8d6f2f7b-6c1b-4f6d-9f43-4e0e6c4c1a11"));

            migrationBuilder.DeleteData(
                table: "EducationalInstitutes",
                keyColumn: "Id",
                keyValue: new Guid("f2b9a3c8-1e74-4c2a-8d91-7b5e6f3a2d22"));

            migrationBuilder.RenameColumn(
                name: "HashedAccessCode",
                table: "AdminAccessCodes",
                newName: "Code");
        }
    }
}
