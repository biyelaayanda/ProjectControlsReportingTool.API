using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectControlsReportingTool.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPushNotificationSubscription_Fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PushNotificationSubscriptions_Users_UserId1",
                table: "PushNotificationSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_PushNotificationSubscriptions_UserId1",
                table: "PushNotificationSubscriptions");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "PushNotificationSubscriptions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "PushNotificationSubscriptions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationSubscriptions_UserId1",
                table: "PushNotificationSubscriptions",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PushNotificationSubscriptions_Users_UserId1",
                table: "PushNotificationSubscriptions",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
