using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectControlsReportingTool.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TemplateType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlainTextContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemTemplate = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Variables = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PreviewData = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailTemplates_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmailTemplates_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Category",
                table: "EmailTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_CreatedAt",
                table: "EmailTemplates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_CreatedBy",
                table: "EmailTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_IsActive",
                table: "EmailTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_IsSystemTemplate",
                table: "EmailTemplates",
                column: "IsSystemTemplate");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_LastUsed",
                table: "EmailTemplates",
                column: "LastUsed");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Name",
                table: "EmailTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_TemplateType",
                table: "EmailTemplates",
                column: "TemplateType");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_UpdatedBy",
                table: "EmailTemplates",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_UsageCount",
                table: "EmailTemplates",
                column: "UsageCount");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Version",
                table: "EmailTemplates",
                column: "Version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailTemplates");
        }
    }
}
