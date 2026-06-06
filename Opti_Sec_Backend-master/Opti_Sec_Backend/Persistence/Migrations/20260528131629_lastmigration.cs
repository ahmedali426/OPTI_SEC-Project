using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opti_Sec_Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class lastmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AITrainingStatus",
                table: "Members",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FaceEmbedding",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTrainedAt",
                table: "Members",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceApiKey",
                table: "Gates",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "Gates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxFailedAttempts",
                table: "Gates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Gates",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SilentAlarmHash",
                table: "Gates",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Gates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FcmToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccessMethod",
                table: "AccessLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GateSessionId",
                table: "AccessLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSilentAlarm",
                table: "AccessLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GateSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GateId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    IsSilentAlarm = table.Column<bool>(type: "bit", nullable: false),
                    PasswordValidatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordPassed = table.Column<bool>(type: "bit", nullable: false),
                    AIValidatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AIPassed = table.Column<bool>(type: "bit", nullable: false),
                    AIConfidenceScore = table.Column<double>(type: "float", nullable: true),
                    AIAttemptCount = table.Column<int>(type: "int", nullable: false),
                    FingerprintValidatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FingerprintPassed = table.Column<bool>(type: "bit", nullable: false),
                    FingerprintAttemptCount = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Result = table.Column<int>(type: "int", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GateSessions_Gates_GateId",
                        column: x => x.GateId,
                        principalTable: "Gates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GateSessions_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AIValidationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateSessionId = table.Column<int>(type: "int", nullable: false),
                    GateId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsAuthorized = table.Column<bool>(type: "bit", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float(5)", precision: 5, scale: 2, nullable: false),
                    MatchedMemberId = table.Column<int>(type: "int", nullable: true),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponseTimeMs = table.Column<int>(type: "int", nullable: false),
                    AIRawResponseJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIValidationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIValidationLogs_GateSessions_GateSessionId",
                        column: x => x.GateSessionId,
                        principalTable: "GateSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AIValidationLogs_Gates_GateId",
                        column: x => x.GateId,
                        principalTable: "Gates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AIValidationLogs_Members_MatchedMemberId",
                        column: x => x.MatchedMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DeviceCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GateSessionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCommands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceCommands_AspNetUsers_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeviceCommands_GateSessions_GateSessionId",
                        column: x => x.GateSessionId,
                        principalTable: "GateSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceCommands_Gates_GateId",
                        column: x => x.GateId,
                        principalTable: "Gates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BuzzerActivated = table.Column<bool>(type: "bit", nullable: false),
                    BuzzerDurationSeconds = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GateSessionId = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyEvents_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EmergencyEvents_GateSessions_GateSessionId",
                        column: x => x.GateSessionId,
                        principalTable: "GateSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyEvents_Gates_GateId",
                        column: x => x.GateId,
                        principalTable: "Gates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FingerprintValidationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateSessionId = table.Column<int>(type: "int", nullable: false),
                    GateId = table.Column<int>(type: "int", nullable: false),
                    ExpectedMemberId = table.Column<int>(type: "int", nullable: true),
                    FingerprintTemplateHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsMatch = table.Column<bool>(type: "bit", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FingerprintValidationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FingerprintValidationLogs_GateSessions_GateSessionId",
                        column: x => x.GateSessionId,
                        principalTable: "GateSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FingerprintValidationLogs_Gates_GateId",
                        column: x => x.GateId,
                        principalTable: "Gates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FingerprintValidationLogs_Members_ExpectedMemberId",
                        column: x => x.ExpectedMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GateId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DataJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsSent = table.Column<bool>(type: "bit", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GateSessionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_GateSessions_GateSessionId",
                        column: x => x.GateSessionId,
                        principalTable: "GateSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Gates_GateId",
                        column: x => x.GateId,
                        principalTable: "Gates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasswordAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PasswordHashAttempt = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    TriggeredEmergency = table.Column<bool>(type: "bit", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GateSessionId = table.Column<int>(type: "int", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordAttempts_GateSessions_GateSessionId",
                        column: x => x.GateSessionId,
                        principalTable: "GateSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PasswordAttempts_Gates_GateId",
                        column: x => x.GateId,
                        principalTable: "Gates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PasswordAttempts_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                columns: new[] { "FcmToken", "PasswordHash" },
                values: new object[] { null, "AQAAAAIAAYagAAAAENihKU2tvyGgP142z/30UMuf23zx+E9s8a1R+K0WIqKmj/gVpaAIBOr8FRLRwPQ3MA==" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_GateSessionId",
                table: "AccessLogs",
                column: "GateSessionId",
                unique: true,
                filter: "[GateSessionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AIValidationLogs_GateId",
                table: "AIValidationLogs",
                column: "GateId");

            migrationBuilder.CreateIndex(
                name: "IX_AIValidationLogs_GateSessionId",
                table: "AIValidationLogs",
                column: "GateSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AIValidationLogs_MatchedMemberId",
                table: "AIValidationLogs",
                column: "MatchedMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_GateId",
                table: "DeviceCommands",
                column: "GateId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_GateSessionId",
                table: "DeviceCommands",
                column: "GateSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_IssuedByUserId",
                table: "DeviceCommands",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyEvents_GateId_IsResolved",
                table: "EmergencyEvents",
                columns: new[] { "GateId", "IsResolved" });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyEvents_GateSessionId",
                table: "EmergencyEvents",
                column: "GateSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyEvents_ResolvedByUserId",
                table: "EmergencyEvents",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FingerprintValidationLogs_ExpectedMemberId",
                table: "FingerprintValidationLogs",
                column: "ExpectedMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_FingerprintValidationLogs_GateId",
                table: "FingerprintValidationLogs",
                column: "GateId");

            migrationBuilder.CreateIndex(
                name: "IX_FingerprintValidationLogs_GateSessionId",
                table: "FingerprintValidationLogs",
                column: "GateSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GateSessions_GateId",
                table: "GateSessions",
                column: "GateId");

            migrationBuilder.CreateIndex(
                name: "IX_GateSessions_MemberId",
                table: "GateSessions",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_GateSessions_SessionToken",
                table: "GateSessions",
                column: "SessionToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GateId",
                table: "Notifications",
                column: "GateId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GateSessionId",
                table: "Notifications",
                column: "GateSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsSent_RetryCount",
                table: "Notifications",
                columns: new[] { "IsSent", "RetryCount" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "RecipientUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordAttempts_GateId_AttemptedAt",
                table: "PasswordAttempts",
                columns: new[] { "GateId", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordAttempts_GateSessionId",
                table: "PasswordAttempts",
                column: "GateSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordAttempts_MemberId",
                table: "PasswordAttempts",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_GateSessions_GateSessionId",
                table: "AccessLogs",
                column: "GateSessionId",
                principalTable: "GateSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_GateSessions_GateSessionId",
                table: "AccessLogs");

            migrationBuilder.DropTable(
                name: "AIValidationLogs");

            migrationBuilder.DropTable(
                name: "DeviceCommands");

            migrationBuilder.DropTable(
                name: "EmergencyEvents");

            migrationBuilder.DropTable(
                name: "FingerprintValidationLogs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PasswordAttempts");

            migrationBuilder.DropTable(
                name: "GateSessions");

            migrationBuilder.DropIndex(
                name: "IX_AccessLogs_GateSessionId",
                table: "AccessLogs");

            migrationBuilder.DropColumn(
                name: "AITrainingStatus",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "FaceEmbedding",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "LastTrainedAt",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "DeviceApiKey",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "MaxFailedAttempts",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "SilentAlarmHash",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "FcmToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AccessMethod",
                table: "AccessLogs");

            migrationBuilder.DropColumn(
                name: "GateSessionId",
                table: "AccessLogs");

            migrationBuilder.DropColumn(
                name: "IsSilentAlarm",
                table: "AccessLogs");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEFLE3hEtOBQsQZnTjEqiD6xwRq6DBpPKUDVLfOWY/WN0cxCbcNmPDfPhRbHVl8h4ew==");
        }
    }
}
