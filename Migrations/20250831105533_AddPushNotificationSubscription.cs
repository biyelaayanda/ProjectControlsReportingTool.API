using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectControlsReportingTool.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPushNotificationSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PushNotificationSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    P256dhKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AuthToken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuccessfulNotifications = table.Column<int>(type: "int", nullable: false),
                    FailedNotifications = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    BrowserInfo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HasPermission = table.Column<bool>(type: "bit", nullable: false),
                    EnabledForReports = table.Column<bool>(type: "bit", nullable: false),
                    EnabledForApprovals = table.Column<bool>(type: "bit", nullable: false),
                    EnabledForDeadlines = table.Column<bool>(type: "bit", nullable: false),
                    EnabledForAnnouncements = table.Column<bool>(type: "bit", nullable: false),
                    EnabledForMentions = table.Column<bool>(type: "bit", nullable: false),
                    EnabledForReminders = table.Column<bool>(type: "bit", nullable: false),
                    MinimumPriority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushNotificationSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushNotificationSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PushNotificationSubscriptions_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationSubscriptions_DeviceType",
                table: "PushNotificationSubscriptions",
                column: "DeviceType");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationSubscriptions_DeviceType_IsActive",
                table: "PushNotificationSubscriptions",
                columns: new[] { "DeviceType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationSubscriptions_Endpoint",
                table: "PushNotificationSubscriptions",
                column: "Endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationSubscriptions_ExpiresAt",
                table: "PushNotificationSubscriptions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationSubscriptions_HasPermission",
                table: "PushNotificationSubscriptions",
                column: "HasPermission");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationSubscriptions_IsActive",
                table: "PushNotificationSubscriptions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationSubscriptions_LastUsed",
                table: "PushNotificationSubscriptions",
                column: "LastUsed");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationSubscriptions_UserId",
                table: "PushNotificationSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationSubscriptions_UserId_IsActive",
                table: "PushNotificationSubscriptions",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationSubscriptions_UserId1",
                table: "PushNotificationSubscriptions",
                column: "UserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PushNotificationSubscriptions");
        }
    }
}
