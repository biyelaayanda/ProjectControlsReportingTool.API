using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectControlsReportingTool.API.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalStageToAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalStage",
                table: "ReportAttachments",
                type: "int",
                nullable: false,
                defaultValue: 1); // Default to Initial stage

            migrationBuilder.AddColumn<string>(
                name: "UploadedByName",
                table: "ReportAttachments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UploadedByRole",
                table: "ReportAttachments",
                type: "int",
                nullable: false,
                defaultValue: 1); // Default to GeneralStaff role
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStage",
                table: "ReportAttachments");

            migrationBuilder.DropColumn(
                name: "UploadedByName",
                table: "ReportAttachments");

            migrationBuilder.DropColumn(
                name: "UploadedByRole",
                table: "ReportAttachments");
        }
    }
}
