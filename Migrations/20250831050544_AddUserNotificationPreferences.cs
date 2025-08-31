using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectControlsReportingTool.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PushEnabled = table.Column<bool>(type: "bit", nullable: false),
                    InAppEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SmsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PreferredTime = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FrequencyMinutes = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SubscribedTypes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecretKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AuthToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastTriggeredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    FailureCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationSubscriptions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HtmlTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TextTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    Variables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTemplates_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationTemplates_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RealTimeEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PushEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SmsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Schedule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinimumPriority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    QuietHoursStart = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    QuietHoursEnd = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActionText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsEmailSent = table.Column<bool>(type: "bit", nullable: false),
                    IsPushSent = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    NotificationTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_NotificationTemplates_NotificationTemplateId",
                        column: x => x.NotificationTemplateId,
                        principalTable: "NotificationTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Reports_RelatedReportId",
                        column: x => x.RelatedReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EmailQueue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToEmail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ToName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CcEmails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BccEmails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HtmlBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TextBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MessageId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Attachments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQueue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailQueue_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NotificationHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AdditionalDetails = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsError = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationHistory_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueue_NotificationId",
                table: "EmailQueue",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueue_Priority",
                table: "EmailQueue",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueue_Status_ScheduledDate",
                table: "EmailQueue",
                columns: new[] { "Status", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistory_NotificationId_CreatedDate",
                table: "NotificationHistory",
                columns: new[] { "NotificationId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId_NotificationType",
                table: "NotificationPreferences",
                columns: new[] { "UserId", "NotificationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotificationTemplateId",
                table: "Notifications",
                column: "NotificationTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId_ReadDate",
                table: "Notifications",
                columns: new[] { "RecipientId", "ReadDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedReportId",
                table: "Notifications",
                column: "RelatedReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SenderId",
                table: "Notifications",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Status",
                table: "Notifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type_CreatedDate",
                table: "Notifications",
                columns: new[] { "Type", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSubscriptions_CreatedBy",
                table: "NotificationSubscriptions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSubscriptions_IsActive",
                table: "NotificationSubscriptions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSubscriptions_WebhookUrl",
                table: "NotificationSubscriptions",
                column: "WebhookUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_CreatedBy",
                table: "NotificationTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_ModifiedBy",
                table: "NotificationTemplates",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Name",
                table: "NotificationTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Type_IsActive",
                table: "NotificationTemplates",
                columns: new[] { "Type", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_EmailEnabled",
                table: "UserNotificationPreferences",
                column: "EmailEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_NotificationType",
                table: "UserNotificationPreferences",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_RealTimeEnabled",
                table: "UserNotificationPreferences",
                column: "RealTimeEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_UserId",
                table: "UserNotificationPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_UserId_NotificationType",
                table: "UserNotificationPreferences",
                columns: new[] { "UserId", "NotificationType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailQueue");

            migrationBuilder.DropTable(
                name: "NotificationHistory");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "NotificationSubscriptions");

            migrationBuilder.DropTable(
                name: "UserNotificationPreferences");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");
        }
    }
}
