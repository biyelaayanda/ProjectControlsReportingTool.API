using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectControlsReportingTool.API.Migrations
{
    /// <inheritdoc />
    public partial class TeamsIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamsWebhookConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnabledNotificationsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DefaultFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DefaultThemeColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CustomSettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamsWebhookConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamsWebhookConfigs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamsIntegrationStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    TotalMessages = table.Column<int>(type: "int", nullable: false),
                    SuccessfulDeliveries = table.Column<int>(type: "int", nullable: false),
                    FailedDeliveries = table.Column<int>(type: "int", nullable: false),
                    AverageResponseTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    MaxResponseTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    MinResponseTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    LastMessageSent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorSummaryJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebhookConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamsIntegrationStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamsIntegrationStats_TeamsWebhookConfigs_WebhookConfigId",
                        column: x => x.WebhookConfigId,
                        principalTable: "TeamsWebhookConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamsIntegrationStats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeamsMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ThemeColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ActionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UseAdaptiveCard = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponseTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    MessageId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WebhookConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamsMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamsMessages_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamsMessages_TeamsWebhookConfigs_WebhookConfigId",
                        column: x => x.WebhookConfigId,
                        principalTable: "TeamsWebhookConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamsMessages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeamsNotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    TitleTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MessageTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DefaultActionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UseAdaptiveCard = table.Column<bool>(type: "bit", nullable: false),
                    DefaultFactsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebhookConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamsNotificationTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamsNotificationTemplates_TeamsWebhookConfigs_WebhookConfigId",
                        column: x => x.WebhookConfigId,
                        principalTable: "TeamsWebhookConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamsNotificationTemplates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamsDeliveryFailures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginalPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebhookConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TeamsMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamsDeliveryFailures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamsDeliveryFailures_TeamsMessages_TeamsMessageId",
                        column: x => x.TeamsMessageId,
                        principalTable: "TeamsMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamsDeliveryFailures_TeamsWebhookConfigs_WebhookConfigId",
                        column: x => x.WebhookConfigId,
                        principalTable: "TeamsWebhookConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamsDeliveryFailures_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamsDeliveryFailures_TeamsMessageId",
                table: "TeamsDeliveryFailures",
                column: "TeamsMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamsDeliveryFailures_UserId",
                table: "TeamsDeliveryFailures",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamsDeliveryFailures_WebhookConfigId",
                table: "TeamsDeliveryFailures",
                column: "WebhookConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamsIntegrationStats_UserId",
                table: "TeamsIntegrationStats",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamsIntegrationStats_WebhookConfigId",
                table: "TeamsIntegrationStats",
                column: "WebhookConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamsMessages_ReportId",
                table: "TeamsMessages",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamsMessages_UserId",
                table: "TeamsMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamsMessages_WebhookConfigId",
                table: "TeamsMessages",
                column: "WebhookConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamsNotificationTemplates_UserId",
                table: "TeamsNotificationTemplates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamsNotificationTemplates_WebhookConfigId",
                table: "TeamsNotificationTemplates",
                column: "WebhookConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamsWebhookConfigs_UserId",
                table: "TeamsWebhookConfigs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamsDeliveryFailures");

            migrationBuilder.DropTable(
                name: "TeamsIntegrationStats");

            migrationBuilder.DropTable(
                name: "TeamsNotificationTemplates");

            migrationBuilder.DropTable(
                name: "TeamsMessages");

            migrationBuilder.DropTable(
                name: "TeamsWebhookConfigs");
        }
    }
}
