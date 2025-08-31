using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectControlsReportingTool.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SmsStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Provider = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    MessagesSent = table.Column<int>(type: "int", nullable: false),
                    MessagesDelivered = table.Column<int>(type: "int", nullable: false),
                    MessagesFailed = table.Column<int>(type: "int", nullable: false),
                    MessagesPending = table.Column<int>(type: "int", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    AverageDeliveryTime = table.Column<double>(type: "float", nullable: false),
                    DeliveryRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    UniqueRecipients = table.Column<int>(type: "int", nullable: false),
                    MessagesByType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorBreakdown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryStats = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmsTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Template = table.Column<string>(type: "nvarchar(1600)", maxLength: 1600, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemTemplate = table.Column<bool>(type: "bit", nullable: false),
                    Variables = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DefaultPriority = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    DefaultMessageType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    DefaultIsUrgent = table.Column<bool>(type: "bit", nullable: false),
                    MaxRenderedLength = table.Column<int>(type: "int", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsTemplates_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SmsTemplates_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SmsMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalMessageId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Recipient = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1600)", maxLength: 1600, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    MessageType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    Provider = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SegmentCount = table.Column<int>(type: "int", nullable: false),
                    Encoding = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    CountryCode = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeliveryAttempts = table.Column<int>(type: "int", nullable: false),
                    LastDeliveryAttempt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeliveryReceiptRequested = table.Column<bool>(type: "bit", nullable: false),
                    IsSensitive = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsMessages_Reports_RelatedReportId",
                        column: x => x.RelatedReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SmsMessages_SmsTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "SmsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SmsMessages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SmsMessages_BatchId",
                table: "SmsMessages",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsMessages_ExternalMessageId",
                table: "SmsMessages",
                column: "ExternalMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsMessages_Provider_SentAt",
                table: "SmsMessages",
                columns: new[] { "Provider", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsMessages_Recipient",
                table: "SmsMessages",
                column: "Recipient");

            migrationBuilder.CreateIndex(
                name: "IX_SmsMessages_RelatedReportId",
                table: "SmsMessages",
                column: "RelatedReportId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsMessages_SentAt",
                table: "SmsMessages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_SmsMessages_Status",
                table: "SmsMessages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SmsMessages_Status_IsUrgent",
                table: "SmsMessages",
                columns: new[] { "Status", "IsUrgent" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsMessages_TemplateId",
                table: "SmsMessages",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsMessages_UserId_SentAt",
                table: "SmsMessages",
                columns: new[] { "UserId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsStatistics_Date",
                table: "SmsStatistics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_SmsStatistics_Date_Provider",
                table: "SmsStatistics",
                columns: new[] { "Date", "Provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmsStatistics_Provider",
                table: "SmsStatistics",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplates_Category",
                table: "SmsTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplates_Category_IsActive",
                table: "SmsTemplates",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplates_CreatedBy",
                table: "SmsTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplates_IsActive",
                table: "SmsTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplates_IsSystemTemplate",
                table: "SmsTemplates",
                column: "IsSystemTemplate");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplates_Name",
                table: "SmsTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplates_UpdatedBy",
                table: "SmsTemplates",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmsMessages");

            migrationBuilder.DropTable(
                name: "SmsStatistics");

            migrationBuilder.DropTable(
                name: "SmsTemplates");
        }
    }
}
