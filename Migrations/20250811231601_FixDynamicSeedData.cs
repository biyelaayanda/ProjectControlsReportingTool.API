using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectControlsReportingTool.API.Migrations
{
    /// <inheritdoc />
    public partial class FixDynamicSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("ab986eab-b558-466c-8684-cb48c97cfe96"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedDate", "Department", "Email", "FirstName", "IsActive", "JobTitle", "LastLoginDate", "LastName", "PasswordHash", "PasswordSalt", "PhoneNumber", "Role" },
                values: new object[] { new Guid("12345678-1234-5678-9012-123456789012"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "admin@projectcontrols.com", "System", true, "System Administrator", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Administrator", "$2a$11$8ZPNVUXQQDDjwGQFAEZ8LuLuVsS1tU.HtPdDEo9p8tYl7Z2kZcZQW", "salt", null, 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("12345678-1234-5678-9012-123456789012"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedDate", "Department", "Email", "FirstName", "IsActive", "JobTitle", "LastLoginDate", "LastName", "PasswordHash", "PasswordSalt", "PhoneNumber", "Role" },
                values: new object[] { new Guid("ab986eab-b558-466c-8684-cb48c97cfe96"), new DateTime(2025, 8, 11, 23, 14, 36, 750, DateTimeKind.Utc).AddTicks(5982), 1, "admin@projectcontrols.com", "System", true, "System Administrator", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Administrator", "$2a$11$8ZPNVUXQQDDjwGQFAEZ8LuLuVsS1tU.HtPdDEo9p8tYl7Z2kZcZQW", "salt", null, 3 });
        }
    }
}
