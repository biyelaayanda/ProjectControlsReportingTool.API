using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectControlsReportingTool.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReportTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExecutiveApprovedDate",
                table: "Reports",
                newName: "GMApprovedDate");

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId",
                table: "Reports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReportTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContentTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultPriority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefaultDepartment = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemTemplate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    DefaultTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DefaultDueDays = table.Column<int>(type: "int", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportTemplates_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportTemplates_Users_LastModifiedBy",
                        column: x => x.LastModifiedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TemplateId",
                table: "Reports",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_CreatedBy",
                table: "ReportTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_LastModifiedBy",
                table: "ReportTemplates",
                column: "LastModifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ReportTemplates_TemplateId",
                table: "Reports",
                column: "TemplateId",
                principalTable: "ReportTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ReportTemplates_TemplateId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "ReportTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Reports_TemplateId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "GMApprovedDate",
                table: "Reports",
                newName: "ExecutiveApprovedDate");
        }
    }
}
