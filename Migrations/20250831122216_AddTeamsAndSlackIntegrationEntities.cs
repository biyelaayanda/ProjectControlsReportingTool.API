using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectControlsReportingTool.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamsAndSlackIntegrationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SlackWebhookConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DefaultChannel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultIconEmoji = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnabledNotificationsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UseAttachments = table.Column<bool>(type: "bit", nullable: false),
                    UseBlocks = table.Column<bool>(type: "bit", nullable: false),
                    CustomSettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlackWebhookConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlackWebhookConfigs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SlackIntegrationStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_SlackIntegrationStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlackIntegrationStats_SlackWebhookConfigs_WebhookConfigId",
                        column: x => x.WebhookConfigId,
                        principalTable: "SlackWebhookConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SlackIntegrationStats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SlackMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IconEmoji = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AttachmentsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlocksJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThreadTs = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnfurlLinks = table.Column<bool>(type: "bit", nullable: false),
                    UnfurlMedia = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_SlackMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlackMessages_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SlackMessages_SlackWebhookConfigs_WebhookConfigId",
                        column: x => x.WebhookConfigId,
                        principalTable: "SlackWebhookConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SlackMessages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SlackNotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    TextTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IconEmoji = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultAttachmentsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultBlocksJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UseAttachments = table.Column<bool>(type: "bit", nullable: false),
                    UseBlocks = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebhookConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlackNotificationTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlackNotificationTemplates_SlackWebhookConfigs_WebhookConfigId",
                        column: x => x.WebhookConfigId,
                        principalTable: "SlackWebhookConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SlackNotificationTemplates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SlackDeliveryFailures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    SlackMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlackDeliveryFailures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlackDeliveryFailures_SlackMessages_SlackMessageId",
                        column: x => x.SlackMessageId,
                        principalTable: "SlackMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SlackDeliveryFailures_SlackWebhookConfigs_WebhookConfigId",
                        column: x => x.WebhookConfigId,
                        principalTable: "SlackWebhookConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SlackDeliveryFailures_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SlackDeliveryFailures_SlackMessageId",
                table: "SlackDeliveryFailures",
                column: "SlackMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_SlackDeliveryFailures_UserId",
                table: "SlackDeliveryFailures",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SlackDeliveryFailures_WebhookConfigId",
                table: "SlackDeliveryFailures",
                column: "WebhookConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_SlackIntegrationStats_UserId",
                table: "SlackIntegrationStats",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SlackIntegrationStats_WebhookConfigId",
                table: "SlackIntegrationStats",
                column: "WebhookConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_SlackMessages_ReportId",
                table: "SlackMessages",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_SlackMessages_UserId",
                table: "SlackMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SlackMessages_WebhookConfigId",
                table: "SlackMessages",
                column: "WebhookConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_SlackNotificationTemplates_UserId",
                table: "SlackNotificationTemplates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SlackNotificationTemplates_WebhookConfigId",
                table: "SlackNotificationTemplates",
                column: "WebhookConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_SlackWebhookConfigs_UserId",
                table: "SlackWebhookConfigs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SlackDeliveryFailures");

            migrationBuilder.DropTable(
                name: "SlackIntegrationStats");

            migrationBuilder.DropTable(
                name: "SlackNotificationTemplates");

            migrationBuilder.DropTable(
                name: "SlackMessages");

            migrationBuilder.DropTable(
                name: "SlackWebhookConfigs");
        }
    }
}
